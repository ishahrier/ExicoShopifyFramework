using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class ListDbPlans : IXCommand
    {
        public string GetName()
        {
            return "list-db-plans";
        }

        public string GetDescription()
        {
            return "Lists all plans from database.";
        }

        public async Task Run(XConsole xc)
        {
            try
            {
                xc.AskForInput(this, "Filter by name or ID (leave blank for all): ", false);
                var filter = Console.ReadLine();
                using (var scope = xc.WebHost.Services.CreateScope())
                {

                    var db = scope.ServiceProvider.GetService<IDbService<Plan>>();
                    List<Plan> list;
                    if (string.IsNullOrEmpty(filter)) list = db.FindAll();
                    else
                    {
                        list = db.FindManyWhere(x => x.Name.Contains(filter) || x.Id.ToString()==filter);
                    }
                    xc.WriteInfo(this,"Listing plans.");
                    if (list.Count > 0)
                    {
                        xc.WriteSuccess(this, $"Total {list.Count} plan(s) found.");
                        var table = xc.CreateTable(new string[] { "Id", "Name", "TrialDays", "IsActive", "IsDev", "IsTest","DisplayOrder", "IsPopular", "Price" });
                        foreach (var i in list)
                        {
                            table.AddRow(i.Id, i.Name, i.TrialDays, i.Active, i.IsDev, i.IsTest,i.DisplayOrder,i.IsPopular, i.Price.ToString("C"));
                        }
                        xc.WriteTable(table);
                    }
                    else
                    {
                        xc.WriteWarning(this, "Total 0 plan(s) found."); 
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
