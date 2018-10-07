using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class ExportStoreEmails : IXCommand
    {
        public string GetName()
        {
            return "export-store-emails";
        }

        public string GetDescription()
        {
            return "Exports store's emails along with store domain (one per line) and writes into a .csv file.";
        }

        public async Task Run(XConsole xc)
        {
            try
            {
                xc.AskForInput(this, "Enter file name with or without path (file extension not needed): ");
                var fileName = Console.ReadLine();
                if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrWhiteSpace(fileName))
                {
                    using (var scope = xc.WebHost.Services.CreateScope())
                    {
                        var userService = scope.ServiceProvider.GetService<IDbService<AspNetUser>>();                        
                        var users = userService.FindAll();
                        xc.WriteInfo(this, $"Found {users.Count} records");
                        var csv = new StringBuilder();
                        csv.AppendLine("Email Address,First Name");
                        foreach (var u in users)
                        {
                            var newLine = string.Format("{0},{1}", u.Email, u.MyShopifyDomain);
                            csv.AppendLine(newLine);
                        }
                        await File.WriteAllTextAsync(fileName +  ".csv", csv.ToString());
                        xc.WriteInfo(this, $"Finished writing to {new FileInfo(fileName).FullName}.csv file.");
                        xc.WriteSuccess(this, $"Export was successfull.");
                    }
                }
                else
                {
                    xc.WriteWarning(this, "Invalid file name.");
                }
            }
            catch (Exception ex)
            {
                xc.WriteException(this, ex);
            }
        }
    }
}
