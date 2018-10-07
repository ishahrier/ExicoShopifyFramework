using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Exico.Shopify.Web.Core.Controllers.BaseControllers
{
    /// <summary>
    /// If you want to make sure you controller is accessible only after login then extend this abstract controller.
    /// <see cref="ABaseController"/>
    /// </summary>
    [Authorize]

    public abstract class ABaseAuthorizedController : ABaseController
    {
        protected readonly IUserCaching AppUserCache;

        public ABaseAuthorizedController(IUserCaching cachedUser,IConfiguration config, IDbSettingsReader settings,ILogger logger) : base(config, settings,logger)
        {            
            this.AppUserCache = cachedUser;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            Store = AppUserCache.GetLoggedOnUser().Result;
            base.OnActionExecuting(context);
        }

    }
}