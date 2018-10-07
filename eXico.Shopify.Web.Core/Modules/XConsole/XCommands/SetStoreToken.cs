using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class SetStoreToken : IXCommand
    {
        public string GetName()
        {
            return "set-store-token";
        }

        public string GetDescription()
        {
            return "Sets access token for a store.";
        }

        public async Task Run(XConsole xc)
        {
            try
            {
                xc.AskForInput(this, "Enter myshopify domain (exact url): ");
                using (var scope = xc.WebHost.Services.CreateScope())
                {
                    string input = Console.ReadLine();
                    var db = scope.ServiceProvider.GetService<IDbService<AspNetUser>>();

                    var user = db.FindSingleWhere(x => x.MyShopifyDomain == input);
                    if (user != null)
                    {                        
                        xc.AskForInput(this, "Enter shopify token: ");
                        var token = Console.ReadLine();
                        var previousToken = user.ShopifyAccessToken;
                        user.ShopifyAccessToken = token;
                        xc.WriteInfo(this,"Updating token.");
                        var updatedUser = db.Update(user, user.Id);
                        if (updatedUser != null)
                        {
                            xc.WriteSuccess(this, "Successfully saved new token for the store.");
                            var table = xc.CreateTable(new string[] { "Column", "Value" });
                            table.AddRow(new[] { "Id", updatedUser.Id });
                            table.AddRow(new[] { "Store", updatedUser.MyShopifyDomain });
                            table.AddRow(new[] { "PreviousToken", previousToken });
                            table.AddRow(new[] { "CurrentToken", updatedUser.ShopifyAccessToken });
                            xc.WriteTable(table);
                        }
                        else
                        {
                            xc.WriteError(this, "Error saving new token for the store.");
                        }
                    }
                    else
                    {
                        xc.WriteWarning(this, "Store not found.");
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
