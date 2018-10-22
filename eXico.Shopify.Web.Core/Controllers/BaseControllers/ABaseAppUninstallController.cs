using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces;
using Exico.Shopify.Web.Core.Helpers;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Controllers.BaseControllers
{
    /// <summary>
    /// This abstract class has all necessary functionlities to handle the shopify uninstall web hook.
    /// NOTE: shopify web always hook expects success 200 http code) even if the app fails on its side.
    /// <see cref="ABaseController"/>
    /// <see cref="IAppUnInstallController"/>
    /// </summary>
    public abstract class ABaseAppUninstallController : ABaseController, IAppUnInstallController
    {
        protected readonly IShopifyEventsEmailer Emailer;
        protected readonly IShopifyApi ShopifyAPI;
        protected readonly IDbService<AspNetUser> UsrDbService;
        protected readonly IUserCaching UserCache;

        public ABaseAppUninstallController(IUserCaching userCache, IShopifyEventsEmailer emailer, IShopifyApi shopify, IDbService<AspNetUser> usrDbService, IConfiguration config, IDbSettingsReader settings, ILogger logger) : base(config, settings, logger)
        {
            UserCache = userCache;
            Emailer = emailer;
            ShopifyAPI = shopify;
            UsrDbService = usrDbService;
        }

        protected override string GetPageTitle()
        {
            return "App Uninstaller";
        }

        /// <summary>
        /// Called by the uninstall web hook from shopify end
        /// </summary>
        /// <param name="userId">User who uninstalled</param>
        /// <returns></returns>
        /// 
        public virtual async Task<IActionResult> AppUninstalled(string userId)
        {
            Logger.LogInformation("Start handling app/uninstalled webhook.");
            bool isValidRequest = false;
            Logger.LogInformation("Checking webhook authenticity.");
            try
            {
                isValidRequest = await ShopifyAPI.IsAuthenticWebhook(Request);

            }
            catch (Exception ex)
            {
                LogGenericError(ex);
                Logger.LogWarning("Exception occurred during checking of the webhook authenticity. Gracefully ignoring and continueing.");

            }

            if (!isValidRequest)
            {
                Logger.LogWarning("Webhook is not authentic.Still returning 200 OK.");
                return Content("Webhook is not authentic.");//yet its a 200 OK msg
            }
            else
            {
                Logger.LogInformation("Request is authentic.");
                AppUser user = null;
                Logger.LogInformation("Trying to retrieve user data.");
                try
                {
                    user = await UserDbServiceHelper.GetAppUserByIdAsync(UsrDbService, userId);
                }
                catch (Exception ex)
                {
                    LogGenericError(ex);
                    Logger.LogWarning("Exception occurred while retrieving user data. Gracefully ingnoring and continuing.");

                }
                if (user != null)
                {
                    Logger.LogInformation("Found user data. {@user}", user);
                    bool removeSuccess = false;
                    Exception removalException = null;
                    try
                    {
                        Logger.LogInformation("Trying to remove user account.");
                        removeSuccess = UserDbServiceHelper.RemoveUser(UsrDbService, userId);
                    }
                    catch (Exception ex)
                    {
                        LogGenericError(ex);
                        Logger.LogInformation("Error occurred during user account removal. Gracefully ignoring and continuing.");
                        removalException = ex;
                    }

                    if (!removeSuccess)
                    {
                        Logger.LogInformation("Calling CouldNotDeleteUser() event.");
                        await CouldNotDeleteUser(user, removalException ?? new Exception("Reason not known"));
                        Logger.LogInformation("Done handling CouldNotDeleteUser() event.");
                    }
                    else
                    {
                        Logger.LogInformation("user account removal was successfull.");
                        Logger.LogInformation("Calling UserIsDeleted() event.");
                        await UserIsDeleted(user);
                        Logger.LogInformation("Done handling UserIsDeleted() event.");
                    }

                    try
                    {
                        Logger.LogInformation("Trying to clear user cache calling ClearUser().");
                        UserCache.ClearUser(userId);
                        Logger.LogInformation("Done clearning user cache.");
                    }
                    catch (Exception ex)
                    {
                        LogGenericError(ex);
                        Logger.LogWarning("Error occurred during clearning user cache.Ignoring and continuing.");
                    }

                }
                try
                {
                    Logger.LogInformation("Sending out uninstall event email.");
                    var emailRet = await SendUninstallEmail(user);
                    if (emailRet) Logger.LogInformation("Successfully sent out uninstall even email.");
                    else Logger.LogWarning("Error sending uninstall event email.");
                    Logger.LogInformation("Calling UnInstallCompleted() event.");
                    await UnInstallCompleted(user);
                    Logger.LogInformation("Done handling UnInstallCompleted() event.");

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Unhandled exection occurred during app uninstall web hook processing.");
                }
                Logger.LogInformation("Returning 200 OK to satisfy shopify.");
                return Ok(); //shopify wants 200 OK msg
            }
        }

        /// <summary>
        /// Called when uninstall is completed.
        /// NOTE: completed doesn't mean that on app side we were able to do the uninstallation with success.
        /// </summary>
        /// <param name="user">User who uninstalled.<see cref="AppUser"/></param>
        /// <returns></returns>
         [NonAction]
        public virtual async Task UnInstallCompleted(AppUser user) { }


        /// <summary>
        /// Called when user is deleted as a part of the uninstallation process.
        /// Override this if you need to do after deletion (of user/shop) cleanup.
        /// </summary>
        /// <param name="user">User who uninstalled.<see cref="AppUser"/></param>
        /// <returns></returns>
        [NonAction]
        public virtual async Task UserIsDeleted(AppUser user) { }

        /// <summary>
        /// Called when user deletetion (as a part of the uninstallation process) failed.
        /// Override this if you want to do something in case of user/shop deletion error event.
        /// </summary>
        /// <param name="user">User who uninstalled.<see cref="AppUser"/></param>
        /// <returns></returns>
        [NonAction]
        public virtual async Task CouldNotDeleteUser(AppUser user, Exception ex) { }

        /// <summary>
        /// This is called right after <seealso cref="UnInstallCompleted(AppUser)"/>.
        /// This method sends out email that a user/store just uninstalled our app.
        /// </summary>
        /// <param name="user">User who uninstalled.<see cref="AppUser"/></param>
        /// <returns></returns>
         [NonAction]
        public virtual async Task<bool> SendUninstallEmail(AppUser user)
        {
            return await Emailer.UserUnInstalledAppAsync(user);
        }
    }
}