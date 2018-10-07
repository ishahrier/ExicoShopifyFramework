using System;
using System.Collections.Generic;
using System.Text;

namespace Exico.Shopify.Data.Domain.ShopifyApiModels
{

    public class ShopifyWebhookObject
    {

        public long? Id { get; set; }
        /// <summary>
        /// The URL where the webhook should send the POST request when the event occurs.
        /// </summary>            
        public string Address { get; set; }

        /// <summary>
        /// The date and time when the webhook was created.
        /// </summary>            
        public DateTimeOffset? CreatedAt { get; set; }

        /// <summary>
        /// An optional array of fields which should be included in webhooks.
        /// </summary>            
        public IEnumerable<string> Fields { get; set; }

        /// <summary>
        /// The format in which the webhook should send the data. Valid values are json and xml.
        /// </summary>            
        public string Format { get; set; }

        /// <summary>
        /// An optional array of namespaces for metafields that should be included in webhooks.
        /// </summary>            
        public IEnumerable<string> MetafieldNamespaces { get; set; }

        /// <summary>
        /// The event that will trigger the webhook, e.g. 'orders/create' or 'app/uninstalled'. A full list of webhook topics can be found at https://help.shopify.com/api/reference/webhook.
        /// </summary>            
        public string Topic { get; set; }

        /// <summary>
        /// The date and time when the webhook was updated.
        /// </summary>            
        public DateTimeOffset? UpdatedAt { get; set; }
    }

}
