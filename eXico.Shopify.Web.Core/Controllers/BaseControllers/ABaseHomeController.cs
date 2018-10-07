using Exico.Shopify.Data.Domain.ViewModels;
using Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces;
using Exico.Shopify.Web.Core.Helpers;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Controllers.BaseControllers
{

    public abstract class ABaseHomeController : ABaseController, IHomeController
    {
        public ABaseHomeController(IConfiguration config, IDbSettingsReader settings, ILogger logger) : base(config, settings, logger)
        {
        }

        public virtual async Task<ActionResult> Index()
        {
            return View(Views.Home.Index);
        }
        protected override string GetPageTitle()
        {
            return "home";
        }
        [Route("/Error")]
        public virtual async Task<IActionResult> Error()
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>();
            ErrorViewModel model = new ErrorViewModel()
            {
                Exception = exception?.Error,
                GeneralMessage = exception?.Error.Message,
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                HelpMessage = "Please contact support if it keeps happening.",
                HelpLinkHref = "/",

            };
            //these following lines might throw error, and we do not want error while dealing with one alraedy.
            try
            {
                model.SupportEmail = Settings.GetAppSupportEmailAddress();
            }
            catch (System.Exception) { }

            try
            {
                model.ShowExceptionDetails = UserInContextHelper.IsCurrentUserIdAdmin(HttpContext);
            }
            catch (System.Exception) { }

            try
            {
                Logger.LogCritical(exception?.Error, "Critical Error Occured.");
            }
            catch (System.Exception)
            {
            }
            return View(Views.ErrorPage, model);
        }
    }
}