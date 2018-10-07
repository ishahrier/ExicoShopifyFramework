using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole.XCommands
{
    public class SetDbPlan : IXCommand
    {
        public string GetDescription()
        {
            return "Update a db plan value by plan id. To find the id of a plan use list-db-plans command.";
        }

        public string GetName()
        {
            return "set-db-plan";
        }

        public async Task Run(XConsole xc)
        {
            using (var scope = xc.WebHost.Services.CreateScope())
            {
                var planService = scope.ServiceProvider.GetService<IDbService<Plan>>();
                xc.AskForInput(this, "Enter plan id: ");
                int planId = Int32.Parse(Console.ReadLine());
                var plan = planService.FindSingleWhere(x => x.Id == planId);
                if (plan != null)
                {
                    xc.WriteSuccess(this, $"Found the plan {plan.Name}.");
                    List<string> options = new List<string>() { "test", "active", "trial", "dev", "price", "order", "popular" };
                    xc.AskForInput(this, "Which field to update (test,active,trial,price,order or dev)?: ");
                    var option = Console.ReadLine();
                    if (options.Contains(option))
                    {
                        switch (option)
                        {
                            case "test":
                                xc.AskForInput(this, "Enter true or false: ");
                                plan.IsTest = bool.Parse(Console.ReadLine());
                                break;
                            case "active":
                                xc.AskForInput(this, "Enter true or false: ");
                                plan.Active = bool.Parse(Console.ReadLine());
                                break;
                            case "trial":
                                xc.AskForInput(this, "Enter number of days: ");
                                plan.TrialDays = Int16.Parse(Console.ReadLine());
                                break;
                            case "dev":
                                xc.AskForInput(this, "Enter true or false: ");
                                plan.IsDev = bool.Parse(Console.ReadLine());
                                break;
                            case "order":
                                xc.AskForInput(this, "Enter display order#: ");
                                plan.DisplayOrder = Int32.Parse(Console.ReadLine());
                                break;
                            case "price":
                                xc.AskForInput(this, "Enter decimal amount: ");
                                plan.Price = decimal.Parse(Console.ReadLine());
                                break;
                            case "popular":
                                xc.AskForInput(this, "Enter true or false: ");
                                plan.IsPopular = bool.Parse(Console.ReadLine());
                                break;
                            default:
                                break;
                        }
                        xc.WriteInfo(this, "Updating plan.");
                        planService.Update(plan, plan.Id);
                        xc.WriteSuccess(this, "Update was successul.");
                        var table = xc.CreateTable(new string[] { "Id", "Name", "TrialDays", "IsActive", "IsDev", "IsTest", "DisplayOrder", "IsPopular", "Price" });
                        table.AddRow(plan.Id, plan.Name, plan.TrialDays, plan.Active, plan.IsDev, plan.IsTest, plan.DisplayOrder, plan.IsPopular, plan.Price.ToString("C"));
                        xc.WriteTable(table);
                    }
                    else
                    {
                        xc.WriteWarning(this, "Invalid option entered.Cannot continue.");
                    }
                }
                else
                {
                    xc.WriteError(this, "Could not find the plan.");
                }

            }
        }
    }
}
