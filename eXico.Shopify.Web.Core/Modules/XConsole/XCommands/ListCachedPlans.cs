using Exico.Shopify.Data.Domain.AppModels;
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
    public class ListCachedPlans : IXCommand
    {
        public string GetName()
        {
            return "list-cached-plans";
        }

        public string GetDescription()
        {
            return "Lists all plans from app servers memory cache.";
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
                        xc.WriteInfo(this, $"Sending request to {remoteSite.Value} for remote plans list.");
                        var data = new WebClient().DownloadString($"{remoteSite.Value}/{XConsole.SERVICE_CONSTROLLER}/listloadedplans?{config[AdminPasswordVerification.ADMIN_PASS_KEYT]}={config[AdminPasswordVerification.ADMIN_PASS_VALUE]}");
                        xc.WriteSuccess(this, "Received response.");
                        try
                        {
                            xc.WriteInfo(this, "Now trying to parse received data.");
                            var list = JsonConvert.DeserializeObject<List<PlanAppModel>>(data);
                            xc.WriteSuccess(this, "Successfull parsed response data");
                            var table = xc.CreateTable(new string[] { "Id", "Name", "TrialDays", "IsActive", "IsDev", "IsTest", "DisplayOrder", "IsPopular", "Price" });
                            foreach (var i in list)
                            {
                                table.AddRow(i.Id, i.Name, i.TrialDays, i.Active, i.IsDev, i.IsTest,i.DisplayOrder,i.IsPopular, i.Price.ToString("C"));
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
