using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Data.Domain.ViewModels;
using Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces;
using Exico.Shopify.Web.Core.Helpers;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Controllers.BaseControllers
{
    /// <summary>
    /// Every controller extends this base controller.
    /// It also inserts all necessary viewbag variables.
    /// </summary>
    public abstract class ABaseController : Controller, IABaseController
    {
        protected readonly IDbSettingsReader Settings;
        protected readonly IConfiguration Config;
        protected readonly ControllerNames Controllers;
        protected readonly ViewNames Views;
        protected readonly ILogger Logger;
        public ABaseController(IConfiguration config, IDbSettingsReader settings, ILogger logger) : base()
        {
            this.Settings = settings;
            this.Config = config;
            this.Views = new ViewNames();
            this.Logger = logger;
            this.Controllers = new ControllerNames
            {
                DashboardController = Settings.GetAppDashboardControllerName(),
                MyProfileController = Settings.GetAppMyProfileControllerName(),
                AccountController = Settings.GetAccountControllerName(),
                UninstallController = Settings.GetAppUninstallControllerName()
            };
            
            this.VersionInfo = new Versions()
            {
                AppVersion = settings.GetAppVersion(),
                FrameWorkVersion = AppSettingsAccessor.GetFrameWorkBuildNumber(),                
                DataSeederFrameworkVersion = Settings.GetDataSeederFrameworkVersion()
            };            

        }

        /// <summary>
        /// Current logged on store data. Null if not logged on.
        /// </summary>
        protected AppUser Store { get; set; }

        /// <summary>
        /// Framework buildnumber
        /// </summary>
        protected Versions VersionInfo { get;  }        

        /// <summary>
        /// Indicates if the app uses shopify embeded sdk
        /// </summary>
        protected bool UsesEmbebedSdk => AppSettingsAccessor.IsUsingEmbededSdk(Config);

        /// <summary>
        /// The application name
        /// </summary>
        protected string AppName => Settings.GetAppName();
        
        /// <summary>
        /// Html page title for the title tag        
        /// </summary>
        /// <returns></returns>
        protected abstract string GetPageTitle();

        /// <summary>
        /// Logs the generic error. A helper method.
        /// </summary>
        /// <param name="ex">The error/exception.</param>
        protected virtual void LogGenericError(Exception ex)
        {
            Logger.LogError(ex, "Error Occurred");
        }

        /// <summary>
        /// Helper method for logging action.result executing steps.
        /// </summary>
        /// <param name="msgPrefix">The message prefix.</param>
        [NonAction]
        private void LogExecution(string msgPrefix)
        {
            var action = ControllerContext.ActionDescriptor.ActionName;
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            Logger.LogInformation($"{msgPrefix} - Action: {controller}/{action}");
        }
        [NonAction]
        public void OnResultExecuted(ResultExecutedContext context)
        {
            Logger.LogInformation("Calling MyOnResultExecuted() event.");
            MyOnResultExecuted(context);
            Logger.LogInformation("Handled MyOnResultExecuted() event.");
            var vr = (context.Result as ViewResult)?.ViewName;
            vr = vr == null ? "null" : vr + ".cshtml";
            LogExecution($"Result Executed - View : '{vr}'");
        }
        [NonAction]
        public void OnResultExecuting(ResultExecutingContext context)
        {
            var vr = (context.Result as ViewResult)?.ViewName;
            vr = vr == null ? "null" : vr + ".cshtml";
            LogExecution($"Result Executing - View : '{vr}'");
            Logger.LogInformation("Calling MyOnResultExecuting() event.");
            MyOnResultExecuting(context);
            Logger.LogInformation("Handled MyOnResultExecuting() event.");
        }
        [NonAction]
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            LogExecution("Action Executing");
            Logger.LogInformation("Calling MyOnActionExecuting() event.");
            MyOnActionExecuting(context);
            Logger.LogInformation("Handled MyOnActionExecuting() event.");

        }

        [NonAction]
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            ViewBag.UsesEmbededSdk = UsesEmbebedSdk;
            ViewBag.PrintViewFileName = Config["PrintViewFileName"];
            ViewBag.AppName = AppName;
            ViewBag.PageTitle = GetPageTitle();
            ViewBag.Controllers = Controllers;
            ViewBag.Views = Views;
            ViewBag.VersionInfo = VersionInfo;            
            ViewBag.Store = Store;
            Logger.LogInformation("Calling AddOverrideViewsData() for additional view bag data.");
            AddOverrideViewsData();
            Logger.LogInformation("Done calling AddOverrideViewsData().");
            Logger.LogInformation("Calling MyOnActionExecuted() event.");
            MyOnActionExecuted(context);
            Logger.LogInformation("Handled MyOnActionExecuted() event.");
            LogExecution("Action Executed");

        }

        /// <summary>
        /// If you want to add some values to the view bag without impacting anything else. just override this method.
        /// And add stuffs to view data to viewbag to viewata.
        /// </summary>
        [NonAction]
        protected virtual void AddOverrideViewsData() {}

        /// <summary>
        /// Override this if you need to do something on OnResultExecuted()
        /// </summary>
        /// <param name="contex">The <seealso cref="ResultExecutedContext"/> contex.</param>        
        [NonAction]
        public virtual void MyOnResultExecuted(ResultExecutedContext contex) { }

        /// <summary>
        /// Override this if you need to do something on OnResultExecuting()
        /// </summary>
        /// <param name="contex">The <seealso cref="ResultExecutingContext"/> contex.</param>        
        [NonAction]
        public virtual void MyOnResultExecuting(ResultExecutingContext contex) { }

        /// <summary>
        /// Override this if you need to do something on OnActionExecuting()
        /// </summary>
        /// <param name="contex">The <seealso cref="ActionExecutingContext"/> contex.</param>        
        [NonAction]
        public virtual void MyOnActionExecuting(ActionExecutingContext context) { }

        /// <summary>
        /// Override this if you need to do something on OnActionExecuted()
        /// </summary>
        /// <param name="contex">The <seealso cref="ResultExecutingContext"/> contex.</param>
        [NonAction]
        public virtual void MyOnActionExecuted(ActionExecutedContext context) { }        

    }
}