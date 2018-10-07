using Exico.Shopify.Web.Core.Filters;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Exico.Shopify.Web.Core.Controllers.BaseControllers
{
    /// <summary>
    /// If you want to make sure your controller is available to only those are paid subscribers then extend 
    /// this controller.
    /// <see cref="ABaseAuthorizedController"/>
    /// /// <see cref="RequireSubscription"/>
    /// </summary>
    [ServiceFilter(typeof(RequireSubscription), Order = RequireSubscription.DEFAULT_ORDER)]

    public abstract class ABaseSubscriberController : ABaseAuthorizedController
    {
        protected readonly IPlansReader Plans;
        public ABaseSubscriberController(IPlansReader plansReader, IUserCaching cachedUser, IConfiguration config, IDbSettingsReader settings, ILogger logger) : base(cachedUser, config, settings, logger)
        {
            this.Plans = plansReader;
        }
    }
}
