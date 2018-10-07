using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class SetLocalLogLevel : IXCommand
    {
        public string GetName()
        {
            return "set-local-log-level";
        }

        public string GetDescription()
        {
            return "Dynamically switches (seri) log level for this session.";
        }

        public async Task Run(XConsole xc)
        {
            try
            {
                using (var scope = xc.WebHost.Services.CreateScope())
                {
                    xc.AskForInput(this, "Enter log level (case sensitive) you want to switch over: ");
                    var toLevel = Console.ReadLine();
                    xc.WriteInfo(this, $"Trying to change log level to {toLevel} ...");
                    Serilog.Events.LogEventLevel level = (Serilog.Events.LogEventLevel)Enum.Parse(typeof(Serilog.Events.LogEventLevel), toLevel);
                    Extensions.HostBuilderExtensions.SwitchToLogLevel(level);
                    xc.WriteSuccess(this, "Done");
                }
            }
            catch (Exception ex)
            {
                xc.WriteException(this, ex);
            }
        }
    }
}
