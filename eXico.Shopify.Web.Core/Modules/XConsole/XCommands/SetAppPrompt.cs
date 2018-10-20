using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class SetAppPrompt : IXCommand
    {
        public string GetName()
        {
            return "set-app-prompt";
        }

        public string GetDescription()
        {
            return "Sets the prompt to app name";
        }

        public async Task Run(XConsole xc)
        {

            try
            {
                using (var scope = xc.WebHost.Services.CreateScope())
                {
                    var settings = scope.ServiceProvider.GetService<IDbService<SystemSetting>>();
                    var name = settings.FindSingleWhere(x => x.SettingName == CORE_SYSTEM_SETTING_NAMES.APP_NAME.ToString());

                    if (name != null)
                    {
                        Console.Title = $"X-Console  -  {name.Value}";
                        xc.SetPromptString($"{name.Value}:>> ");
                    }
                    else
                    {
                        xc.WriteError(this, "App name is not set in the db.");
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
