using System;
using System.Collections.Generic;
using System.Text;

namespace Exico.Shopify.Data.Domain.ShopifyApiModels
{
    public class ShopifyRecurringChargeObject
    {
        public long? Id { get; set; }
        public DateTimeOffset? ActivatedOn { get; set; }
        public DateTimeOffset? BillingOn { get; set; }
        public decimal? CappedAmount { get; set; }
        public DateTimeOffset? CancelledOn { get; set; }
        public string ConfirmationUrl { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public string ReturnUrl { get; set; }
        public string Status { get; set; }
        public string Terms { get; set; }
        public bool? Test { get; set; }
        public int? TrialDays { get; set; }
        public DateTimeOffset? TrialEndsOn { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
