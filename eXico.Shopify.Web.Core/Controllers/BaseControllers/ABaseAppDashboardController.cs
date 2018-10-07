using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Domain.ViewModels;
using Exico.Shopify.Data.Framework;
using Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces;
using Exico.Shopify.Web.Core.Modules;
using Exico.Shopify.Web.Core.Plugins.WebMsg;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Controllers.BaseControllers
{
    /// <summary>
    /// This bastract controller is the landing page for subscribed and logged in paid user.
    /// <see cref="ABaseSubscriberController"/>
    /// <see cref="IAppDashBoardController"/>
    /// </summary>
    public abstract class ABaseAppDashBoardController : ABaseSubscriberController, IAppDashBoardController
    {
        protected readonly IShopifyEventsEmailer Emailer;
        protected readonly IDbService<AspNetUser> UsrDbService;
        protected readonly IWebMessenger WebMsg;


        public ABaseAppDashBoardController(IWebMessenger webMSg, IShopifyEventsEmailer emailer, IDbService<AspNetUser> usrDbService, IUserCaching userCache, IPlansReader planReader, IConfiguration config, IDbSettingsReader settings, ILogger logger) : base(planReader, userCache, config, settings, logger)
        {
            Emailer = emailer;
            UsrDbService = usrDbService;
            WebMsg = webMSg;
        }

        /// <summary>
        /// Displays the app support page
        /// </summary>
        /// <returns></returns>        
        public virtual async Task<IActionResult> Support()
        {
            Logger.LogInformation("Getting current user to generate support view.");
            var currentUser = await AppUserCache.GetLoggedOnUser();
            Logger.LogInformation("Found  current user {@user}.", currentUser);
            Logger.LogInformation($"Support view name is {Views.Dashboard.Support}.");
            return View(Views.Dashboard.Support, new ContactUsViewModel()
            {
                ShopDomain = currentUser.MyShopifyDomain,
                FromEmail = currentUser.Email,
                PlanName = Plans[currentUser.GetPlanId()]?.Name
            });
        }

        /// <summary>
        /// Sends support email 
        /// </summary>
        /// <param name="model">Senders information and the msg <seealso cref="ContactUsViewModel"/></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<IActionResult> SendMsg(ContactUsViewModel model)
        {
            Logger.LogInformation("Getting logged on user to send message");
            var user = await AppUserCache.GetLoggedOnUser();
            Logger.LogInformation("Found user is {@user}", user);
            Logger.LogInformation("Sending email.");
            var result = await Emailer.SendSupportEmailAsync(user, model);
            Logger.LogInformation("Email sent was " + (result ? "successful" : "unsuccessful"));
            if (result)
            {
                WebMsg.AddTempSuccessPopUp(this, "Successfully sent the message. Someone will get in touch with you shortly!");                
                Logger.LogInformation($"Now redirecting to {DASHBOARD_ACTIONS.Support.ToString()}");
                return RedirectToAction(DASHBOARD_ACTIONS.Support.ToString(), Settings.GetAppDashboardControllerName());

            }
            else
            {
                WebMsg.AddTempDangerPopUp(this, "Could not send your meesage to the support department.Try again.");
                return View(Views.Dashboard.Support, model);                
            }
            
        }

        /// <summary>
        /// If your current plan doesn't meet the requirement then this page can be shown to suggest the user to upgrade.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IActionResult> ConsiderUpgradingPlan()
        {
            Logger.LogInformation($"ConsiderUpgradingPlan view name is {Views.Dashboard.ConsiderUpgradingPlan}.");
            return View(Views.Dashboard.ConsiderUpgradingPlan);
        }

        /// <summary>
        /// Default app dashboard page 
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IActionResult> Index()
        {
            Logger.LogInformation($"Dashboard index view name is {Views.Dashboard.Index}.");
            return View(Views.Dashboard.Index);
        }

        /// <summary>
        /// This action will be called if you plan doesn't meet requesment for any resource you want to access within the app
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IActionResult> PlanDoesNotAllow()
        {
            Logger.LogInformation($"PlanDoesNotAllow view name is {Views.Dashboard.PlanDoesNotAllow}.");
            return View(Views.Dashboard.PlanDoesNotAllow);
        }
        protected override string GetPageTitle()
        {
            return "Dashboard";
        }
    }
}