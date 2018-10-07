using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class AddIp : IXCommand
    {
        public string GetName()
        {
            return "add-ip";
        }

        public string GetDescription()
        {
            return "Adds an IP address in the privileged IP list.";
        }

        public async Task Run(XConsole xc)
        {
            try
            {
                using (var scope = xc.WebHost.Services.CreateScope())
                {
                    var ipServiceUrl = "http://ipinfo.io/ip";
                    string externalIp = null;
                    try
                    {
                        externalIp = new WebClient().DownloadString(ipServiceUrl);
                        externalIp = Regex.Replace(externalIp, @"\r\n?|\n|\t", String.Empty);
                        xc.WriteInfo(this, $"(FYI : your external IP is - {externalIp})");
                    }
                    catch (Exception)
                    {
                        xc.WriteInfo(this, $"(FYI : your external IP is - ", false);
                        xc.WriteError(this, $"failed to retrive using {ipServiceUrl}.)");
                    }


                    xc.AskForInput(this, "Enter the IP you want to add to the Privilege IP list: ");
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
                            var alreadyExists = false;
                            if (item.Value != null)
                            {
                                if (item.Value.Contains(ip))
                                {
                                    xc.WriteInfo(this, $"IP {ip} is already in the privileged IP list.");
                                    alreadyExists = true;
                                }
                            }
                            if (!alreadyExists)
                            {
                                ip = ip.Trim();
                                item.Value = string.IsNullOrEmpty(item.Value) ? ip : $"{item.Value},{ip}";
                                xc.WriteInfo(this, "Updating IP list.");
                                var itemUpdated = db.Update(item, item.Id);
                                if (itemUpdated == null)
                                {
                                    xc.WriteError(this, text: $"Adding {externalIp} to the privileged IP list failed.");
                                }
                                else
                                {
                                    xc.WriteSuccess(this, $"{externalIp} has been added to the privileged IP list.");
                                }

                            }
                        }
                    }
                    else
                    {
                        xc.WriteWarning(this, $"Nothing has been added to the privileged IP list.");
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
