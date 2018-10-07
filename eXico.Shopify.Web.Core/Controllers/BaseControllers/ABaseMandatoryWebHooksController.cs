using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Domain.ShopifyApiModels;
using Exico.Shopify.Data.Framework;
using Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Controllers.BaseControllers
{
    //TODO check webhook authentication
    //TODO logging
    public abstract class ABaseMandatoryWebHooksController : ABaseController,IShopifyMandatoryHooks
    {
        protected readonly IShopifyEventsEmailer Emailer;
        protected readonly IShopifyApi ShopifyAPI;
        protected readonly IDbService<AspNetUser> UsrDbService;
        protected readonly IUserCaching UserCache;

        public ABaseMandatoryWebHooksController(IShopifyEventsEmailer emailer, IShopifyApi shopify, IDbService<AspNetUser> usrDbService, IConfiguration config, IDbSettingsReader settings, ILogger logger) : base(config, settings, logger)
        {
            Emailer = emailer;
            ShopifyAPI = shopify;
            UsrDbService = usrDbService;
        }

        public async Task<IActionResult> CustomersRedact(ShopifyCustomerRedactPayload payload)
        {
            try
            {
                await this.CustomerRequestedDataDeletion(payload);
            }
            catch (Exception ex)
            {
                this.LogGenericError(ex);
                
            }
            return Ok();
        }

        public async Task<IActionResult> ShopRedact([FromBody]ShopifyShopRedactPayload payload)
        {
            try
            {
                await this.ShopifyRequestedDataDeletion(payload);
            }
            catch (Exception ex)
            {
                this.LogGenericError(ex);
            }
            return Ok();
        }

        public async Task<IActionResult> CustomersDataRequest([FromBody]ShopifyCustomerDataRequestPayload payload)
        {
            try
            {
                await this.CustomerRequestedData(payload);
            }
            catch (Exception ex)
            {
                this.LogGenericError(ex);

            }
            return Ok();
        }

        protected override string GetPageTitle()
        {
            return "Mandatory web hook";
        }

        [NonAction]
        public virtual async Task CustomerRequestedDataDeletion(ShopifyCustomerRedactPayload payload) { }
        [NonAction]
        public virtual async Task ShopifyRequestedDataDeletion(ShopifyShopRedactPayload payload) { }
        [NonAction]
        public virtual async Task CustomerRequestedData(ShopifyCustomerDataRequestPayload payload) { }
    }
}