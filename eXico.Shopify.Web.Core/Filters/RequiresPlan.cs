using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;

namespace Exico.Shopify.Web.Core.Filters
{
    /// <summary>
    /// This custom filter helps to restrict a user or allow a user to a certain functionalities
    /// based on the user's plan and plan option values. Use <see cref="PlanRequirementAppModel"/>
    /// to pass the requirement to this attribute.
    /// <note type="note">
    /// - If you need to check only user's plan then use this way. For example if we want to check if the user has plan with id =1
    /// <code>
    /// [TypeFilter(typeof(RequiresPlan), Arguments = new object[] { 1 }, Order = RequiresPlan.DEFAULT_ORDER)]
    /// </code>
    /// - If you need to check user's plan and also want to check an one of its option's value of the plan then follow this example.
    /// Lets say there is a plan with id =1 and we need to check if one of the options of this plan called 'MaxRun' has a value of 100
    /// <code>[TypeFilter(typeof(RequiresPlan), Arguments = new object[] { 1 , "MaxRun", "100"})]</code>
    /// </note>
    /// <note type="note">
    ///  NOTE: If you provide an option name then you must provide a value  as well.And the value and option name are always string type.
    /// </note>
    /// </summary>
    /// <seealso cref="System.Attribute" />
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Filters.IAuthorizationFilter" />
    public class RequiresPlan : Attribute, IAuthorizationFilter, IOrderedFilter
    {
        public const int DEFAULT_ORDER = 3;
        private readonly ILogger<RequiresPlan> _Logger;
        private readonly IUserCaching _UserCache;
        private readonly IPlansReader _PlanReader;
        private readonly IDbSettingsReader _Settings;
        private readonly PlanRequirementAppModel _Requirement;

        public RequiresPlan(IUserCaching userCache, IPlansReader planReader, IDbSettingsReader settings, ILogger<RequiresPlan> logger, int planId, string optionName = null, string expectedValue = null)
        {
            _Logger = logger;
            _UserCache = userCache;
            _PlanReader = planReader;
            _Settings = settings;
            _Requirement = new PlanRequirementAppModel(planId, optionName, expectedValue);

        }

        public int Order { get; set; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            _Logger.LogInformation($"Started checking requirement {_Requirement.ToString()}");
            if (context.Result == null)
            {
                _Logger.LogInformation("Getting current user.");
                var user = _UserCache.GetLoggedOnUser().Result;
                if (user == null)
                {
                    _Logger.LogWarning("User must be logged on before checking subscription.Redirecting to login page.");
                    context.Result = new RedirectToActionResult(ACCOUNT_ACTIONS.Login.ToString(), _Settings.GetAccountControllerName(), new { });
                }
                else
                {
                    _Logger.LogInformation($"Current user is '{user.MyShopifyDomain}'");
                    if (user.PlanId.HasValue == false)
                    {
                        _Logger.LogError($"Current user '{user.MyShopifyDomain}' doesn't have any valid plan.Throwing error.");
                        throw new Exception("Your account is not associated with any valid plan.Contact Support.");
                    }
                    else
                    {
                        bool requirementMet = false;
                        if (_PlanReader[user.PlanId.Value] == null)
                        {
                            _Logger.LogError($"Current user '{user.MyShopifyDomain}'  plan id ='{user.PlanId.Value}' is not found in the loaded plans list.");
                            throw new Exception($"Current user '{user.MyShopifyDomain}' plan id ='{user.PlanId.Value}' is not found in the loaded plans list.");
                        }
                        else if (_PlanReader[user.PlanId.Value].IsDev)
                        {
                            _Logger.LogInformation("Plan requirement is waved because user has DEV plan.");
                            requirementMet = true;
                        }
                        else
                        {
                            if (user.PlanId.Value != _Requirement.PlanId)
                                _Logger.LogWarning($"User '{user.MyShopifyDomain}' doesn't have required plan id = '{_Requirement.PlanId}'.");
                            else
                            {
                                PlanAppModel userPlan = _PlanReader[user.PlanId.Value];
                                _Logger.LogInformation($"User '{user.MyShopifyDomain}' has plan id = '{userPlan.Id}' and name = '{userPlan.Name}'");

                                if (_Requirement.OptionName != null && _Requirement.ExpectedValue != null)
                                {
                                    if (_PlanReader[userPlan.Id, _Requirement.OptionName]?.OptionValue == _Requirement.ExpectedValue)
                                    {
                                        requirementMet = true;
                                        _Logger.LogInformation($"User '{user.MyShopifyDomain}' has valid plan '{userPlan.Name}' and valid value = '{_Requirement.ExpectedValue}' for option = '{_Requirement.OptionName}'.");
                                    }
                                    else
                                        _Logger.LogWarning($"User '{user.MyShopifyDomain}' plan = '{userPlan.Name}' doesn't have expected value for option ='{_Requirement.OptionName}'.");
                                }
                                else
                                {
                                    _Logger.LogInformation($" User '{user.MyShopifyDomain}' hsa required plan.");
                                    requirementMet = true;
                                }

                            }

                            var controller = context.RouteData.Values["controller"];
                            var action = context.RouteData.Values["action"];
                            if (!requirementMet)
                            {
                                _Logger.LogWarning($"User '{user.MyShopifyDomain}' is denied to '{controller}/{action}' route. Redirecting to app dashboard.");
                                context.Result = new RedirectToRouteResult(
                                                    new RouteValueDictionary()
                                                    {
                                                        { "controller", _Settings.GetAppDashboardControllerName() },
                                                        { "action",  DASHBOARD_ACTIONS.PlanDoesNotAllow.ToString() }
                                                    });
                            }
                            else
                                _Logger.LogInformation($"Requirement met. User '{user.MyShopifyDomain}' is allowed to '{controller}/{action}' route.");
                        }
                    }
                }
            }
        }
    }
}