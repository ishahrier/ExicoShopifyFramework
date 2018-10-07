using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class GetInstallUrl : IXCommand
    {
        public string GetName()
        {
            return "get-install-url";
        }

        public string GetDescription()
        {
            return "Displays the app install URL (for developers).";
        }

        public async Task Run(XConsole xc)
        {
            try
            {
                using (var scope = xc.WebHost.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetService<IDbService<SystemSetting>>();
                    xc.AskForInput(this, "Enter the myshopify domain of the target shopify store: ");
                    var storeUrl = Console.ReadLine();
                    if (string.IsNullOrEmpty(storeUrl))
                        xc.WriteError(this, "Not a valid my shopify domain name.");
                    else if (!storeUrl.EndsWith(".myshopify.com"))
                        xc.WriteError(this, "Not a valid my shopify domain name.");
                    else
                    {

                        var apiKey = db.FindSingleWhere(x => x.SettingName == CORE_SYSTEM_SETTING_NAMES.API_KEY.ToString());
                        if (apiKey == null)
                            xc.WriteError(this, $"Setting { CORE_SYSTEM_SETTING_NAMES.API_KEY.ToString()} is missing in the settings table.");
                        else
                        {
                            if (string.IsNullOrEmpty(apiKey.Value))
                            {
                                xc.WriteWarning(this, $"{CORE_SYSTEM_SETTING_NAMES.API_KEY.ToString()} setting doesn't have a value. Please update that setting first.");
                            }
                            else /*all good */
                            {
                                xc.WriteInfo(this, "The installation URL for the app is ");
                                xc.WriteSuccess(this, $"{storeUrl}/admin/api/auth?api_key={apiKey.Value}");
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
