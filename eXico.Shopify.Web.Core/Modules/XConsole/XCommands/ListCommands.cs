using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class ListCommands : IXCommand
    {
        public string GetName()
        {
            return "list-commands";
        }

        public string GetDescription()
        {
            return "Displays the list of commands available.";
        }

        public async Task Run(XConsole xc)
        {
            try
            {
                xc.AskForInput(this, "Filter by (leave blank for all): ",false);
                var filter = Console.ReadLine();
                List<string> list;
                if (string.IsNullOrEmpty(filter)) list = xc.CommandList;
                else
                {
                    filter = filter.Replace("-", "").ToLower();
                    list  = xc.CommandList.Where(x => x.ToLower().Contains(filter )).ToList();
                }
                xc.WriteInfo(this, $"Total {list.Count} commands found.");
                
                if (list.Count > 0)
                {
                    var table = xc.CreateTable(new string[] { "Command", "Description" });
                    foreach (string className in list)
                    {
                        try
                        {
                            var instance = xc.CreateCommandInstance(className);
                            table.AddRow(new[] { instance.GetName(), instance.GetDescription() });
                        }
                        catch (Exception)
                        {

                            throw;
                        }
                    }
                    //table.Write(ConsoleTables.Format.Alternative);
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
