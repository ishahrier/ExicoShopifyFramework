using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Exico.Shopify.Web.Core.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class FindStore : IXCommand
    {
        public string GetName()
        {
            return "find-store";
        }

        public async Task Run(XConsole xc)
        {
            try
            {
                xc.AskForInput(this, "Enter exatct or part of my shopify url or email: ");
                using (var scope = xc.WebHost.Services.CreateScope())
                {
                    string input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input))
                    {
                        xc.WriteWarning(this, "Invalid url or email.");
                    }
                    else
                    {
                        var userService = scope.ServiceProvider.GetService<IDbService<AspNetUser>>();
                        var users = userService.FindManyWhere(x => x.MyShopifyDomain.Contains(input) || x.Email.Contains(input) );
                        if (users.Count<=0)
                        {
                            xc.WriteWarning(this, "Nothing found.");
                        }
                        else
                        {
                            xc.WriteSuccess(this, $"Found {users.Count} store(s).");
                            var table = xc.CreateTable(new[] {"Id", "MyShopifyUrl", "Email","Plan", "ChargeId","Token", "BillingOn" ,"IsAdmin" });

                            UserManager<AspNetUser> _userManager = scope.ServiceProvider.GetService<UserManager<AspNetUser>>();
                            var planService = scope.ServiceProvider.GetService<IDbService<Plan>>();
                            foreach (var u in users)
                            {
                                var isAdmin = await _userManager.IsInRoleAsync(u, UserInContextHelper.ADMIN_ROLE);
                                var plan = planService.FindSingleWhere(x => x.Id == u.PlanId);
                                string planInfo = plan == null ? "n/a" : plan.Name;
                                table.AddRow(u.Id, u.MyShopifyDomain, u.Email, $"{u.PlanId}({planInfo})", u.ShopifyChargeId, u.ShopifyAccessToken,u.BillingOn?.Date.ToShortDateString(), isAdmin);
                            }

                            xc.WriteTable(table);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                xc.WriteException(this, ex);
            }
        }

        public string GetDescription()
        {
            return "Find a store and display store info.";
        }
    }
}
