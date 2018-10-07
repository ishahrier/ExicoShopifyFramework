using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Web.Core.Controllers.BaseControllers;
using Exico.Shopify.Web.Core.Filters;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Controllers.BaseControllers
{
    [ServiceFilter(typeof(IPAddressVerification), Order = IPAddressVerification.DEFAULT_ORDER)]
    [ServiceFilter(typeof(AdminPasswordVerification))]
    public class XController : ABaseController
    {
        protected readonly IPlansReader PlanReader;        

        public XController( IPlansReader planReader, IConfiguration config, IDbSettingsReader settings,ILogger<XController> logger) : base(config, settings,logger)
        {
            this.PlanReader = planReader;            
        }       

        public IActionResult Index()
        {
            return View();
        }

        public JsonResult ListLoadedPlans()
        {
            try
            {
                Logger.LogInformation("Getting plan data.");
                var data = PlanReader.GetAllPlans();
                Logger.LogInformation("Done getting plan data.");
                Logger.LogInformation($"Returning {data.Count} plans as json data.");
                return Json(data);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,"Error listing plan.");
                return Json(null);
            }
            
        }

        public JsonResult ReloadPlans()
        {
            try
            {
                Logger.LogInformation("Requesting to reload plans.");
                PlanReader.ReloadFromDBAndUpdateCache();
                Logger.LogInformation("Successfully reloaded plans.");
                return Json(true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,"Error reloading plan from db.");
                return Json(false);
            }
        }

        public JsonResult ListLoadedSettings( )
        {
            try
            {
                Logger.LogInformation("Getting settings data.");
                var data = Settings.AllSettings;
                Logger.LogInformation("Done getting settings data.");
                Logger.LogInformation($"Returning settings as json data.");
                return Json(data);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error listing settings.");
                return Json(null);
            }
            
        }
        public JsonResult ReloadSettings()
        {
            try
            {
                Logger.LogInformation("Requesting to reload settings.");
                Settings.ReloadFromDbAndUpdateCache();
                Logger.LogInformation("Done reloading settings.");
                return Json(true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,"Request to reload settings data failed.");
                return Json(false);
            }
        }

        public JsonResult ChangeLogLevel(string toLevel)
        {
            try
            {
                Logger.LogInformation($"Trying to change log level to {toLevel}.");
                Serilog.Events.LogEventLevel level = (Serilog.Events.LogEventLevel)Enum.Parse(typeof(Serilog.Events.LogEventLevel), toLevel);
                Extensions.HostBuilderExtensions.SwitchToLogLevel(level);
                Logger.LogInformation($"Successfully changed log level to {toLevel}.");
                return Json(true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error changing log level to {toLevel}.");
                return Json(false);
            }
            
            
        }
        
        public JsonResult GetCurrentLogLevel()
        {
            try
            {
                var level = Extensions.HostBuilderExtensions.GetLogLevel();
                Logger.LogInformation($"Current log level is {level}.");
                return Json(Extensions.HostBuilderExtensions.GetLogLevel());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting current log level.");
                return Json(string.Empty);
            }

        }
        protected override string GetPageTitle()
        {
            return "X-Console";
        }

    }
}