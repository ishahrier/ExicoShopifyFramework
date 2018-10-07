namespace Exico.Shopify.Data.Domain.ShopifyApiModels
{
    public class RedactCustomer
    {
        public string id { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
    }

    public abstract class ShopifyBaseCustomerPayload
    {
        public string shop_id { get; set; }
        public string shop_domain { get; set; }
        public RedactCustomer customer { get; set; }
    }

    public class ShopifyCustomerRedactPayload : ShopifyBaseCustomerPayload
    {
        public System.Collections.Generic.List<string> orders_to_redact { get; set; }
    }

}
