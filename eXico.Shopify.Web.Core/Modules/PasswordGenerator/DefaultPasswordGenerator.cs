using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Web.Core.Helpers;
using System;

namespace Exico.Shopify.Web.Core.Modules
{
    /// <summary>
    /// This is the default implementation of <see cref="IGenerateUserPassword"/>
    /// </summary>
    public class DefaultPasswordGenerator : IGenerateUserPassword
    {
        private readonly string PasswordSalt = string.Empty;
        protected DefaultPasswordGenerator() { }
        public DefaultPasswordGenerator(IDbSettingsReader settings)
        {
            this.PasswordSalt = settings.GetPasswordSalt();
        }
        /// <summary>
        /// Internal strategy for generating a (user) password 
        /// <remarks>Uses users ShopDoman name , adds a sault and then hashes the entire string.
        /// <para>The salt comes from the config. Config item name is UserPassSault.</para>
        /// </remarks>
        /// </summary>
        /// <param name="info"></param>
        /// <returns>The password (for a user) </returns>
        protected string PasswordGenrator(PasswordGeneratorInfo info)
        {
            if (string.IsNullOrEmpty(info.MyShopifyDomain) || string.IsNullOrWhiteSpace(info.MyShopifyDomain))
            {
                throw new Exception("My shopify domain name is not valid");
            }
            return (info.MyShopifyDomain + PasswordSalt).ToMd5();
        }
        public string GetPassword(PasswordGeneratorInfo info)
        {
            return PasswordGenrator(info);
        }


    }
}
