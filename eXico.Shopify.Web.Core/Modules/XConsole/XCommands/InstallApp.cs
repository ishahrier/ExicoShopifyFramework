using Exico.Shopify.Data.Domain.DBModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class InstallApp : IXCommand
    {
        public string GetDescription()
        {
            return "Installs this app for a store.";
        }

        public string GetName()
        {
            return "install-app";
        }

        public async Task Run(XConsole xc)
        {
            try
            {

                using (var scope = xc.WebHost.Services.CreateScope())
                {
                    AspNetUser user = new AspNetUser();
                    xc.AskForInput(this, "Enter myshopify URL (without http part): ");
                    user.MyShopifyDomain = Console.ReadLine();
                    xc.AskForInput(this, "Enter shop email: ");
                    user.Email = Console.ReadLine();

                    xc.AskForInput(this, "Enter plan id: ");
                    var pid = Console.ReadLine();
                    user.PlanId = Int32.Parse(pid);

                    xc.AskForInput(this, "Enter shopify access token: ");
                    user.ShopifyAccessToken = Console.ReadLine();
                    user.UserName = user.MyShopifyDomain;

                    xc.AskForInput(this, "Enter Charge Id (optoinal for admin store): ");
                    var cid = Console.ReadLine();
                    if (string.IsNullOrEmpty(cid)) user.ShopifyChargeId = null;
                    else user.ShopifyChargeId = long.Parse(cid);

                    user.BillingOn = DateTime.Now;

                    UserManager<AspNetUser> db = scope.ServiceProvider.GetService<UserManager<AspNetUser>>();
                    xc.WriteInfo(this, "Installing the app for the store.");
                    var passGen = scope.ServiceProvider.GetService<IGenerateUserPassword>();
                    var pass = passGen.GetPassword(new Data.Domain.AppModels.PasswordGeneratorInfo(user));
                    var result = await db.CreateAsync(user, pass);
                    if (result.Succeeded)
                    {
                        xc.WriteSuccess(this, "Successfull installed the app.");
                        var table = xc.CreateTable(new string[] { "Column", "Value" });
                        table.AddRow("Id", user.Id);
                        table.AddRow("UserName", user.UserName);
                        table.AddRow("MyShopifyDomain", user.MyShopifyDomain);
                        table.AddRow("Email", user.Email);
                        table.AddRow("ShopifyAccessToken", user.ShopifyAccessToken);
                        table.AddRow("PlanId", user.PlanId);
                        table.AddRow("ShopifyChargeId", user.ShopifyChargeId);
                        table.AddRow("BilingOn", user.BillingOn);
                        xc.WriteTable(table);
                    }
                    else
                    {
                        xc.WriteError(this, $"Installation error occurred.{Environment.NewLine}{result.ToString()}");
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
