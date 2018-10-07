using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Exico.Shopify.Web.Core.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class SetRemoteLogLevel : IXCommand
    {
        public string GetName()
        {
            return "set-remote-log-level";
        }

        public string GetDescription()
        {
            return "Dynamically switch remote site's (seri) log level to a new one.";
        }

        public async Task Run(XConsole xc)
        {
            try
            {

                using (var scope = xc.WebHost.Services.CreateScope())
                {
                    var config = scope.ServiceProvider.GetService<IConfiguration>();
                    var settingDB = scope.ServiceProvider.GetService<IDbService<SystemSetting>>();
                    var remoteSite = settingDB.FindSingleWhere(x => x.SettingName == CORE_SYSTEM_SETTING_NAMES.APP_BASE_URL.ToString());
                    if (remoteSite == null && string.IsNullOrEmpty(remoteSite.Value))
                    {
                        xc.WriteError(this, "App base url is not found in the db.Cannot continue.");
                    }
                    else
                    {                        
                        xc.AskForInput(this, "Enter log level (case sensitive) you want to switch over: ");
                        var level = Console.ReadLine();
                        xc.WriteInfo(this, $"Sending request to {remoteSite.Value} to switch log level to {level}.");
                        var data = new WebClient().DownloadString($"{remoteSite.Value}/{XConsole.SERVICE_CONSTROLLER}/ChangeLogLevel?tolevel={level}&{config[AdminPasswordVerification.ADMIN_PASS_KEYT]}={config[AdminPasswordVerification.ADMIN_PASS_VALUE]}");
                        xc.WriteSuccess(this, "Received response.");
                        try
                        {
                            xc.WriteInfo(this, "Now trying to parse received data..");
                            var response = JsonConvert.DeserializeObject<bool>(data);                            
                            if (response) xc.WriteSuccess(this,"Server responded switching was successful.");
                            else xc.WriteError(this, "Server responded switching was unsuccessful.");
                                                  
                            
                        }
                        catch (Exception ex)
                        {
                            xc.WriteError(this, "Error parsing received data." + ex.Message);
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
