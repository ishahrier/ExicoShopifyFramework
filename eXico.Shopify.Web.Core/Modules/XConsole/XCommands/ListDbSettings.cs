using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class ListDbSettings : IXCommand
    {
        public string GetName()
        {
            return "list-db-settings";
        }

        public string GetDescription()
        {
            return "Lists all settings from database.";
        }

        public async Task Run(XConsole xc)
        {
            try
            {
                xc.AskForInput(this,"Filter by name (or part of it) or leave blank for all settings: ");
                using (var scope = xc.WebHost.Services.CreateScope())
                {
                    string input = Console.ReadLine();
                    var settingsService = scope.ServiceProvider.GetService<IDbService<SystemSetting>>();
                    List<SystemSetting> setting;
                    xc.WriteInfo(this,"Searching settings.");
                    if (string.IsNullOrEmpty(input))
                    {
                        setting = settingsService.FindAll();
                    }
                    else
                    {
                        setting = settingsService.FindManyWhere(x => x.SettingName.Contains(input));
                    }
                    if (setting.Count<=0)
                    {
                        xc.WriteWarning(this, "Total 0 settings found.");
                    }
                    else
                    {
                        xc.WriteSuccess(this,$"Total {setting.Count} setting(s) found.");
                        var table = xc.CreateTable(new[] {"ID", "Name", "DisplayName","Group","Value" });
                        foreach(var s in setting)
                        {
                            table.AddRow(s.Id,s.SettingName, s.DisplayName, s.GroupName, s.Value );
                        }
                        xc.WriteTable(table);

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
