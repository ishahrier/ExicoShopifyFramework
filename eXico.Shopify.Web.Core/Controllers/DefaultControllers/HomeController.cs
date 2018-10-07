#if DEBUG
using Exico.Shopify.Web.Core.Controllers.BaseControllers;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Exico.Shopify.Web.Core.Controllers.DefaultControllers
{

    public class HomeController : ABaseHomeController
    {
        public HomeController(IShopifyEventsEmailer emailer, IConfiguration config, IDbSettingsReader settings, ILogger<HomeController> logger) : base(config, settings, logger)
        {
         
        }

    }
}
#endif