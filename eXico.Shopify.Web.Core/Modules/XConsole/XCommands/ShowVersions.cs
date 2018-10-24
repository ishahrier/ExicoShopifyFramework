using Exico.Shopify.Data.Domain.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class ShowVersions : IXCommand
    {
        public string GetName()
        {
            return "show-versions";
        }

        public string GetDescription()
        {
            return "Displays the framewor and application version and build numbers.";
        }

        public async Task Run(XConsole xc)
        {
            try
            {
                using (var scope = xc.WebHost.Services.CreateScope())
                {
                    var settings = scope.ServiceProvider.GetService<IDbSettingsReader>();
                    xc.WriteInfo(this, "Retriving version information..");
                    var versions = new Versions()
                    {
                        AppVersion = settings.GetAppVersion(),
                        DataSeederFrameworkVersion = settings.GetDataSeederFrameworkVersion(),                        
                        FrameWorkVersion = AppSettingsAccessor.GetFrameWorkBuildNumber(true)
                    };
                    xc.WriteSuccess(this, "Done.");
                    var table = xc.CreateTable(new string[] { "Item", "Value" });
                    table.AddRow("Application Version", versions.AppVersion);
                    table.AddRow("Data Seeder Framework Version", versions.DataSeederFrameworkVersion);
                    table.AddRow("Framework Version", versions.FrameWorkVersion);
                    
                    var parts = versions.NugetVersion.Split('.');
                    table.AddRow("Nuget Version", parts.Length <= 3 ? versions.NugetVersion : string.Join('.', parts.Take(3)));
                    xc.WriteTable(table);

                }
            }
            catch (Exception ex)
            {
                xc.WriteException(this, ex);
            }
        }


    }
}
