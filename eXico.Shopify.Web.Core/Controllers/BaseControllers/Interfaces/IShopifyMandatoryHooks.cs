using Exico.Shopify.Data.Domain.ShopifyApiModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
//https://help.shopify.com/en/api/guides/gdpr-resources#mandatory-webhooks
namespace Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces
{
    public interface IShopifyMandatoryHooks
    {
        [NonAction]
        Task CustomerRequestedDataDeletion(ShopifyCustomerRedactPayload payload);
        [NonAction]
        Task ShopifyRequestedDataDeletion(ShopifyShopRedactPayload payload);
        [NonAction]
        Task CustomerRequestedData(ShopifyCustomerDataRequestPayload payload);
    }
}
