using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class ShowHelp : IXCommand
    {
        public string GetName()
        {
            return "show-help";
        }

        public string GetDescription()
        {
            return "Displays help text for a given command.";
        }

        public async Task Run(XConsole xc)
        {
            try
            {
                xc.AskForInput(this, "Enter command name: ");
                using (var scope = xc.WebHost.Services.CreateScope())
                {
                    string command = Console.ReadLine();
                    var commandClass = xc.FindCommand(command);
                    if (commandClass != null)
                    {
                        try
                        {
                            var commandInstance = xc.CreateCommandInstance(commandClass);
                            xc.WriteHelp(commandInstance);
                        }
                        catch (Exception )
                        {
                            xc.WriteError(this,$"Unable to display help for {command}");
                        }
                    }
                    else
                    {
                        xc.WriteWarning(this, "Command not found.");
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
