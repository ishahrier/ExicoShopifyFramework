using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Domain.ViewModels;
using Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces;
using Exico.Shopify.Web.Core.Helpers;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Controllers.BaseControllers
{
    /// <summary>
    /// This abstract controller has all the functionalities for logging in and off. 
    /// <see cref="ABaseAuthorizedController"/>
    /// <see cref="IAccountController"/>
    /// </summary>
    [Route("[controller]/[action]")]
    public abstract class ABaseAccountController : ABaseAuthorizedController, IAccountController
    {
        private readonly SignInManager<AspNetUser> _signInManager;
        public ABaseAccountController(SignInManager<AspNetUser> signInManager, IUserCaching cachedUser, IConfiguration config, IDbSettingsReader settings, ILogger logger) : base(cachedUser, config, settings, logger)
        {
            _signInManager = signInManager;
        }
        /// <summary>
        /// Log into the system.
        /// </summary>
        /// <param name="model">Contains user credential information <see cref="LoginViewModel"/></param>
        /// <param name="returnUrl">Return to this URL after login</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            Logger.LogInformation("Processing login request {@model}", model);
            Logger.LogInformation($"Return url is '{returnUrl}'.");
            ViewData["ReturnUrl"] = returnUrl;
            var viewName = this.Views.Account.Login;            
            if (ModelState.IsValid)
            {
                Logger.LogInformation("Valid login model data detected.");
                Logger.LogInformation("Trying to log the user in using signinmanager.PasswordSignInAsync().");
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    Logger.LogInformation($"Login was successfull.");
                    Logger.LogInformation("Calling GetLoggedOnUser(true) to refresh user cache.");
                    await AppUserCache.GetLoggedOnUser(true);
                    Logger.LogInformation("Done calling GetLoggedOnUser(true).");
                    var user = await AppUserCache.GetLoggedOnUser();
                    Logger.LogInformation("Calling LogInHappened() event with user {@user}", user);
                    await LogInHappened(user);
                    Logger.LogInformation("Done handling LogInHappened() event.");
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        Logger.LogInformation($"Now redirecting to '{returnUrl}'.");
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        var controller = Settings.GetAppDashboardControllerName();
                        Logger.LogInformation($"Now redirecting to '{controller}/index'.");
                        return RedirectToAction(DASHBOARD_ACTIONS.Index.ToString(), controller);
                    }
                }
                else
                {
                    Logger.LogWarning("Login was unsuccessful.");
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    Logger.LogInformation("Calling LoginErrorHappened() event.");
                    await LoginErrorHappened(model);
                    Logger.LogInformation("Done handling LoginErrorHappened() event.");                    
                    return View(viewName, model);
                }
            }
            Logger.LogWarning("Invalid login model data detected.");
            return View(viewName, model);
        }

        /// <summary>
        /// Displays the login form
        /// </summary>
        /// <param name="returnUrl">Return to this URL after login</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public virtual async Task<IActionResult> Login(string returnUrl = null)
        {
            Logger.LogInformation("Trying to signout from external scheme first.");
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            Logger.LogInformation("Done signing out from external scheme.");
            Logger.LogInformation($"Login return url is '{returnUrl}'");
            ViewData["ReturnUrl"] = returnUrl;            
            Logger.LogInformation("Displaying login screen now.");
            return View(this.Views.Account.Login);
        }

        /// <summary>
        /// Logs out of the application. cleans user cache.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> LogOff()
        {

            Logger.LogInformation("Getting the current logged on user object first.");
            var me = await AppUserCache.GetLoggedOnUser();
            Logger.LogInformation("Current user is {@me}.", me);
            Logger.LogInformation("Now calling ClearLoggedOnUser() to clear the cache.");
            AppUserCache.ClearLoggedOnUser();
            Logger.LogInformation("Done calling ClearLoggedOnUser().");
            Logger.LogInformation("Now calling SignOutAsync() to do identity framework signout.");
            await _signInManager.SignOutAsync();
            Logger.LogInformation("Done calling SignOutAsync().");
            Logger.LogInformation("Now calling LogOffHappened() event.");
            await LogOffHappened(me);
            Logger.LogInformation("Done handling LogOffHappened() event.");
            var redirectTo = ShopifyUrlHelper.GetAdminUrl(me.MyShopifyDomain);
            Logger.LogInformation($"Now redirecting to '{redirectTo}'.");
            return Redirect(ShopifyUrlHelper.GetAdminUrl(me.MyShopifyDomain));

        }

        /// <summary>
        /// Override this method to do any work when user loggs off.
        /// </summary>
        /// <param name="user">User who logged off  <see cref="AppUser"/></param>
        /// <returns></returns>
        [NonAction]
        public virtual async Task LogOffHappened(AppUser user) { }

        /// <summary>
        /// Override this method to do any work when user loggs in.
        /// </summary>
        /// <param name="user">User who just logged in  <see cref="AppUser"/></param>
        /// <returns></returns>
        [NonAction]
        public virtual async Task LogInHappened(AppUser user) { }
        /// <summary>
        /// Override this method to do any work when user tried to log in but failed.
        /// </summary>
        /// <param name="user">User who could not log in  <see cref="AppUser"/></param>
        /// <returns></returns>
        [NonAction]
        public virtual async Task LoginErrorHappened(LoginViewModel data) { }

        protected override string GetPageTitle()
        {
            return "Account";
        }
    }
}