using Exico.Shopify.Web.Core.Controllers.BaseControllers;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Demo.Controllers
{

    public class HomeController : ABaseHomeController
    {
        public HomeController(IConfiguration config, IDbSettingsReader settings, ILogger<HomeController> logger) : base(config, settings, logger)
        {
        }
    }
}
