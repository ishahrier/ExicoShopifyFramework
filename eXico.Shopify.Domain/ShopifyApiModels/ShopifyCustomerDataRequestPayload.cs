using System;
using System.Collections.Generic;
using System.Text;

namespace Exico.Shopify.Data.Domain.ShopifyApiModels
{  
    public class ShopifyCustomerDataRequestPayload: ShopifyBaseCustomerPayload
    {
        public System.Collections.Generic.List<string> orders_requested { get; set; }
    }

}
