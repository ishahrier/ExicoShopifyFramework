 
using Exico.Shopify.Web.Core.Controllers.BaseControllers;
using Exico.Shopify.Web.Core.Modules;
using Exico.Shopify.Web.Core.Plugins.WebMsg;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Demo.Controllers
{
    public class MyProfileController : ABaseMyProfileController
    {
        public MyProfileController(IWebMessenger webMsg, IShopifyApi shopifyApi, IPlansReader plansReader, IUserCaching cachedUser, IConfiguration config, IDbSettingsReader settings, ILogger<MyProfileController> logger) : base(webMsg, shopifyApi, plansReader, cachedUser, config, settings, logger)
        {
        }
    }
}
 