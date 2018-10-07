using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Data.Domain.ShopifyApiModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Exico.Shopify.Data.Domain.ViewModels
{

    public class MyProfileViewModel
    {
        public AppUser Me { get; set; }
        public PlanAppModel MyPlan { get; set; }
        public ShopifyRecurringChargeObject ChargeData { get; set; }

    }
}
