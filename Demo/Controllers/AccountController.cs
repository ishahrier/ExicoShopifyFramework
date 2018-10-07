
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Web.Core.Controllers.BaseControllers;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Demo.Controllers
{
    public class AccountController : ABaseAccountController
    {
        public AccountController(SignInManager<AspNetUser> signInManager, IUserCaching cachedUser, IConfiguration config, IDbSettingsReader settings, ILogger<AccountController> logger) : base(signInManager, cachedUser, config, settings, logger)
        {
        }
    }
}
