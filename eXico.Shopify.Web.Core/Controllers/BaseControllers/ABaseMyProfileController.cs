using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Domain.ShopifyApiModels;
using Exico.Shopify.Data.Domain.ViewModels;
using Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces;
using Exico.Shopify.Web.Core.Modules;
using Exico.Shopify.Web.Core.Plugins.WebMsg;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Controllers.BaseControllers
{
    /// <summary>
    /// This class should be used as the base user profile class.It extends <c>ABaseSubscriberController</c> and implements <c>IMyProfileController</c>
    /// <see cref="ABaseSubscriberController"/>
    /// <see cref="IMyProfileController"/>
    /// </summary>
    public abstract class ABaseMyProfileController : ABaseSubscriberController, IMyProfileController
    {
        protected readonly IWebMessenger WebMsg;
        protected readonly IShopifyApi ShopifyAPI;
        protected readonly SignInManager<AspNetUser> SManager;

        public ABaseMyProfileController(IWebMessenger webMsg, IShopifyApi shopifyApi, IPlansReader plansReader, IUserCaching cachedUser, IConfiguration config, IDbSettingsReader settings, ILogger logger) : base(plansReader, cachedUser, config, settings, logger)
        {
            WebMsg = webMsg;
            ShopifyAPI = shopifyApi;
        }

        /// <summary>
        /// Generates a view with basic profile information (such as billing date, trial expiry, plan name etc tec)
        /// </summary>
        /// <returns></returns>
        public virtual async Task<ActionResult> Index()
        {
            Logger.LogInformation("Getting user to display his profile.");
            var me = await AppUserCache.GetLoggedOnUser();
            Logger.LogInformation("Found user is {@user}", me);
            Logger.LogInformation("Getting  plan data.");
            var plan = Plans[me.GetPlanId()];
            Logger.LogInformation("Found plan is {@plan}", plan);
            Logger.LogInformation("Preparing to get recurring charge info.");
            ShopifyRecurringChargeObject recChargeInfo = null;
            bool getFrmApi = (me.IsAdmin && me.BillingIsConnected) ? false : true;
            if (!getFrmApi) Logger.LogInformation("User is admin and billing is connected so recurring charge info will NOT be loaded.");
            else Logger.LogInformation("Recurring charge info will be read from shopify API.");

            if (getFrmApi)
            {
                try
                {
                    Logger.LogInformation("Requesting recurring charge infor from shopify.");
                    recChargeInfo = await ShopifyAPI.GetRecurringChargeAsync(me.MyShopifyDomain, me.ShopifyAccessToken, me.ShopifyChargeId.Value);
                    Logger.LogInformation("Recieved recurring charge info {@info}.", recChargeInfo);
                }
                catch (Exception ex)
                {
                    recChargeInfo = null;
                    Logger.LogError("Error occurred while reading recurring charge info from shopify.", ex);
                }
            }
            Logger.LogInformation($"My profile index view nmae is {this.Views.MyProfile.Index}.");
            return View(this.Views.MyProfile.Index, new MyProfileViewModel()
            {
                Me = me,
                MyPlan = plan,
                ChargeData = recChargeInfo
            });
        }

        /// <summary>
        /// Checks if changing your plan is possible.
        /// And begins the plan change (upgrade/downgrade) process is requested.
        /// </summary>
        /// <param name="proceed">/if set <c>true</c> then the upgrade process begins (if upgrade/downgrade is possible), otherwise related information only displayed.</param>
        /// <returns></returns>
        public virtual async Task<ActionResult> ChangePlan(bool proceed = false)
        {
            Logger.LogInformation("Getting current user to continue the plan change process.");
            var me = await AppUserCache.GetLoggedOnUser();
            Logger.LogInformation("Found user is {@me}",me);
            Logger.LogInformation("Checking plan change feasibility.");
            var canUpgrage = Plans.CanUpgrade(me.PlanId.Value, me.IsAdmin);
            if (canUpgrage)
            {
                Logger.LogInformation("Plane change is possible.");
                if (proceed)
                {
                    Logger.LogInformation($"Redirecting to {SHOPIFY_ACTIONS.ChoosePlan.ToString()}.");
                    return RedirectToAction(SHOPIFY_ACTIONS.ChoosePlan.ToString(), Settings.GetShopifyControllerName());
                }
                else
                {
                    Logger.LogInformation("Rendering change plan view.");
                    Logger.LogInformation($"Change plan view name is {this.Views.MyProfile.ChangePlan}.");
                    return View(this.Views.MyProfile.ChangePlan);
                }

            }
            else
            {
                Logger.LogWarning("Plane change is not possible already using highest plan.");
                WebMsg.AddTempInfo(this, "You are already using the highest plan.");
                Logger.LogInformation($"Redirecting to {PROFILE_ACTIONS.Index.ToString()}");
                return RedirectToAction(PROFILE_ACTIONS.Index.ToString(), Settings.GetAppMyProfileControllerName());
            }
        }

        protected override string GetPageTitle()
        {
            return "My Profile";
        }
    }
}