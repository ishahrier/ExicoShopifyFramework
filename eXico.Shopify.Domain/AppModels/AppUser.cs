using Exico.Shopify.Data.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Exico.Shopify.Data.Domain.AppModels
{
    public class AppUser
    {
        public AppUser(AspNetUser data,bool isAdmin=false)
        {
            this.Id = data.Id;
            this.Email = data.Email;
            this.ShopifyAccessToken = data.ShopifyAccessToken;
            this.MyShopifyDomain = data.MyShopifyDomain;
            this.ShopifyChargeId = data.ShopifyChargeId;
            this.BillingOn = data.BillingOn;
            this.Discount = data.Discount;
            this.IsAdmin = isAdmin;
            this.PlanId = data.PlanId;
        }
               

        public string Id { get; protected set; }
        public string Email { get; protected set; }

        public string ShopifyAccessToken { get; protected set; }
        public string MyShopifyDomain { get; protected set; }
        public long? ShopifyChargeId { get; protected  set; }

        public DateTime? BillingOn { get; protected set; }
        public int? PlanId { get; protected  set; }
        public double? Discount { get; protected  set; }
        public bool IsAdmin { get; protected  set; }

        public string UserName => MyShopifyDomain;
        public bool ShopIsConnected => string.IsNullOrEmpty(ShopifyAccessToken) == false;
        public bool BillingIsConnected => ShopifyChargeId.HasValue;
        public int GetPlanId() => PlanId.HasValue ? PlanId.Value : 0;

    }
}
