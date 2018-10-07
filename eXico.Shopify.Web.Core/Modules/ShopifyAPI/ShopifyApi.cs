using Exico.Shopify.Data.Domain.ShopifyApiModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ShopifySharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules
{
    /// <summary>
    /// This class implements <c>IShopifyApi</c> interface.
    /// It uses ShopifyLibrary to communicate with shopify official API service.
    /// The framework is wrapping up the ShopifySharp library instead of using it directly so that 
    /// any API library can be used as long as it is registered for <c>IShopifyApi</c> interface.
    /// </summary>
    /// <seealso cref="Exico.Shopify.Web.Core.Modules.IShopifyApi" />
    public class ShopifyApi : IShopifyApi
    {

        private readonly IDbSettingsReader _Settings;
        private readonly ILogger<ShopifyApi> _Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShopifyApi"/> class.
        /// </summary>
        /// <param name="settings">The settings service.</param>
        /// <param name="logger">The logger service.</param>
        public ShopifyApi(IDbSettingsReader settings, ILogger<ShopifyApi> logger)
        {
            _Settings = settings;
            _Logger = logger;
        }

        /// <summary>
        /// Activates the recurring charge asynchronously.
        /// </summary>
        /// <param name="myShopifyDomain">My shopify URL.</param>
        /// <param name="shopifyAccessToken">The shopify access token.</param>
        /// <param name="chargeId">The charge identifier.</param>
        /// <returns></returns>
        public async Task ActivateRecurringChargeAsync(string myShopifyDomain, string shopifyAccessToken, long chargeId)
        {
            _CheckmyShopifyDomain(myShopifyDomain);
            _CheckShopAccessToken(shopifyAccessToken);
            _Logger.LogInformation($"Activating recurring charge id '{chargeId}'");
            RecurringChargeService service = new RecurringChargeService(myShopifyDomain, shopifyAccessToken);
            await service.ActivateAsync(chargeId);
            _Logger.LogInformation($"Done activating recurring charge id '{chargeId}'");
        }

        /// <summary>
        /// Authorizes the shopify store with specified authentication code.
        /// In return it gets the shopify access token for the store (myShopifyDomain)
        /// </summary>
        /// <param name="authCode">The authentication code supplied by shopify</param>
        /// <param name="myShopifyDomain">My shopify URL.</param>
        /// <returns></returns>
        public async Task<string> Authorize(string myShopifyDomain, string authCode)
        {
            _CheckmyShopifyDomain(myShopifyDomain);
            if (string.IsNullOrEmpty(authCode))
            {
                _Logger.LogError($"Auth code cannot be null or empty for shop '{myShopifyDomain}'.");
                throw new Exception("Auth code cannot be null or empty.");
            }
            _Logger.LogInformation($"Authorizing shop '{myShopifyDomain}' with auth code '{authCode}' for access code.");
            var secretKey = GetSecretKey();
            var apiKey = GetApiKey();
            var accessCode = await AuthorizationService.Authorize(authCode, myShopifyDomain, apiKey, secretKey);
            _Logger.LogInformation($"Authorization done. Returning access code '{accessCode}'.");
            return accessCode;
        }

        /// <summary>
        /// Gets the authorization URL from shopify during installation process.
        /// This URL is used to redirect to an action method that can receive 
        /// and handle the authcode.
        /// <see cref="Authorize(string, string)"/>
        /// </summary>
        /// <param name="myShopifyDomain">My shopify URL.</param>
        /// <param name="scopes">The scopes; list of permissions.</param>
        /// <param name="redirectUrl">The redirect URL to your app (https://yourAppSite/controller/action) from shopify.</param>
        /// <param name="state">The state, optional</param>
        /// <param name="grants">The grants, optional</param>
        /// <returns></returns>
        /// <exception cref="Exception">Null list of scopes detected</exception>
        public Uri GetAuthorizationUrl(string myShopifyDomain, IEnumerable<string> scopes, string redirectUrl, string state = null, IEnumerable<string> grants = null)
        {
            _CheckmyShopifyDomain(myShopifyDomain);
            if (scopes == null)
            {
                _Logger.LogError($"Null list of scopes detected for shop '{myShopifyDomain}'");
                throw new Exception("Null list of scopes detected");
            }
            else
            {
                _Logger.LogInformation($"Building auth redirection url for '{myShopifyDomain}' with scopes [{string.Join(",", scopes.ToArray())}]");
                var authUrl = AuthorizationService.BuildAuthorizationUrl(
                    scopes,
                    myShopifyDomain,
                    GetApiKey(),
                    redirectUrl,
                    state,
                    grants);
                _Logger.LogInformation($"Return auth url '{authUrl}' for shop '{myShopifyDomain}'");
                return authUrl;

            }
        }

        /// <summary>
        /// Creates the recurring charge asynchronously.
        /// </summary>
        /// <param name="myShopifyDomain">My shopify URL.</param>
        /// <param name="shopifyAccessToken">The shopify access token.</param>
        /// <param name="charge">Valid charge object.</param>
        /// <returns></returns>
        public async Task<ShopifyRecurringChargeObject> CreateRecurringChargeAsync(string myShopifyDomain, string shopifyAccessToken, ShopifyRecurringChargeObject charge)
        {
            _CheckmyShopifyDomain(myShopifyDomain);
            _CheckShopAccessToken(shopifyAccessToken);
            _Logger.LogInformation($"Creating recurring charge '{charge.Name} - {charge.Price} - {(charge.Test.Value ? "is_test" : "not_test")} - {charge.TrialDays} days - {charge.ReturnUrl}'.");
            RecurringChargeService service = new RecurringChargeService(myShopifyDomain, shopifyAccessToken);
            var ret = await service.CreateAsync(new RecurringCharge()
            {
                Name = charge.Name,
                Price = charge.Price,
                Terms = charge.Terms,
                ReturnUrl = charge.ReturnUrl,
                TrialDays = charge.TrialDays,
                Test = charge.Test
            });

            _Logger.LogInformation($"Done creating '{ret.Name}' with id = {ret.Id}.");

            return new ShopifyRecurringChargeObject()
            {
                CancelledOn = ret.CancelledOn,
                BillingOn = ret.BillingOn,
                ActivatedOn = ret.ActivatedOn,
                CappedAmount = ret.CappedAmount,
                ConfirmationUrl = ret.ConfirmationUrl,
                CreatedAt = ret.CreatedAt,
                Id = ret.Id,
                Name = ret.Name,
                Price = ret.Price,
                ReturnUrl = ret.ReturnUrl,
                Status = ret.Status,
                Terms = ret.Terms,
                Test = ret.Test,
                TrialDays = ret.TrialDays,
                TrialEndsOn = ret.TrialEndsOn,
                UpdatedAt = ret.UpdatedAt
            };
        }

        /// <summary>
        /// Creates the webhook asynchronously.
        /// </summary>
        /// <param name="myShopifyDomain">The myshopify URL.</param>
        /// <param name="shopifyAccessToken">The shopify access token.</param>
        /// <param name="webhook">Valid webhook object.</param>
        /// <returns></returns>
        public async Task<ShopifyWebhookObject> CreateWebhookAsync(string myShopifyDomain, string shopifyAccessToken, ShopifyWebhookObject webhook)
        {
            _CheckmyShopifyDomain(myShopifyDomain);
            _CheckShopAccessToken(shopifyAccessToken);
            if (webhook == null)
            {
                _Logger.LogError($"Web hook object cannot be null for shop {myShopifyDomain}");
                throw new Exception("Web hook object cannot be null");
            }
            _Logger.LogInformation($"Sending request to create webhook for '{myShopifyDomain}' on topic '{webhook.Topic}'");
            WebhookService service = new WebhookService(myShopifyDomain, shopifyAccessToken);
            var data = await service.CreateAsync(new Webhook()
            {
                Address = webhook.Address,
                CreatedAt = webhook.CreatedAt,
                Fields = webhook.Fields,
                Format = webhook.Format,
                MetafieldNamespaces = webhook.MetafieldNamespaces,
                Topic = webhook.Topic,
                UpdatedAt = webhook.UpdatedAt
            });
            if (data == null)
            {
                _Logger.LogError("Failed creating webhook. Server response was NULL.");
                throw new Exception("Failed creating webhook.Server responded NULL.");
            }
            else
            {
                var ret = new ShopifyWebhookObject()
                {
                    Address = data.Address,
                    CreatedAt = data.CreatedAt,
                    Fields = data.Fields,
                    Format = data.Format,
                    MetafieldNamespaces = data.MetafieldNamespaces,
                    Topic = data.Topic,
                    UpdatedAt = data.UpdatedAt,
                    Id = data.Id
                };
                _Logger.LogInformation($"Done creating webhook for '{myShopifyDomain}' on topic '{webhook.Topic}' where id = '{ret.Id}'");
                return ret;
            }
        }

        /// <summary>
        /// Gets the recurring charge asynchronous.
        /// </summary>
        /// <param name="myShopifyDomain">The myshopify URL.</param>
        /// <param name="shopifyAccessToken">The shopify access token.</param>
        /// <param name="chargeId">The charge identifier.</param>
        /// <param name="fields">The fields.</param>
        /// <returns></returns>
        public async Task<ShopifyRecurringChargeObject> GetRecurringChargeAsync(string myShopifyDomain, string shopifyAccessToken, long chargeId, string fields = null)
        {
            _CheckmyShopifyDomain(myShopifyDomain);
            _CheckShopAccessToken(shopifyAccessToken);
            RecurringChargeService service = new RecurringChargeService(myShopifyDomain, shopifyAccessToken);
            _Logger.LogInformation($"Retriving recurring charge with id ='{chargeId}'");
            var data = await service.GetAsync(chargeId, fields);
            var ret = new ShopifyRecurringChargeObject()
            {
                CancelledOn = data.CancelledOn,
                BillingOn = data.BillingOn,
                ActivatedOn = data.ActivatedOn,
                CappedAmount = data.CappedAmount,
                ConfirmationUrl = data.ConfirmationUrl,
                CreatedAt = data.CreatedAt,
                Id = data.Id,
                Name = data.Name,
                Price = data.Price,
                ReturnUrl = data.ReturnUrl,
                Status = data.Status,
                Terms = data.Terms,
                Test = data.Test,
                TrialDays = data.TrialDays,
                TrialEndsOn = data.TrialEndsOn,
                UpdatedAt = data.UpdatedAt
            };
            _Logger.LogInformation($"Found recurring charge '{ret.Id} - {ret.Name} - {ret.Price} - {(ret.Test.Value ? "is_test" : "not_test")} - {ret.TrialDays} days - {ret.ReturnUrl}'.");
            return ret;
        }

        /// <summary>
        /// Gets the shop meta information object asynchronously.
        /// </summary>
        /// <param name="myShopifyDomain">The myshopify URL.</param>
        /// <param name="shopifyAccessToken">The shopify access token.</param>
        /// <returns></returns>
        public async Task<ShopifyShopObject> GetShopAsync(string myShopifyDomain, string shopifyAccessToken)
        {
            try
            {
                _CheckmyShopifyDomain(myShopifyDomain);
                _CheckShopAccessToken(shopifyAccessToken);
                ShopService service = new ShopService(myShopifyDomain, shopifyAccessToken);
                var shop = await service.GetAsync();
                var ret = new ShopifyShopObject()
                {
                    Address1 = shop.Address1,
                    City = shop.City,
                    Country = shop.Country,
                    CountryCode = shop.CountryCode,
                    CountryName = shop.CountryName,
                    CountyTaxes = shop.CountyTaxes,
                    CreatedAt = shop.CreatedAt,
                    Currency = shop.Currency,
                    CustomerEmail = shop.CustomerEmail,
                    Description = shop.Description,
                    Domain = shop.Domain,
                    Email = shop.Email,
                    ForceSSL = shop.ForceSSL,
                    GoogleAppsDomain = shop.GoogleAppsDomain,
                    GoogleAppsLoginEnabled = shop.GoogleAppsLoginEnabled,
                    HasStorefront = shop.HasStorefront,
                    IANATimezone = shop.IANATimezone,
                    Id = shop.Id,
                    Latitude = shop.Latitude,
                    Longitude = shop.Longitude,
                    MoneyFormat = shop.MoneyFormat,
                    MoneyWithCurrencyFormat = shop.MoneyWithCurrencyFormat,
                    MultiLocationEnabled = shop.MultiLocationEnabled,
                    MyShopifyDomain = shop.MyShopifyDomain,
                    Name = shop.Name,
                    PasswordEnabled = shop.PasswordEnabled,
                    Phone = shop.Phone,
                    PlanDisplayName = shop.PlanDisplayName,
                    PlanName = shop.PlanName,
                    PrimaryLocale = shop.PrimaryLocale,
                    Province = shop.Province,
                    ProvinceCode = shop.ProvinceCode,
                    SetupRequired = shop.SetupRequired,
                    ShipsToCountries = shop.ShipsToCountries,
                    ShopOwner = shop.ShopOwner,
                    Source = shop.Source,
                    TaxesIncluded = shop.TaxesIncluded,
                    TaxShipping = shop.TaxShipping,
                    Timezone = shop.Timezone,
                    WeightUnit = shop.WeightUnit,
                    Zip = shop.Zip
                };

                return ret;
            }
            catch (Exception ex)
            {
                var ex2 = new Exception("Error occurred while getting shop information",ex);
                _Logger.LogError(ex2, ex2.Message);
                throw ex2;
            }

        }

        /// <summary>
        /// Determines whether the request to the app is authentic.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <returns>
        ///   <c>true</c> if request is authentic; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAuthenticRequest(HttpRequest request)
        {            
            try
            {
                return  AuthorizationService.IsAuthenticRequest(request.Query, GetSecretKey());
            }
            catch (Exception ex)
            {

                var ex2 = new  Exception("Error occurred during shopify request authentication check.", ex);
                _Logger.LogError(ex2, ex2.Message);
                throw ex2;
            }
           
        }

        /// <summary>
        /// Determines whether the webhook call is authentic or not.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <returns><c>true</c> if authentic, <c>false</c> otherwise.</returns>
        public async Task<bool> IsAuthenticWebhook(HttpRequest request)
        {
            bool ret = false;
            try
            {
                var memoryStream = new MemoryStream();
                await request.Body.CopyToAsync(memoryStream);
                ret = await AuthorizationService.IsAuthenticWebhook(request.Headers, memoryStream, GetSecretKey());
                return ret;
            }
            catch (Exception ex)
            {
                var ex2 = new Exception("Error occurred during shopify webhook authentication check.", ex);
                _Logger.LogError(ex2, ex2.Message);
                throw ex2;
            }

            
        }

        /// <summary>
        /// Gets the application secret key provided by shopify.
        /// </summary>
        /// <returns>secret key as <c>string</c></returns>        
        public string GetSecretKey()
        {
            return _Settings.GetShopifySecretKey();
        }

        /// <summary>
        /// Gets the API key provided by shopify for the app.
        /// </summary>
        /// <returns>secret key as <c>string</c></returns>     
        public string GetApiKey()
        {
            return _Settings.GetShopifyApiKey();
        }

        private void _CheckmyShopifyDomain(string myShopifyDomain)
        {
            if (string.IsNullOrEmpty(myShopifyDomain) || string.IsNullOrWhiteSpace(myShopifyDomain))
            {                
                throw new Exception("My shopify url cannot be empty or null.");
            }
            else if (!myShopifyDomain.EndsWith(".myshopify.com"))
            {                
                throw new Exception($"Not a valid my shopify URL '{myShopifyDomain}'");
            }

        }
        private void _CheckShopAccessToken(string shopifyAccessToken)
        {
            if (string.IsNullOrEmpty(shopifyAccessToken) || string.IsNullOrWhiteSpace(shopifyAccessToken))
            {                
                throw new Exception("Shopify access token cannot empty or null");
            }
        }

    }
}
