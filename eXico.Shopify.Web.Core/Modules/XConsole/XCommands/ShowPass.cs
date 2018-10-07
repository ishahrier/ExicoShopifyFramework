using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class ShowPass : IXCommand
    {
        public string GetName()
        {
            return "show-pass";
        }

        public string GetDescription()
        {
            return "Type a store's my shopify domain and it will give you the username and password for that store.";
        }

        public async Task Run(XConsole xc)
        {
            try
            {
                xc.AskForInput(this, "Enter myshopify domain: ");
                using (var scope = xc.WebHost.Services.CreateScope())
                {
                    string input = Console.ReadLine();
                    var db = scope.ServiceProvider.GetService<IDbService<AspNetUser>>();
                    var user = db.FindSingleWhere(x => x.MyShopifyDomain == input);
                    if (user != null)
                    {
                        xc.WriteSuccess(this, "Found the store.");
                        var passGen = scope.ServiceProvider.GetService<IGenerateUserPassword>();
                        var pass = passGen.GetPassword(new Data.Domain.AppModels.PasswordGeneratorInfo(user));
                        var table = xc.CreateTable(new[] { "UserName", "Password" });
                        table.AddRow(user.UserName, pass);
                        xc.WriteTable(table);
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
