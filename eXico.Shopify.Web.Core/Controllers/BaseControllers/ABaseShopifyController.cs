using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Domain.ShopifyApiModels;
using Exico.Shopify.Data.Framework;
using Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces;
using Exico.Shopify.Web.Core.Extensions;
using Exico.Shopify.Web.Core.Helpers;
using Exico.Shopify.Web.Core.Modules;
using Exico.Shopify.Web.Core.Plugins.WebMsg;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Controllers.BaseControllers
{
    /// <summary>
    /// This class is the class that handles the shopify auth process mainly.
    /// It also has the app entry point method, plan selection and handling methods etc.
    /// It extends <c>ABaseController</c> and implements <c>IShopifyController</c>
    /// <seealso cref="ABaseController"/>
    /// <seealso cref="IShopifyApi"/>
    /// </summary>
    public abstract class ABaseShopifyController : ABaseController, IShopifyController
    {
        protected readonly IPlansReader PlanReader;
        protected readonly SignInManager<AspNetUser> SignInManager;
        protected readonly UserManager<AspNetUser> UserManager;
        protected readonly IGenerateUserPassword PassGenerator;
        protected readonly IWebMessenger WebMsg;

        protected readonly IShopifyApi ShopifyAPI;
        protected readonly IDbService<AspNetUser> UserDbService;
        protected readonly IShopifyEventsEmailer Emailer;
        protected readonly IUserCaching UserCache;
        private readonly IAppSettingsAccessor AppSettings;

        public ABaseShopifyController(
            IAppSettingsAccessor appSettings,
            IDbService<AspNetUser> userService,
            IPlansReader planReader,
            SignInManager<AspNetUser> signInManager,
            UserManager<AspNetUser> userManager,
            IGenerateUserPassword passwordGenerator,
            IUserCaching userCache,
            IShopifyEventsEmailer emailer,
            IWebMessenger webMsg,
            IShopifyApi shopifyApi,
            IConfiguration config,
            IDbSettingsReader settings,
            ILogger logger
            ) : base(config, settings, logger)
        {
            PlanReader = planReader;
            SignInManager = signInManager;
            UserManager = userManager;
            PassGenerator = passwordGenerator;
            WebMsg = webMsg;
            ShopifyAPI = shopifyApi;
            UserDbService = userService;
            Emailer = emailer;
            UserCache = userCache;
            AppSettings = appSettings;
        }


        #region Shopify api handlers
        /// <summary>
        /// This is the method that is called first when user tries to access or install your app.
        /// This method starts the installation process (by sending to authrization url) if user is not subscribed already otherwise
        /// tries to auto login the user and sends to the landing page(dashboard).
        /// </summary>
        /// <param name="shop">The store (URL) that is trying to access app.</param>
        /// <returns></returns>
        public virtual async Task<IActionResult> Handshake(string shop)
        {
            using (Logger.BeginScope(new { Shop = shop }))
            {
                try
                {
                    Logger.LogInformation("Checking request authenticity.");
                    if (ShopifyAPI.IsAuthenticRequest(Request))
                    {
                        Logger.LogInformation("Request is authentic.");
                        Logger.LogInformation("Checking if shop is authorized.");
                        if (UserDbServiceHelper.ShopIsAuthorized(UserDbService, shop))
                        {
                            Logger.LogInformation("Shop is already authrorized.Calling _ProceedWithShopExists.");
                            return await _ProceedWithShopExists(shop);
                        }
                        else
                        {
                            Logger.LogWarning("Shop is NOT authrorized.Either new shop or imcomplete installation from previous attempt.So let's install it.");
                            var handlerUrl = ShopifyUrlHelper.GetAuthResultHandlerUrl(Settings);
                            Logger.LogInformation("Getting permission list.");
                            var permissions = ListPermissions();
                            Logger.LogInformation("Permission list acquiring is done. {@list}.", permissions);
                            Logger.LogInformation("Getting authorization url.");
                            var authUrl = ShopifyAPI.GetAuthorizationUrl(shop, permissions, handlerUrl);
                            Logger.LogInformation($"Redirecting to authrization url '{authUrl}'.");
                            return Redirect(authUrl.ToString());
                        }
                    }
                    else
                    {
                        Logger.LogWarning("Request is NOT authentic.Throwing Exception.");
                        throw new Exception("Request is not authentic.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning("Error occurred while handshaking.");
                    throw ex;
                }
            }
        }

        /// <summary>
        /// This method mainly tries to generate access token, creates user record, creates uninstall web hook and finally
        /// sends to the page where user can choose plan.
        /// </summary>
        /// <param name="shop">The store (URL) that is trying to access app.</param>
        /// <param name="code">Authrization code generated and send from shopify that will be used to gnerate access code</param>
        /// <returns></returns>
        public virtual async Task<IActionResult> AuthResult(string shop, string code)
        {
            using (Logger.BeginScope(new { Shop = shop, Code = code }))
            {
                try
                {
                    Logger.LogInformation("Checking request authenticity.");
                    if (ShopifyAPI.IsAuthenticRequest(Request))
                    {
                        Logger.LogInformation("Request is authentic.");
                        Logger.LogInformation("Checking if shop is authorized.");
                        if (UserDbServiceHelper.ShopIsAuthorized(UserDbService, shop))
                        {
                            Logger.LogInformation("Shop is authrorized.Calling _ProceedWithShopExists.");
                            return await _ProceedWithShopExists(shop);
                        }
                        else
                        {
                            string accessToken = "";
                            try
                            {
                                Logger.LogWarning("Shop is NOT authrorized.Requesting authentication (access) token via api.");
                                accessToken = await ShopifyAPI.Authorize(shop, code);
                            }
                            catch (Exception ex)
                            {
                                Logger.LogCritical("Shopify did not authorize.No access code received.Throwing exception.");
                                throw new Exception("Shopify did not authorize.No access code received.", ex);
                            }
                            Logger.LogInformation($"Rceived access token '{accessToken}'");
                            Thread.Sleep(500);
                            /*Get shop object cause we need the shop email address*/
                            ShopifyShopObject shopObj = null;
                            try
                            {
                                Logger.LogInformation("Request shopify shop obj using access token.");
                                shopObj = await ShopifyAPI.GetShopAsync(shop, accessToken);
                            }
                            catch (Exception ex)
                            {
                                Logger.LogCritical("Could not retrive shop info obj.");
                                throw new Exception("Could not retrive shop info obj.", ex);
                            }

                            Logger.LogInformation($"Received shop info obj. Shop '{shopObj.Domain}' and email '{shopObj.Email}'");

                            #region User Creation
                            //Generate password
                            Logger.LogInformation("Generating password using shop info obj.");
                            string password = PassGenerator.GetPassword(new PasswordGeneratorInfo(shopObj.MyShopifyDomain, shopObj.Email));
                            Logger.LogInformation($"Successfully generated a password '{password}'.");
                            /*Now create an account */
                            Logger.LogInformation("Creating user account using shop info and password.");
                            var userCreation = await UserManager.CreateAppUser(shopObj.MyShopifyDomain, shopObj.Email, accessToken, password);
                            #endregion

                            if (userCreation.Succeeded)
                            {
                                AppUser user = await UserDbServiceHelper.GetUserByShopDomain(UserDbService, shopObj.MyShopifyDomain);
                                Logger.LogInformation($"Successfully created user for the shop.User id '{user.Id}'");

                                #region Uninstall hook creation
                                //As soon as user is created , create the uninstall hook
                                string uninstallCallback = "";
                                try
                                {
                                    Logger.LogInformation("Trying to find app/uninstalled topic in the web hook definition list in the appsettings.json file.");
                                    List<WebHookDefinition> whDefList = AppSettings.BindObject<List<WebHookDefinition>>("WebHooks", Config);

                                    if (whDefList.Count > 0)
                                    {

                                        var def = whDefList.Where(x => x.Topic.ToLower() == "app/uninstalled").FirstOrDefault();
                                        if (def.Topic == string.Empty)
                                        {
                                            Logger.LogWarning("List of webhooks found in the appsettings file but no app/uninstalled topic found.");
                                            uninstallCallback = ShopifyUrlHelper.GetAppUninstallWebHookUrl(Settings, user.Id);
                                            Logger.LogInformation($"Using system default uninstall callback url {uninstallCallback}.");

                                        }
                                        else
                                        {
                                            Logger.LogInformation($"Found app/uninstalled topic call in the appsettings file.The callback url is {def.Callback}.");
                                            uninstallCallback = ShopifyUrlHelper.GetAppUninstallWebHookUrl(def.Callback, user.Id);
                                        }
                                    }
                                    else
                                    {
                                        Logger.LogInformation("No weebhooks are defined in the appsettings file.");
                                        uninstallCallback = ShopifyUrlHelper.GetAppUninstallWebHookUrl(Settings, user.Id);
                                        Logger.LogInformation($"Using system default uninstall callback url {uninstallCallback}.");

                                    }

                                    Logger.LogInformation($"Trying to create uninstall web hook with call back url {uninstallCallback}.");
                                    var hook = await ShopifyAPI.CreateWebhookAsync(user.MyShopifyDomain, accessToken, new ShopifyWebhookObject()
                                    {
                                        Address = uninstallCallback,
                                        Topic = "app/uninstalled"
                                    });
                                    Logger.LogInformation($"Successfully created uninstall web hook. Hook id '{hook.Id.Value}'.");

                                }
                                catch (Exception ex)
                                {
                                    LogGenericError(ex);
                                    Logger.LogCritical($"Failed creating uninstall webhook for user id '{user.Id}.The call back url is {uninstallCallback}'.");
                                    Logger.LogInformation("Sending UninstallHookCreationFailedAsync email.");
                                    var response = await Emailer.UninstallHookCreationFailedAsync(user, shopObj.MyShopifyDomain);
                                    if (response) Logger.LogInformation("Successfully sent UninstallHookCreationFailedAsync email.");
                                    else Logger.LogInformation("Could not send UninstallHookCreationFailedAsync email.");
                                    //we dont thorw error here...just gracefully ignore it                                    
                                }
                                #endregion

                                #region Sign in
                                //Now sign in
                                Logger.LogInformation($"Trying to sign in using username '{user.UserName}' and password '{password}'. User id '{user.Id}'");
                                var signInStatus = await SignInManager.PasswordSignInAsync(user.UserName, password, false, lockoutOnFailure: false);
                                if (signInStatus.Succeeded)
                                {
                                    Logger.LogInformation($"Successfully signed in. User id '{user.Id}'");
                                    UserCache.ClearLoggedOnUser();
                                    return RedirectToAction(SHOPIFY_ACTIONS.ChoosePlan.ToString(), Settings.GetShopifyControllerName());
                                }
                                else
                                {
                                    Logger.LogCritical($"Signing in for  User id '{user.Id}' failed.");
                                    throw new Exception("Could not sign you in using app account.Sign in failed!");
                                }
                                #endregion
                            }
                            else
                            {
                                var reasons = $"Reasons: { string.Join(", ", userCreation.Errors) }";
                                Logger.LogCritical($"User creation failed.{reasons}");
                                throw new Exception($"Could not create app user.{reasons}.");
                            }
                        }
                    }
                    else
                    {
                        Logger.LogCritical("Request is not authentic. Throwing error.");
                        throw new Exception("Request is not authentic.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning("Error occurred while executing AuthResult().");
                    LogGenericError(ex);
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Detects the status of the charge the app requested (for a selected plan).
        /// If the status is valid then it tries to activate the charge that is authorized by 
        /// shopify and checks the status.If the activation status is valid then charge related 
        /// information is saved in the database and user has access to the app.
        /// </summary>
        /// <param name="shop">The store (URL) that is trying to access app.</param>
        /// <param name="charge_id"></param>
        /// <returns></returns>
        [Authorize]
        public virtual async Task<IActionResult> ChargeResult(string shop, long charge_id)
        {
            using (Logger.BeginScope(new { Shop = shop, ChargeId = charge_id }))
            {

                try
                {
                    Logger.LogInformation("Getting current user.");
                    var user = await UserDbServiceHelper.GetAppUserByIdAsync(UserDbService, UserInContextHelper.GetCurrentUserId(HttpContext));
                    Logger.LogInformation($"Found user. Id is '{user.Id}'");
                    //to detect if its an upgrade we need users previous plan id, 0 means no previous plan.
                    Logger.LogInformation($"Getting previous plan info, if any.");
                    var previousPlan = user.GetPlanId();
                    Logger.LogInformation($"Previous plan ID is set to '{previousPlan}'.");
                    PlanAppModel newPlan = null;
                    ShopifyRecurringChargeObject charge = null;

                    try
                    {
                        Logger.LogInformation($"Retriving recurring charge info (before activation). Access token '{user.ShopifyAccessToken}' and charge id '{charge_id}'.");
                        charge = await ShopifyAPI.GetRecurringChargeAsync(user.MyShopifyDomain, user.ShopifyAccessToken, charge_id);
                        Logger.LogInformation($"Successfully retrived recurring charge info (before activation).Id is '{charge.Id}'.Associated plan name '{charge.Name}' and price '{charge.Price}'.");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error retriving recurring charge info (before activation).");
                        WebMsg.AddTempDanger(this, "Could not retrive charge status by charge id via shopify api (before activation).");
                        return RedirectToAction(SHOPIFY_ACTIONS.ChoosePlan.ToString(), Settings.GetShopifyControllerName());
                    }

                    newPlan = PlanReader[charge.Name];

                    if (newPlan != null)
                    {
                        Logger.LogInformation($"New plan ID is set to '{newPlan.Id}'.");
                        Logger.LogInformation($"Recurring charge status (before activation) is '{charge.Status}'.");
                        if (charge.Status == "accepted")
                        {
                            //Lets activate the charge
                            try
                            {
                                Logger.LogInformation("Trying to activate the recurring charge.");
                                await ShopifyAPI.ActivateRecurringChargeAsync(user.MyShopifyDomain, user.ShopifyAccessToken, charge_id);
                                Logger.LogInformation("Recurring charge activationon is done.");
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError(ex, $"Recurring charge activation failed.Redirecting to '{SHOPIFY_ACTIONS.ChoosePlan.ToString()}' action.");
                                WebMsg.AddTempDanger(this, "Could not activate the recurring charge via shopify api.Please try again.");
                                return RedirectToAction(SHOPIFY_ACTIONS.ChoosePlan.ToString(), Settings.GetShopifyControllerName());
                            }

                            //Lets check if we were sucessful activating the charge
                            try
                            {
                                Logger.LogInformation("Checking recurring charge status after activation.");
                                charge = await ShopifyAPI.GetRecurringChargeAsync(user.MyShopifyDomain, user.ShopifyAccessToken, charge_id);
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError(ex, "Error getting recurring charge status after activation.");
                                WebMsg.AddTempDanger(this, "Could not retrieve charge status by id via shopify api (after activation). Please try again or contact support");
                                return RedirectToAction(SHOPIFY_ACTIONS.ChoosePlan.ToString(), Settings.GetShopifyControllerName());
                            }

                            Logger.LogInformation($"Recurring Charge Status after activation is '{charge.Status}'.");
                            //if we were succesful
                            if (charge.Status == "active")
                            {
                                Logger.LogInformation($"Saving user payment information. User id '{user.Id}', charge id '{charge_id}' , plan id '{newPlan.Id}' and billing on '{charge.BillingOn}'.");
                                var updateResult = UserDbServiceHelper.SetUsersChargeInfo(UserDbService, user.Id, charge_id, newPlan.Id, charge.BillingOn);
                                if (!updateResult)
                                {
                                    Logger.LogCritical($"Could not save user payment information.Redirecting to '{SHOPIFY_ACTIONS.ChoosePlan.ToString()}' action.");
                                    await Emailer.UserPaymentInfoCouldNotBeSavedAsync(user, charge_id, charge.Name);
                                    //this.LogCoreException(new CoreException(CoreErrorType.APP_COULD_NOT_SAVE_CHARGE_INFO, errMessage + $".Activated Charge Id {charge.Id}", user, shop));
                                    WebMsg.AddTempDanger(this, "Could not save your payment confirmation in our db.Please try again.");
                                    return RedirectToAction(SHOPIFY_ACTIONS.ChoosePlan.ToString(), Settings.GetShopifyControllerName());
                                }
                                else
                                {
                                    Logger.LogInformation("Succesfully saved user payment information.");
                                    Logger.LogInformation("Refreshing user cache data now.");                                    
                                    user = await UserCache.GetLoggedOnUser(true);
                                    Logger.LogInformation("Now detecting installation type.");
                                    if (previousPlan > 0 && previousPlan != newPlan.Id)//its an upgrade
                                    {
                                        Logger.LogInformation("Installation was an upgrade type for existing store. Redirecting to RedirectAfterSuccessfulUpgrade().");
                                        await UserChangedPlan(user, newPlan.Id);//handle upgrade event
                                        return RedirectAfterSuccessfulUpgrade(charge.Name);//now redirect
                                    }
                                    else//new installation
                                    {
                                        Logger.LogInformation("Installation was for a new store.");
                                        Logger.LogInformation("Now handling post installation tasks by calling DoPostInstallationTasks().");
                                        await DoPostInstallationTasks(user);//handle new installation event
                                        Logger.LogInformation("Done handling post installation tasks. ");
                                        await SendEmailsOnSuccessfullInstallation(user);//send emails
                                        Logger.LogInformation($"Now processing all webhooks defined in the appsettings.json by calling ProcessWebHooksCreation().");
                                        await ProcessWebhooks(user);
                                        Logger.LogInformation("Done processing webhooks defined in appsettings.json.");
                                        Logger.LogInformation("Now redirecting after successfull sign in.");
                                        return RedirectAfterSuccessfullLogin();//now redirect
                                    }
                                }
                            }
                            else /*if status is not active*/
                            {
                                Logger.LogCritical($"SHopify could not activate the recurring charge. Redirecting to '{SHOPIFY_ACTIONS.ChoosePlan.ToString()}' action.");
                                WebMsg.AddTempDanger(this, "Shopify did not activate the recurring payment this app requested. Please try again by choosing a plan.");
                                return RedirectToAction(SHOPIFY_ACTIONS.ChoosePlan.ToString(), Settings.GetShopifyControllerName());
                            }
                        }
                        else if (charge.Status == "declined")
                        {
                            Logger.LogCritical("Recurring charge was declined (before activation).Probably user declined payment.");
                            Logger.LogInformation("Calling UserCancelledPayment() event.");
                            await UserCancelledPayment(user, newPlan.Id);
                            Logger.LogInformation("Done handling UserCancelledPayment() event.");
                            if (user.GetPlanId() <= 0)
                            {
                                Logger.LogWarning("Redirecting to RedirectAfterNewUserCancelledPayment() as payment cancelled.");
                                return RedirectAfterNewUserCancelledPayment(user);
                            }
                            else
                            {
                                Logger.LogWarning("Redirecting to RedirectAfterPlanChangePaymentDeclined() as payment declined");
                                return RedirectAfterPlanChangePaymentDeclined();
                            }
                        }
                        else
                        {
                            Logger.LogCritical($"Recurring charge was not accepted by shopify (before activation). Redirecting to '{SHOPIFY_ACTIONS.ChoosePlan.ToString()}' action.");
                            WebMsg.AddTempDanger(this, "Payment was not accepted by shopify. Please try again.");
                            return RedirectToAction(SHOPIFY_ACTIONS.ChoosePlan.ToString(), Settings.GetShopifyControllerName());
                        }
                    }
                    else
                    {
                        Logger.LogCritical($"Recurring charge's plan is not found in the loaded db plan list.Redirecting to '{SHOPIFY_ACTIONS.ChoosePlan.ToString()}' action.");
                        WebMsg.AddTempDanger(this, "Could not retrieve plan information.Please try again.");
                        return RedirectToAction(SHOPIFY_ACTIONS.ChoosePlan.ToString(), Settings.GetShopifyControllerName());//let the user try again
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning("Error occurred while executing ChargeResult().");
                    LogGenericError(ex);
                    throw ex;
                }

            }
        }

        /// <summary>
        /// Provides user with a list of plans to choose from. 
        /// Note that if the user is admin and the user ip is the list of 
        /// privileged ips only then the "dev" plans are shown in this page.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public virtual async Task<IActionResult> ChoosePlan()
        {
            try
            {
                Logger.LogInformation("Getting user.");
                var user = await UserDbServiceHelper.GetAppUserByIdAsync(UserDbService, UserInContextHelper.GetCurrentUserId(HttpContext));
                var planStartId = user.GetPlanId();
                ViewBag.PrePlan = planStartId > 0 ? true : false;
                Logger.LogInformation($"Got user. User ID '{user.Id}', existing plan/plan start ID '{planStartId}'.");
                //Dev plans are only included if ip is privileged and user is admin
                var ipIsPriviledged = IPAddressHelper.IsCurrentUserIpPrivileged(HttpContext, Settings);
                var isAdmin = user.IsAdmin; ;
                Logger.LogInformation($"User IP is priviledged : '{ipIsPriviledged}'.");
                Logger.LogInformation($"User is admin : '{isAdmin}'.");
                bool includeDev = ipIsPriviledged && isAdmin;
                Logger.LogInformation($"Dev plans are being included : '{includeDev}'");
                Logger.LogInformation("Listing plans now.");
                var plans = PlanReader.GetAvailableUpgrades(planStartId, includeDev);
                Logger.LogInformation($"Total '{plans.Count}' have been listed.");
                Logger.LogInformation($"Choose plan view name is {Views.Shopify.ChoosePlan}.");
                return View(Views.Shopify.ChoosePlan, plans);
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Error occurred while executing ChoosePlan())");
                LogGenericError(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Detects the plan chosen by the user. It can also detect if the user is trying to
        /// do a fresh installation or trying to change previously selected plan. Note that,
        /// only users with privileged IP can downgrade, otherwise only upgrade is allowed.
        /// </summary>
        /// <param name="planId">Id of the selected plan</param>
        /// <returns></returns>
        [Authorize]
        public virtual async Task<IActionResult> SelectedPlan(int planId)
        {
            using (Logger.BeginScope(new { PlanId = planId }))
            {
                try
                {
                    Logger.LogInformation("Getting user");
                    var user = await UserDbServiceHelper.GetAppUserByIdAsync(UserDbService, UserInContextHelper.GetCurrentUserId(HttpContext));
                    string domain = user.MyShopifyDomain;
                    string token = user.ShopifyAccessToken;
                    /*user plan id = 0  means that customer is new*/
                    int userPlanId = user.GetPlanId();
                    Logger.LogInformation($"Got user.User ID '{user.Id}', domain '{user.MyShopifyDomain}', token '{user.ShopifyAccessToken}' and Plan Id '{user.PlanId}'.");
                    //privileged ip holders can downgrade or upgrade plan, others are upgrade only
                    var validUpgrade = planId >= userPlanId;
                    var priviledgedUser = IPAddressHelper.IsCurrentUserIpPrivileged(HttpContext, Settings);

                    Logger.LogInformation($"Selected is a valid upgrade : {validUpgrade}");
                    Logger.LogInformation($"Selector's IP is priviledged: {priviledgedUser}");

                    if (validUpgrade || priviledgedUser)
                    {
                        Logger.LogInformation("Plan selection is approved.");
                        var plan = PlanReader[planId];
                        if (plan != null && plan.Id > 0)
                        {
                            Logger.LogInformation($"Found plan for the selected ID. Plan Name '{plan.Name}'.");
                            var charge = new ShopifyRecurringChargeObject()
                            {
                                Name = plan.Name,
                                Price = plan.Price,
                                TrialDays = plan.TrialDays,
                                Test = plan.IsTest,
                                ReturnUrl = ShopifyUrlHelper.GetChargeResultHandlerUrl(Settings),
                            };
                            try
                            {
                                Logger.LogInformation("Creating recurring charge via api for selected plan.");
                                charge = await ShopifyAPI.CreateRecurringChargeAsync(domain, token, charge);
                                Logger.LogInformation($"Successfully created recurring charge. Redirecting to confirmation URL '{charge.ConfirmationUrl}'.");
                                return Redirect(charge.ConfirmationUrl);
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError(ex, $"Failed creating recurring charge for the selected plan.Redirecting to '{SHOPIFY_ACTIONS.ChoosePlan.ToString()}' action.");
                                WebMsg.AddTempDanger(this, "Could not create a recurring charge record/confirmation url via shopify api. Please try again.", false, false);
                                return RedirectToAction(SHOPIFY_ACTIONS.ChoosePlan.ToString(), Settings.GetShopifyControllerName());
                            }
                        }
                    }

                    //if we are here then it is an invalid plan
                    Logger.LogWarning($"Selection is not approved.Redirecting to '{SHOPIFY_ACTIONS.ChoosePlan.ToString()}' adction");
                    WebMsg.AddTempDanger(this, "Invalid Plan Selected", false, false);
                    return RedirectToAction(SHOPIFY_ACTIONS.ChoosePlan.ToString(), Settings.GetShopifyControllerName());
                }
                catch (Exception ex)
                {
                    Logger.LogWarning("Error occurred while executing SelectedPlan())");
                    LogGenericError(ex);
                    throw ex;
                }
            }
        }
        #endregion

        #region Internal Helpers

        protected override string GetPageTitle() => "Shopify";
        protected virtual async Task<IActionResult> _ProceedWithShopExists(string myShopifyDomain)
        {
            AppUser user = await UserDbServiceHelper.GetUserByShopDomain(UserDbService, myShopifyDomain);//this must exists..otherwise this method wont be called

            var password = PassGenerator.GetPassword(new PasswordGeneratorInfo(user.UserName, user.Email));
            var status = await SignInManager.PasswordSignInAsync(user.UserName, password, false, lockoutOnFailure: false);
            if (status.Succeeded)
            {
                await UserCache.SetLoggedOnUserInCache();
                return RedirectAfterSuccessfullLogin();
            }
            else
            {
                //throw LogCoreException(new CoreException(CoreErrorType.APP_COULD_NOT_SIGN_IN, baseMessage + ".Reason: " + status.ToString(), user, shop));
                throw new Exception($"Could not sign into your account.Please try again or contact {AppName} support.");
            }
        }

        #endregion

        #region Redirect Helpers - Overridable

        /// <summary>
        /// By default redirect to shops admin page is a new user cancelled the payment.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Authorize, NonAction]
        public virtual IActionResult RedirectAfterNewUserCancelledPayment(AppUser user)
        {
            var url = ShopifyUrlHelper.GetAdminUrl(user.MyShopifyDomain);
            Logger.LogInformation($"Redirecting to {url}.");
            return Redirect(url);
        }
        /// <summary>
        /// By default redirects to app dashboard when a user cancelled the payment while changing (upgrade or downgrade) current plan.        
        /// </summary>
        /// <returns></returns>
        [Authorize, NonAction]
        public virtual IActionResult RedirectAfterPlanChangePaymentDeclined()
        {
            WebMsg.AddTempDanger(this, "Changing your current plan did not succeeed.");
            var a = DASHBOARD_ACTIONS.Index.ToString();
            var c = Settings.GetAppDashboardControllerName();
            Logger.LogInformation($"Redirecting to {c}/{a}.");
            return RedirectToAction(a, c);
        }

        /// <summary>
        /// By default redirects to app dashboard when a user successfully changed (upgraded or downgraded) current plan. 
        /// </summary>
        /// <param name="upgradedPlanName"></param>
        /// <returns></returns>
        [Authorize, NonAction]
        public virtual IActionResult RedirectAfterSuccessfulUpgrade(string upgradedPlanName)
        {
            WebMsg.AddTempSuccess(this, "You Successfully Upgraded Your Plan To <b>" + upgradedPlanName + "</b>");
            var a = DASHBOARD_ACTIONS.Index.ToString();
            var c = Settings.GetAppDashboardControllerName();
            Logger.LogInformation($"Redirecting to {c}/{a}.");
            return RedirectToAction(a, c);
        }

        /// <summary>
        /// By default redirects to dashboard after user successfully logs in to the application.
        /// </summary>
        /// <returns></returns>
        [Authorize, NonAction]
        public virtual IActionResult RedirectAfterSuccessfullLogin()
        {
            var a = DASHBOARD_ACTIONS.Index.ToString();
            var c = Settings.GetAppDashboardControllerName();
            Logger.LogInformation($"Redirecting to {c}/{a}.");
            return RedirectToAction(a, c);
        }
        #endregion

        #region Events - Overridables
        /// <summary>
        /// This method is called when plan change occurs.
        /// As a default action on plan change an email is sent out to the admin.
        /// </summary>
        /// <param name="user">User in context</param>
        /// <param name="newPlanId">The upgraded (or downgraded) plan id </param>
        /// <returns></returns>
        [Authorize, NonAction]
        public virtual async Task UserChangedPlan(AppUser user, int newPlanId)
        {

            Logger.LogInformation("Sending UserChangedPlan email for user {@user} and new plan id {@pid}.", user, newPlanId);
            var response = await Emailer.UserUpgradedPlanAsync(user, newPlanId);
            if (response) Logger.LogInformation("Successfully sent UserChangedPlan email.");
            else Logger.LogInformation("Could not send UserChangedPlan email.");

        }

        /// <summary>
        /// This method is called when a new installation completes and before sending emails (<see cref="SendEmailsOnSuccessfullInstallation(AppUser)"/>).
        /// Override this if you need to create after instllation weekbhooks or anything that need to be done post installation .
        /// </summary>
        /// <param name="user">User in context</param>
        /// <returns></returns>
        [Authorize, NonAction]
        public virtual async Task DoPostInstallationTasks(AppUser user) { }

        /// <summary>
        /// Sends out two kinds of emails; one to admin and the second one (welcome email) to the the user who installed.
        /// Override this if you need to have control over sending emails.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [NonAction]
        public virtual async Task SendEmailsOnSuccessfullInstallation(AppUser user)
        {
            //send to app devs/admins
            Logger.LogInformation("Sending UserInstlledApplication dev notification email. {@user}");
            var response = await Emailer.UserInstalledAppAsync(user);
            if (response) Logger.LogInformation("Successfully sent UserInstlledApplication dev notification email.");
            else Logger.LogInformation("Could not send UserInstlledApplication dev notification email.");
            //send to user/customer
            Logger.LogInformation("Sending welcome email for user {@user}");
            response = await Emailer.UserWelcomeEmailAsync(user.Email);
            if (response) Logger.LogInformation("Successfully sent welcome email.");
            else Logger.LogInformation("Could not send welcome email.");
        }

        /// <summary>
        /// This is called when new user cancelled payment during installation.
        /// As default action it sends out a notification email to admin, if you want to do something else
        /// then override it.
        /// </summary>
        /// <param name="user">User in context</param>
        /// <param name="forPlanId">The plan id</param>
        /// <returns></returns>
        [NonAction]
        public virtual async Task UserCancelledPayment(AppUser user, int forPlanId)
        {
            try
            {
                Logger.LogInformation("Calling ClearLoggedOnUser() to clear user cache.");
                UserCache.ClearLoggedOnUser();
                Logger.LogInformation("Done cleaning user cache.");
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Error occurred while calling ClearLoggedOnUser() to clear cache.Gracefully ignoring.");
                LogGenericError(ex);
            }

            try
            {
                Logger.LogInformation("Calling SignInManager.SignOutAsync() to log user off.");
                await SignInManager.SignOutAsync();
                Logger.LogInformation("Done signing off.");
            }
            catch (Exception ex)
            {

                Logger.LogWarning("Error occurred while calling SignInManager.SignOutAsync() to log off.Gracefully ignoring.");
                LogGenericError(ex);
            }
        }

        /// <summary>
        /// Overrride this method and porvide the list of permissions your app needs.
        /// By default it lists the permissions found in the appsettings.json file, the setting name is Permissions.
        /// Override this method with the required permissions for your app.
        /// </summary>
        /// <returns><c>List<string></c> as a list of permissions</returns>
        [NonAction]
        public virtual List<string> ListPermissions()
        {
            Logger.LogInformation("Listing permissions from appsettings file.");
            List<string> permissions = AppSettings.BindObject<List<string>>("Permissions", Config);            
            if (permissions.Count <= 0)
            {
                var e = new Exception("No store permissions/scopes have been set for this application.");
                throw e;
            }
            else
            {
                Logger.LogInformation("Found " + permissions.Count + " permissions. {@list}", permissions);
            }
            return permissions;
        }
        /// <summary>
        /// Creates all webhooks listed in the appsettings.json 
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Authorize, NonAction]
        public virtual async Task ProcessWebhooks(AppUser user)
        {
            Logger.LogInformation("Loading webhooks definition list from appsettings.");
            List<WebHookDefinition> whDefList = AppSettings.BindObject<List<WebHookDefinition>>("WebHooks", Config);            
            if (whDefList.Count > 0)
            {
                Logger.LogInformation($"Found {whDefList.Count} definitions.");
                foreach (var hook in whDefList)
                {
                    try
                    {
                        Logger.LogInformation($"Processing web hook def. Topic {hook.Topic} and url {hook.Callback}.");
                        //because we have taken care of app uninstalled topic seperately.
                        if (hook.Topic == "app/uninstalled")
                        {
                            Logger.LogInformation("App/uninstalled topic detected. Skipping processing.");
                        }
                        else
                        {
                            await ShopifyAPI.CreateWebhookAsync(user.MyShopifyDomain, user.ShopifyAccessToken, new ShopifyWebhookObject()
                            {
                                Address = hook.Callback,
                                Topic = hook.Topic
                            });
                            Logger.LogInformation($"Done processing web hook def. Topic {hook.Topic} and url {hook.Callback}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, $"Failed processing web hook def. Topic {hook.Topic} and url {hook.Callback}.");
                    }
                }

            }
            else
            {
                Logger.LogInformation($"Found {whDefList.Count} definitions.");
            }
        }
        #endregion


    }
}
