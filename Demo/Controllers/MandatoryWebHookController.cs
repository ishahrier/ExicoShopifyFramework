 
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Exico.Shopify.Web.Core.Controllers.BaseControllers;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Demo.Controllers
{
    public class MandatoryWebHookController : ABaseMandatoryWebHooksController
    {
        public MandatoryWebHookController(IShopifyEventsEmailer emailer, IShopifyApi shopify, IDbService<AspNetUser> usrDbService, IConfiguration config, IDbSettingsReader settings, ILogger logger) : base(emailer, shopify, usrDbService, config, settings, logger)
        {
        }
    }
}
 