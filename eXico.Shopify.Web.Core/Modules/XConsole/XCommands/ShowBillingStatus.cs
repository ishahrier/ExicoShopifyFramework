using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class ShowBillingStatus : IXCommand
    {
        public string GetName()
        {
            return "show-billing-status";
        }

        public string GetDescription()
        {
            return "Calls shopify API and displays the charge/billing status for the store.";
        }

        public async Task Run(XConsole xc)
        {
            try
            {
                xc.AskForInput(this, "Enter store ID or my shopify domain: ");
                var storeId = Console.ReadLine();
                AspNetUser store = null;

                using (var scope = xc.WebHost.Services.CreateScope())
                {
                    var userService = scope.ServiceProvider.GetService<IDbService<AspNetUser>>();
                    store = userService.FindSingleWhere(x => x.Id == storeId || x.MyShopifyDomain==storeId);
                    if (store == null)
                    {
                        xc.WriteWarning(this, "Store not found.");
                    }
                    else
                    {
                        if (!store.ShopifyChargeId.HasValue)
                        {
                            xc.WriteWarning(this, "Store is not connected to shopify billing yet.");
                        }
                        else if (string.IsNullOrEmpty(store.ShopifyAccessToken))
                        {
                            xc.WriteWarning(this, "Store is not connected to shopify API yet.No access token.");
                        }
                        else
                        {
                            var shopifyApi = scope.ServiceProvider.GetService<IShopifyApi>();
                            var rObj = await shopifyApi.GetRecurringChargeAsync(store.MyShopifyDomain, store.ShopifyAccessToken, store.ShopifyChargeId.Value);

                            xc.WriteSuccess(this, $"Found charge/billing infromation for {store.MyShopifyDomain}.");
                            var table = xc.CreateTable(new string[] { "Name", "Value" });
                            table.AddRow(new[] { "Id", rObj.Id.Value.ToString() });
                            table.AddRow(new[] { "Status", rObj.Status });
                            table.AddRow(new[] { "Name", rObj.Name});
                            table.AddRow(new[] { "Price", rObj.Price.Value.ToString("C") });
                            table.AddRow(new[] { "Is Test", rObj.Test.ToString() });
                            table.AddRow(new[] { "Trial Ends/Ended On", rObj.TrialEndsOn?.DateTime.ToString("F")});                            
                            table.AddRow(new[] { "Trial Days", rObj.TrialDays.ToString()});
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
    }


}

