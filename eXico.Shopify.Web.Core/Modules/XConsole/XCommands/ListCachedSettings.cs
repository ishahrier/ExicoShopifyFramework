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
    public class ListCachedSettings : IXCommand
    {
        public string GetName()
        {
            return "list-cached-settings";
        }

        public string GetDescription()
        {
            return "Lists all settings from app servers memory cache.";
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
                        xc.WriteInfo(this, $"Sending request to {remoteSite.Value} for remote settings list.");
                        var data = new WebClient().DownloadString($"{remoteSite.Value}/{XConsole.SERVICE_CONSTROLLER}/listloadedsettings?{config[AdminPasswordVerification.ADMIN_PASS_KEYT]}={config[AdminPasswordVerification.ADMIN_PASS_VALUE]}");
                        xc.WriteSuccess(this, "Received response.");
                        try
                        {
                            xc.WriteInfo(this, "Now trying to parse received data.");
                            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, SystemSetting>>>(data);
                            xc.WriteSuccess(this, "Successfull parsed response data");
                            var table = xc.CreateTable(new[] { "ID","Name", "DisplayName", "Group", "Value" });
                            foreach (var k in dictionary.Keys)
                            {
                                var root = dictionary[k];
                                foreach(var l in root.Keys)
                                {
                                    var s = root[l];
                                    table.AddRow(s.Id, s.SettingName, s.DisplayName, s.GroupName, s.Value);
                                }
                            }
                            xc.WriteTable(table);
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
