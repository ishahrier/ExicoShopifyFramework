using System;
using System.Collections.Generic;
using System.Text;

namespace Exico.Shopify.Data.Domain.ShopifyApiModels
{
    public class ShopifyShopObject
    {
        public long? Id { get; set; }
        public string Address1 { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public string CustomerEmail { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public string Domain { get; set; }
        public string Email { get; set; }
        public bool? ForceSSL { get; set; }
        public string GoogleAppsDomain { get; set; }
        public string GoogleAppsLoginEnabled { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string MoneyFormat { get; set; }
        public string MoneyWithCurrencyFormat { get; set; }
        public bool? MultiLocationEnabled { get; set; }
        public string MyShopifyDomain { get; set; }
        public string Name { get; set; }
        public string PlanName { get; set; }
        public string PlanDisplayName { get; set; }
        public bool? PasswordEnabled { get; set; }
        public string Phone { get; set; }
        public string PrimaryLocale { get; set; }
        public string Province { get; set; }
        public string ProvinceCode { get; set; }
        public string ShipsToCountries { get; set; }
        public string ShopOwner { get; set; }
        public string Source { get; set; }
        public bool? TaxShipping { get; set; }
        public bool? TaxesIncluded { get; set; }
        public bool? CountyTaxes { get; set; }
        public string Timezone { get; set; }
        public string IANATimezone { get; set; }
        public string Zip { get; set; }
        public bool? HasStorefront { get; set; }
        public bool? SetupRequired { get; set; }
        public string WeightUnit { get; set; }
    }
}
