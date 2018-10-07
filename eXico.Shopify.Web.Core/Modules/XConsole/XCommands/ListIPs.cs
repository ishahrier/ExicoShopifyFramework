using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class ListIps : IXCommand
    {
        public string GetName()
        {
            return "list-ips";
        }

        public string GetDescription()
        {
            return "Lists all the priviledged ips from the DB.";
        }

        public async Task Run(XConsole xc)
        {
            try
            {
                using (var scope = xc.WebHost.Services.CreateScope())
                {
                    
                    var db = scope.ServiceProvider.GetService<IDbService<SystemSetting>>();
                    var item = db.FindSingleWhere(x => x.SettingName == CORE_SYSTEM_SETTING_NAMES.PRIVILEGED_IPS.ToString());
                    if (item == null)
                    {
                        xc.WriteError(this, $"Could not list IPs because the setting {CORE_SYSTEM_SETTING_NAMES.PRIVILEGED_IPS} doesn't exit in the database.");
                    }
                    else
                    {
                        xc.WriteInfo(this, "Printing the list of priviledged IPs");
                        if(string.IsNullOrEmpty(item.Value))
                        {
                            xc.WriteWarning(this, "none found.");
                        }
                        else
                        {
                            var ips = item.Value.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            xc.WriteInfo(this, $"Total {ips.Length} IPs found.");
                            foreach (var i in ips )
                            {
                                xc.WriteSuccess(this, i);
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
