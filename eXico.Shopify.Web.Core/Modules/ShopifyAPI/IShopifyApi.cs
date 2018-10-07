using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Exico.Shopify.Data.Domain.ShopifyApiModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Exico.Shopify.Web.Core.Modules
{
    /// <summary>
    /// The framework needs to call some shopify API. But it doesn't want to tie itself to a specific 
    /// shopify API library. Hence this interface. The default implementation is <see cref="ShopifyApi"/>
    /// and it uses ShopifySharp library. 
    /// </summary>
    public interface IShopifyApi
    {
        Task ActivateRecurringChargeAsync(string myShopifyDomain, string shopifyAccessToken, long id);
        Task<string> Authorize(string authCode, string myShopifyDomain);
        Task<ShopifyRecurringChargeObject> CreateRecurringChargeAsync(string myShopifyDomain, string shopifyAccessToken, ShopifyRecurringChargeObject charge);
        Task<ShopifyWebhookObject> CreateWebhookAsync(string myShopifyDomain, string shopifyAccessToken, ShopifyWebhookObject webhook);
        Uri GetAuthorizationUrl(string myShopifyDomain, IEnumerable<string> scopes, string redirectUrl, string state = null, IEnumerable<string> grants = null);
        Task<ShopifyRecurringChargeObject> GetRecurringChargeAsync(string myShopifyDomain, string shopifyAccessToken, long chargeId, string fields = null);
        Task<ShopifyShopObject> GetShopAsync(string myShopifyDomain, string shopifyAccessToken);
        bool IsAuthenticRequest(HttpRequest request);
        Task<bool> IsAuthenticWebhook(HttpRequest request);
        string GetSecretKey();
        string GetApiKey();
    }
}