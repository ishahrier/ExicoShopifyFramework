using Exico.Shopify.Web.Core.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;

namespace Exico.Shopify.Web.Core
{
    public class Program
    {

        public static void Main(string[] args)
        {
            ExicoBuildWebHost(args).ExicoBuildAndRun(args,true); 
        }
        public static IWebHostBuilder ExicoBuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        //This is needed for db migration
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

    }
}
