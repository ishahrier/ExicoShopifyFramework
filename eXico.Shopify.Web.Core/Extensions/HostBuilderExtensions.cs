using Exico.Shopify.Web.Core.Modules.XConsole;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using System;
using System.IO;
using System.Net;

namespace Exico.Shopify.Web.Core.Extensions
{
    /// <summary>
    /// Contains IWebHostBuilder extension methods that are used in program.cs files
    /// </summary>
    public static class HostBuilderExtensions
    {
        public static LoggingLevelSwitch LevelSwitch = new LoggingLevelSwitch() { MinimumLevel = Serilog.Events.LogEventLevel.Debug };

        /// <summary>
        /// This method tries to configure the serilog logger and then it either start the x-console or starts the kestrel web server.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If you pass createDefaultLoggger param value as <code>false</code> then the system will not configure the serilogger
        /// for you.If <code>false</code>, then the settings for creating the Serilog logger is read from the config file. Here is an example
        /// serilog settings section that you can copy and paste onto your appsettings.json
        /// <code>
        ///   "Serilog": {
        ///    "MinimumLevel": {
        ///      "Default": "Debug",
        ///     "Override": {
        ///        "Microsoft": "Error",
        ///        "System": "Error"
        ///      }
        ///    },
        ///    "WriteTo": [
        ///      {
        ///        "Name": "Console",
        ///        "Args": {
        ///          "outputTemplate": "{Timestamp:u} [{Level:u3}] {SourceContext:l} {RequestId,13} {Message}{NewLine}{ExicoShopifyUserContext}{NewLine}{Exception}"
        ///        }
        ///      }
        ///    }]
        /// </code>
        /// </para>
        /// <para>
        /// Note that during the creation of logger minimum level is maintained by using a variable. <code> MinimumLevel.ControlledBy(ExicoShopifyBuilder.LevelSwitch)</code>
        /// So that you can change the log level dynamically any time by a simple one line of code anywhere in you code.
        /// <code>ExicoShopifyBuilder.LevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Warning;</code>
        /// <para>Now if you do not let the system configure the SeriLog for you then you remember to call
        /// <code>MinimumLevel.ControlledBy(ExicoShopifyBuilder.LevelSwitch)</code> during the configuration/creation of SeriLog logger
        /// otherwise the system won't be able to let you dynamically change the log level.
        /// </para>
        /// </para>
        /// Note that you should call this method as the last call on the IWebHostBuilder.
        /// Here is an example program.cs file content
        /// <code>
        ///public class Program
        ///{
        ///
        ///    public static void Main(string[] args)
        ///    {
        ///        BuildWebHost(args)
        ///           .UseAzureAppServices() /*use whatever you want to use*/
        ///           .ExicoBuildAndRun(args); /*then as the last step call this method*/
        ///    }
        ///    public static IWebHostBuilder BuildWebHost(string[] args) =>
        ///        WebHost.CreateDefaultBuilder(args)
        ///               .UseStartup<Startup>();
        ///
        ///}
        /// </code>
        /// </remarks>
        /// <param name="args">The arguments from commandline.</param>
        /// <param name="builder">The IWebHostBuilder.</param>
        /// <param name="configureLogger">If you want to use the SeriLog logger configured by the framework itself then pass <code>false</code>, default is true</param>
        public static void ExicoBuildAndRun(this IWebHostBuilder builder, string[] args, bool configureLogger = true)
        {
            //configure the serilog logger if allowed
            var config = Configuration();
            if (configureLogger)
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(config)
                    .Enrich.FromLogContext()
                    .MinimumLevel.ControlledBy(LevelSwitch)                    
                    .CreateLogger();
                
            }
            Log.Information($"Starting ExicoBuildAndRun with args {string.Join(' ', args)}");
            bool isConsole = false;
            try
            {
                Log.Information("Setting tls security protocol.");
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                Log.Information("Done setting tls security protocol.");
                Log.Information("Calling UseSerilog().");
                builder.UseSerilog(dispose: true);
                Log.Information("Done calling UseSerilog().");
                //start console if enabled
                if (args.Length > 0 && args[0] == XConsole.ARG_VALUE)
                {
                    Log.Information("X-Console argument detected.");
                    if (config["ConsoleEnabled"].ToString() == "1")
                    {
                        Log.Information("Starting X-Console...");
                        //Task.Run<object>(async()=> await ( new XConsole(builder.Build()).Start()));
                        isConsole = true;
                        Console.Title = "X-Console  -  Exico Shopify Framework";
                        new XConsole(builder.Build()).Start();
                    }
                    else
                    {
                        Log.Warning("Console is disabled.Starting web app instead...");
                        builder.Build().Run();
                    }
                }
                //or start the kesrel web server
                else
                {
                    Log.Information("Starting web app...");
                    builder.Build().Run();
                }

            }
            catch (Exception ex)
            {
                if (isConsole)
                {
                    Log.Fatal(ex, "X-Console terminated unexpectedly.");
                    Console.Write("Press any key to end..");
                    Console.ReadKey();
                }
                else
                {
                    Log.Fatal(ex, "Web app terminated unexpectedly.");
                }
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }

        /// <summary>
        /// Reading appsettings.json for all environments and building the config 
        /// </summary>
        /// <returns><see cref="IConfiguration"/></returns>
        private static IConfiguration Configuration()
        {
            var currentEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{currentEnv}.json", optional: true, reloadOnChange: true)
                            .Build();
        }

        /// <summary>
        /// Dynamically changes the SeriLog log level.
        /// </summary>
        /// <remarks>        
        /// It wont work if SeriLog logger is configured by yourself
        /// and you forgot to call <code>MinimumLevel.ControlledBy(ExicoShopifyBuilder.LevelSwitch)</code>
        /// during the congiuration/creation.
        /// </remarks>
        /// <param name="level"></param>
        public static void SwitchToLogLevel(Serilog.Events.LogEventLevel level)
        {
            LevelSwitch.MinimumLevel = level;
        }

        /// <summary>
        /// Get current SeriLog log level.
        /// </summary>
        /// <returns></returns>
        public static string GetLogLevel()
        {
            return LevelSwitch.MinimumLevel.ToString();
        }
    }
}
