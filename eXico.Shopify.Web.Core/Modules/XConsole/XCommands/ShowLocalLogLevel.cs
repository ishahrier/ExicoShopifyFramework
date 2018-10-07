using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Exico.Shopify.Web.Core.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public class ShowLocalLogLevel : IXCommand
    {
        public string GetName()
        {
            return "show-local-log-level";
        }

        public string GetDescription()
        {
            return "Show (seri) log level of this current x console session.";
        }

        public async Task Run(XConsole xc)
        {
            try
            {
                xc.WriteInfo(this, "Trying to get current session log level...");
                var level = Extensions.HostBuilderExtensions.GetLogLevel();
                xc.WriteInfo(this, "Current session log level is ", false);
                xc.WriteSuccess(this, level);
            }
            catch (Exception ex)
            {
                xc.WriteException(this, ex);
            }
        }


    }
}
