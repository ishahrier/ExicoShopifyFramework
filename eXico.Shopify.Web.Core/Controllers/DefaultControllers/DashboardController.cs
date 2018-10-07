#if DEBUG
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Exico.Shopify.Web.Core.Controllers.BaseControllers;
using Exico.Shopify.Web.Core.Modules;
using Exico.Shopify.Web.Core.Plugins.WebMsg;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Exico.Shopify.Web.Core.Controllers.DefaultControllers
{
    public class DashboardController : ABaseAppDashBoardController
    {
        public DashboardController(IWebMessenger webMSg, IShopifyEventsEmailer emailer, IDbService<AspNetUser> usrDbService, IUserCaching userCache, IPlansReader planReader, IConfiguration config, IDbSettingsReader settings, ILogger<DashboardController> logger) : base(webMSg, emailer, usrDbService, userCache, planReader, config, settings, logger)
        {
        }
    }
}
#endif