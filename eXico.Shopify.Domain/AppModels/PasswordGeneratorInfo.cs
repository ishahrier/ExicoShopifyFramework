using Exico.Shopify.Data.Domain.DBModels; 

namespace Exico.Shopify.Data.Domain.AppModels
{
    public class PasswordGeneratorInfo
    {
        public PasswordGeneratorInfo(string myShopDomain, string email)
        {
            this.MyShopifyDomain = myShopDomain;
            this.ShopEmail = email;
        }
        public PasswordGeneratorInfo(AspNetUser user) : this(user.MyShopifyDomain, user.Email)
        {

        }

        public string MyShopifyDomain { get; set; }
        public string ShopEmail { get; set; }
    }
}
