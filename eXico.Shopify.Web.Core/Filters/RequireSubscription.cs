using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Domain.ShopifyApiModels;

using Exico.Shopify.Data.Framework;
using Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces;
using Exico.Shopify.Web.Core.Helpers;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Filters
{
    /// <summary>
    /// Thesea are the billing charge status values from shopify.
    /// </summary>
    public enum SHOPIFY_CHARGE_STATUS
    {
        frozen,
        active,
        expired,
        pending,
        declined,
        accepted
    }

    /// <summary>
    /// This filter is used in checking if a logged in user's shopify charge id is still valid or not.
    /// It makes exception for admin types of users by not checking the charge status of the charge id.
    /// For admin users it doesn't event check if there is a charge status is present or not, it will just pass.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Filters.IAuthorizationFilter" />
    public class RequireSubscription : Attribute, IAuthorizationFilter,IOrderedFilter
    {
        public const int DEFAULT_ORDER = 2;
        private readonly IShopifyApi _ShopifyApi;
        private readonly IDbSettingsReader _Settings;
        private readonly IShopifyEventsEmailer _Emailer;
        private readonly IUserCaching _UserCache;
        private readonly ILogger<RequireSubscription> _Logger;
        private readonly IDbService<AspNetUser> _UserDbService;

        public int Order { get; set; } 

        public RequireSubscription(IShopifyApi shopifyApi, IDbSettingsReader settings, IShopifyEventsEmailer shopifyEmailer,
                                   IUserCaching userCaching, IDbService<AspNetUser> userDbService, ILogger<RequireSubscription> logger)
        {
            _ShopifyApi = shopifyApi;
            _Settings = settings;
            _Emailer = shopifyEmailer;
            _UserCache = userCaching;
            _Logger = logger;
            _UserDbService = userDbService;
        }
        private RedirectToRouteResult _CreateRedirectResult(string controller, string action)
        {
            RouteValueDictionary rd = new RouteValueDictionary();
            rd.Add("controller", controller);
            rd.Add("action", action);
            return new RedirectToRouteResult(rd);
        }

        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            //already logged in by Identity now check subscription
            if (filterContext.Result == null)
            {
                _Logger.LogInformation("Starting subscription check.");
                var context = filterContext.HttpContext;

                _Logger.LogInformation("Getting current user.");
                AppUser currentUser = _UserCache.GetLoggedOnUser().Result;
                if (currentUser == null)
                {
                    _Logger.LogError("User must be logged on before checking subscription.Redirecting to login page.");
                    //throw new Exception("Subscription check must be done on logged on user. But current user is found null.");
                    filterContext.Result = new RedirectToActionResult(ACCOUNT_ACTIONS.Login.ToString(), _Settings.GetAccountControllerName(), new { });
                }
                else
                {
                    _Logger.LogInformation($"Current user is '{currentUser.MyShopifyDomain}'");

                    //admin must have access token atleast   
                    if (currentUser.ShopIsConnected == false /*&& !currentUser.IsAdmin*/)
                    {
                        _Logger.LogWarning($"User '{currentUser.MyShopifyDomain}' has no shopify access token. Charge status check cannot be done on diconnected shop.Redirecting to '{_Settings.GetShopifyControllerName()}/{SHOPIFY_ACTIONS.HandShake.ToString()}'.");
                        filterContext.Result = _CreateRedirectResult(_Settings.GetShopifyControllerName(), SHOPIFY_ACTIONS.HandShake.ToString());
                    }
                    //billing connected or disconnected, for admin it is never checked
                    else if (currentUser.BillingIsConnected == false && !currentUser.IsAdmin)
                    {
                        _Logger.LogWarning($"User '{currentUser.MyShopifyDomain}' billing charge id is null.Charge status check cannot be done on null charge id.Redirecting to '{_Settings.GetShopifyControllerName()}/{SHOPIFY_ACTIONS.ChoosePlan.ToString()}'.");
                        filterContext.Result = _CreateRedirectResult(_Settings.GetShopifyControllerName(), SHOPIFY_ACTIONS.ChoosePlan.ToString());
                    }
                    else
                    {
                        ShopifyRecurringChargeObject chargeStatus = null;
                        //for admin user if no billing charge id all good, but if theres one then we will look into it
                        if (currentUser.IsAdmin)
                        {
                            _Logger.LogInformation($"Skipping charge status check because user '{currentUser.MyShopifyDomain}' is admin.");
                            chargeStatus = new ShopifyRecurringChargeObject()
                            {
                                Status = SHOPIFY_CHARGE_STATUS.active.ToString()
                            };
                        }
                        else
                        {
                            _Logger.LogInformation($"Checking charge status for user '{currentUser.MyShopifyDomain}'.");
                            try
                            {
                                chargeStatus = Task.Run(() => _ShopifyApi.GetRecurringChargeAsync(currentUser.MyShopifyDomain, currentUser.ShopifyAccessToken, currentUser.ShopifyChargeId.Value)).Result;
                            }
                            catch (Exception ex)
                            {
                                _Logger.LogError($"Error occurred duing GetRecurringChargeAsync() call.{ex.Message}.{ex.StackTrace}");
                                throw ex;                                
                            }                            
                        }

                        if (chargeStatus.Status == SHOPIFY_CHARGE_STATUS.accepted.ToString() || chargeStatus.Status == SHOPIFY_CHARGE_STATUS.active.ToString())
                        {
                            _Logger.LogInformation($"Require subscription passed for user '{currentUser.MyShopifyDomain}'");
                        }
                        else
                        {
                            _Emailer.InActiveChargeIdDetectedAsync(currentUser, chargeStatus.Status);

                            if (chargeStatus.Status == SHOPIFY_CHARGE_STATUS.declined.ToString() ||
                                chargeStatus.Status == SHOPIFY_CHARGE_STATUS.expired.ToString() ||
                                chargeStatus.Status == SHOPIFY_CHARGE_STATUS.pending.ToString())
                            {
                                _Logger.LogWarning($"Require subscription did not pass for user '{currentUser.MyShopifyDomain}'");
                                _Logger.LogWarning($"User '{currentUser.MyShopifyDomain}' has declined/expired/pending charge status.");
                                _Logger.LogWarning($"Unsetting charge info for user '{currentUser.MyShopifyDomain}'.");
                                UserDbServiceHelper.UnSetUserChargeInfo(_UserDbService, currentUser.Id);
                                _Logger.LogWarning($"Removing user '{currentUser.MyShopifyDomain}' from cache.");
                                _UserCache.ClearLoggedOnUser();//resset cache so that next try makes BillingIsConnected = false                        
                                var handShakeAction = SHOPIFY_ACTIONS.HandShake.ToString();
                                _Logger.LogWarning($"Redirecting user '{currentUser.MyShopifyDomain}' to '{_Settings.GetShopifyControllerName()}/{handShakeAction}'.");
                                filterContext.Result = _CreateRedirectResult(_Settings.GetShopifyControllerName(), handShakeAction);
                            }
                            else if (chargeStatus.Status == SHOPIFY_CHARGE_STATUS.frozen.ToString())
                            {
                                _Logger.LogError($"User '{currentUser.MyShopifyDomain}' has frozen shopify store account. Throwing error.");
                                throw new UnauthorizedAccessException("Your shopify account is frozen.Once shopify unfreezes your store account you will be able to use this app again.");
                            }
                        }
                    }
                }
            }
        }
    }
}
