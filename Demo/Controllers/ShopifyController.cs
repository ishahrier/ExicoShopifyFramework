 
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Exico.Shopify.Web.Core.Controllers.BaseControllers;
using Exico.Shopify.Web.Core.Helpers;
using Exico.Shopify.Web.Core.Modules;
using Exico.Shopify.Web.Core.Plugins.WebMsg;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Demo.Controllers
{
    public class ShopifyController : ABaseShopifyController
    {
        public ShopifyController(IAppSettingsAccessor appSettings, IDbService<AspNetUser> userService, IPlansReader planReader, SignInManager<AspNetUser> signInManager, UserManager<AspNetUser> userManager, IGenerateUserPassword passwordGenerator, IUserCaching userCache, IShopifyEventsEmailer emailer, IWebMessenger webMsg, IShopifyApi shopifyApi, IConfiguration config, IDbSettingsReader settings, ILogger<ShopifyController> logger)
            : base(appSettings,userService, planReader, signInManager, userManager, passwordGenerator, userCache, emailer, webMsg, shopifyApi, config, settings, logger)
        {
        }
    }
}
 