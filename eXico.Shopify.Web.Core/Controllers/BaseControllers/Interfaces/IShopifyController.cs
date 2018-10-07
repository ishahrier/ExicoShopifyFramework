using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Data.Domain.DBModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces
{
    public enum SHOPIFY_ACTIONS
    {
        HandShake,        
        AuthResult,
        ChargeResult,
        SelectedPlan,
        ChoosePlan
    }

    public interface IShopifyController 
    {
        Task<IActionResult> Handshake(string shop);
        Task<IActionResult> AuthResult(string shop, string code);

        [Authorize]
        Task<IActionResult> ChargeResult(string shop, long charge_id);
        [Authorize]
        Task<IActionResult> ChoosePlan();
        [Authorize]
        Task<IActionResult> SelectedPlan(int planId);
        [Authorize, NonAction]
        IActionResult RedirectAfterSuccessfullLogin();
        [Authorize, NonAction]
        IActionResult RedirectAfterPlanChangePaymentDeclined();
        [Authorize, NonAction]
        IActionResult RedirectAfterSuccessfulUpgrade(string upgradedPlanName);
        [Authorize, NonAction]
        IActionResult RedirectAfterNewUserCancelledPayment(AppUser user);


        [NonAction]
        List<string> ListPermissions();
        [Authorize, NonAction]
        Task DoPostInstallationTasks(AppUser user);
        [Authorize, NonAction]
        Task UserChangedPlan(AppUser user, int newPlainId);
        [NonAction]
        Task UserCancelledPayment(AppUser user, int forPlanId);
        [NonAction]
        Task SendEmailsOnSuccessfullInstallation(AppUser user);
        [Authorize, NonAction]
        Task ProcessWebhooks(AppUser user);

    }
}
