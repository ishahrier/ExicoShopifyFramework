using Exico.Shopify.Web.Core.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
 
namespace Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {            
            BuildWebHost(args).ExicoBuildAndRun(args);
        }

        public static IWebHostBuilder BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

    }
}
