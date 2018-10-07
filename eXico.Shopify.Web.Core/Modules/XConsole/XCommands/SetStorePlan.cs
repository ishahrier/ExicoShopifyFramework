using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConsoleTables;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class SetStorePlan : IXCommand
    {
        public string GetName()
        {
            return "set-store-plan";
        }

        public string GetDescription()
        {
            return "Updates store's/account's plan id.";
        }

        public async Task Run(XConsole xc)
        {
            try
            {
                xc.AskForInput(this, "Enter store my shopify domain (exact URL): ");
                var store = Console.ReadLine();
                xc.AskForInput(this, "Enter plan id: ");
                var planId = Int32.Parse(Console.ReadLine());
                AspNetUser user = null;
                Plan plan = null;
                using (var scope = xc.WebHost.Services.CreateScope())
                {
                    var userService = scope.ServiceProvider.GetService<IDbService<AspNetUser>>();
                    user = userService.FindSingleWhere(x => x.MyShopifyDomain == store);
                    if (user == null)
                    {
                        xc.WriteError(this, "Store not found.");
                    }
                    else
                    {
                        var planService = scope.ServiceProvider.GetService<IDbService<Plan>>();
                        plan = planService.FindSingleWhere(x => x.Id == planId);
                        if (plan == null)
                        {
                            xc.WriteError(this, "Plan not found.");
                        }
                        else
                        {
                            xc.WriteSuccess(this, $"Found the plan {plan.Name}.");
                            var prevPlanId = user.PlanId;
                            user.PlanId = plan.Id;
                            xc.WriteInfo(this, "Updating plan id for the store.");
                            var updatedUser = userService.Update(user, user.Id);
                            if (updatedUser == null)
                            {
                                xc.WriteError(this, "Update failed.");
                            }
                            else
                            {
                                xc.WriteSuccess(this, "Successfully updated.");
                                var table = xc.CreateTable(new string[] { "Column", "Value" });
                                table.AddRow(new[] { "Id", updatedUser.Id });
                                table.AddRow(new[] { "Store", updatedUser.MyShopifyDomain });
                                table.AddRow(new[] { "PreviousPlan", prevPlanId.Value.ToString() });
                                table.AddRow(new[] { "PreviousPlan", updatedUser.PlanId.ToString() });
                                xc.WriteTable(table);
                            }
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                xc.WriteException(this, ex);
            }
        }
    }
}
