using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class RemoveIP : IXCommand
    {
        public string GetName()
        {
            return "remove-ip";
        }

        public string GetDescription()
        {
            return "Removes an IP address from the privileged IP list.";
        }

        public async Task Run(XConsole xc)
        {
            try
            {
                using (var scope = xc.WebHost.Services.CreateScope())
                {

                    xc.AskForInput(this, "Enter the IP you want to remove from the privilege IP list: ");
                    string ip = Console.ReadLine();
                    if (!string.IsNullOrEmpty(ip))
                    {
                        var db = scope.ServiceProvider.GetService<IDbService<SystemSetting>>();
                        var item = db.FindSingleWhere(x => x.SettingName == CORE_SYSTEM_SETTING_NAMES.PRIVILEGED_IPS.ToString());
                        if (item == null)
                        {
                            xc.WriteError(this, $"Could not add IP because the setting {CORE_SYSTEM_SETTING_NAMES.PRIVILEGED_IPS} doesn't exit in the database.");
                        }
                        else
                        {
                            if (item.Value != null)
                            {
                                if (item.Value.Contains(ip))
                                {
                                    xc.WriteInfo(this, $"IP {ip} is found in the privileged IP list.");
                                    List<string> _ips = new List<string>();
                                    xc.WriteInfo(this, $"Updating privileged IP table.");
                                    foreach (var i in item.Value.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                                    {
                                        if (!i.Trim().Equals(ip.Trim())) _ips.Add(i);
                                    }
                                    item.Value = string.Join(',', _ips);
                                    var itemUpdated = db.Update(item, item.Id);
                                    if (itemUpdated == null)
                                    {
                                        xc.WriteError(this, text: $"Removing {ip} to the privileged IP list failed.");
                                    }
                                    else
                                    {
                                        xc.WriteSuccess(this, $"{ip} has been removed from the privileged IP list.");
                                    }
                                    
                                }
                                else
                                {
                                    xc.WriteWarning(this, $"The ip {ip} doesn't even exist in the priviledged IP tables.");
                                }
                            }
                        }
                    }
                    else
                    {
                        xc.WriteWarning(this, $"Nothing has been removed from the privileged IP list.");
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
