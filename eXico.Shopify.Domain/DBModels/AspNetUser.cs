using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace Exico.Shopify.Data.Domain.DBModels
{
    
    public class AspNetUser:IdentityUser
    {
        public AspNetUser() : base()
        {        
        }
        public string ShopifyAccessToken { get; set; }
        public string MyShopifyDomain { get; set; }
        public long? ShopifyChargeId { get; set; }
                
        public DateTime? BillingOn { get; set; }
        public int? PlanId { get; set; }        
        public double? Discount { get; set; }
        
    }
}
