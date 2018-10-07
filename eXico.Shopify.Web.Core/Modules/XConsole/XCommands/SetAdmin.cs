using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Web.Core.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class SetAdmin : IXCommand
    {
        public string GetName()
        {
            return "set-admin";
        }

        public string GetDescription()
        {
            return "Sets a store as admin.";
        }

        public async Task Run(XConsole xc)
        {
            try
            {
                xc.AskForInput(this, "Enter myshopify domain(exact url): ");
                using (var scope = xc.WebHost.Services.CreateScope())
                {
                    string input = Console.ReadLine();
                    UserManager<AspNetUser> db = scope.ServiceProvider.GetService<UserManager<AspNetUser>>();

                    var user = db.FindByNameAsync(input).Result;
                    if (user != null)
                    {
                        xc.WriteSuccess(this, "Found store.");
                        xc.AskForInput(this, "Make this store admin ? (y or n): ");
                        var confirm = Console.ReadLine();
                        if(confirm=="y" || confirm == "Y")
                        {
                            
                            if(db.IsInRoleAsync(user, UserInContextHelper.ADMIN_ROLE).Result)
                            {
                                xc.WriteWarning(this, $"Account ({user.MyShopifyDomain}) is already an admin account.");
                            }
                            else
                            {
                                xc.WriteInfo(this, "Updating store as admin account.");
                                var result = await db.AddToRoleAsync(user, UserInContextHelper.ADMIN_ROLE);
                                if (result.Succeeded)
                                {
                                    xc.WriteSuccess(this, $"Successfully updated.");
                                }
                                else
                                {
                                    xc.WriteError(this, $"Update failed.");
                                }
                            }
                        }
                        else
                        {
                            xc.WriteWarning(this, $"Did not update the store as admin.");
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
