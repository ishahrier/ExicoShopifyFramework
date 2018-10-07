using System.Threading.Tasks;
using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Data.Domain.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces
{
    public enum ACCOUNT_ACTIONS
    {
        Login,
        LogOff,
        LoginErrorHappened,
        LogInHappened,
        LogOffHappened
    }

    public interface IAccountController
    {
        [AllowAnonymous]
        [HttpGet]
        Task<IActionResult> Login(string returnUrl = null);

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        Task<IActionResult> Login(LoginViewModel model, string returnUrl = null);

        [NonAction]
        Task LoginErrorHappened(LoginViewModel data);

        [NonAction]
        Task LogInHappened(AppUser user);

        [HttpPost]
        [ValidateAntiForgeryToken]
        Task<ActionResult> LogOff();

        [NonAction]
        Task LogOffHappened(AppUser user);
    }
}