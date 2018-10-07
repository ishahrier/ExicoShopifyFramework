using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class ShowAppStatus : IXCommand
    {
        public string GetName()
        {
            return "show-app-status";
        }

        public string GetDescription()
        {
            return "Shows the app url and and checks if it is online or not.";
        }

        public async Task Run(XConsole xc)
        {
            
            try
            {
                using (var scope = xc.WebHost.Services.CreateScope())
                {
                    var settings = scope.ServiceProvider.GetService<IDbService<SystemSetting>>();
                    var url = settings.FindSingleWhere(x=>x.SettingName== CORE_SYSTEM_SETTING_NAMES.APP_BASE_URL.ToString());

                    if (url != null)
                    {
                        if (string.IsNullOrEmpty(url.Value))
                        {
                            xc.WriteWarning(this, "App base url value is not set in the settings");
                        }
                        else
                        {
                            xc.WriteInfo(this, $"App is hosted at ", false);
                            xc.Writer.Write($"'{url.Value}'  ", ConsoleColor.Gray);
                            xc.WriteInfo(this, "Status code - ", false);

                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url.Value);
                            request.Timeout = 15000;
                            HttpWebResponse response;
                            try
                            {
                                response = (HttpWebResponse)request.GetResponse();
                                xc.WriteSuccess(this, response.StatusCode.ToString());
                            }
                            catch (Exception ex)
                            {
                                xc.WriteError(this, "Unreachable.");
                                throw ex;
                            }
                        }
                    }else
                    {
                        xc.WriteError(this, "App base url setting not found.");
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
