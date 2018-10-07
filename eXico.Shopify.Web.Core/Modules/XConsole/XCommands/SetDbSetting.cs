using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class SetDbSetting : IXCommand
    {
        public string GetName()
        {
            return "set-db-setting";
        }

        public string GetDescription()
        {
            return "Update a db setting value by setting id or name. To find the id of a setting use list-db-settings command.";
        }

        public async Task Run(XConsole xc)
        {
            try
            {
                using (var scope = xc.WebHost.Services.CreateScope())
                {
                    xc.AskForInput(this, "Enter setting name or id: ");
                    var sIdName = Console.ReadLine();
                    var settingsService = scope.ServiceProvider.GetService<IDbService<SystemSetting>>();
                    var setting = settingsService.FindSingleWhere(x => x.SettingName == sIdName || x.Id.ToString()==sIdName);
                    if (setting == null)
                    {
                        xc.WriteError(this, "Setting not found.");
                    }
                    else
                    {
                        xc.WriteSuccess(this, $"Found the setting '{setting.SettingName}'.");
                        xc.AskForInput(this, "Enter setting value: ");
                        var sVal = Console.ReadLine();
                        var oldValue = setting.Value;
                        setting.Value = sVal;
                        xc.WriteInfo(this,"Updating the setting.");
                        setting = settingsService.Update(setting, setting.Id);
                        if (setting == null)
                        {
                            xc.WriteWarning(this, "Update failed.");
                        }
                        else
                        {
                            xc.WriteSuccess(this, "Successfully updated.");
                            var table = xc.CreateTable(new[] { "Column", "Value" });                            
                            table.AddRow("SettingId", setting.Id);
                            table.AddRow("SettingName", setting.SettingName);
                            table.AddRow("OldValue", oldValue);
                            table.AddRow("UpdatedValue", setting.Value);
                            xc.WriteTable(table);
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
