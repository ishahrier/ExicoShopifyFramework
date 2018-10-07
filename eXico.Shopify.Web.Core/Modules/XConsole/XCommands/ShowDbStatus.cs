using Exico.Shopify.Data;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class ShowDbStatus : IXCommand
    {
        public string GetName()
        {
            return "show-db-status";
        }

        public string GetDescription()
        {
            return "Shows the db connection/status that the console and app is connected to.";
        }

        public async Task Run(XConsole xc)
        {
            try
            {
                using (var scope = xc.WebHost.Services.CreateScope())
                {
                    var exicoCtx = scope.ServiceProvider.GetService<ExicoShopifyDbContext>();
                    var con = exicoCtx.Database.GetDbConnection().ConnectionString;
                    if (!string.IsNullOrEmpty(con))
                    {
                        var parts = con.Split(new char[] { ';' });
                        xc.WriteInfo(this, "X Console will connect using>> ");
                        var table = xc.CreateTable(new string[] { "Con Attribute", "value" });
                        foreach (var part in parts)
                        {
                            var keyValue = part.Split(new char[] { '=' });
                            var key = keyValue[0];
                            var value = keyValue[1];
                            table.AddRow(key, value);
                        }
                        xc.WriteTable(table);
                        xc.WriteInfo(this, "Connection status: ", false);
                        try
                        {
                            exicoCtx.Database.GetDbConnection().Open();
                            xc.WriteSuccess(this, "OK");
                            exicoCtx.Database.GetDbConnection().Close();

                        }
                        catch (Exception ex)
                        {
                            xc.WriteError(this, "ERROR");
                            throw ex;
                        }
                    }
                    else
                    {
                        xc.WriteError(this,"Framework db context connection string is empty.");
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
