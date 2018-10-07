#if DEBUG
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Exico.Shopify.Web.Core.Controllers.BaseControllers;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Exico.Shopify.Web.Core.Controllers.DefaultControllers
{
    public class UninstallController : ABaseAppUninstallController
    {
        public UninstallController(IUserCaching userCache, IShopifyEventsEmailer emailer, IShopifyApi shopify, IDbService<AspNetUser> usrDbService, IConfiguration config, IDbSettingsReader settings, ILogger<UninstallController> logger) : base(userCache, emailer, shopify, usrDbService, config, settings, logger)
        {
        }
    }
}
#endif