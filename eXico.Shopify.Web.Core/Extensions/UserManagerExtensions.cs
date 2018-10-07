using Exico.Shopify.Data.Domain.DBModels;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Extensions
{
    /// <summary>
    /// Some useful extensions for <c> Microsoft.AspNetCore.Identity.UserManager</c> to modify,create app users <see cref="Exico.Shopify.Data.Domain.DBModels.AspNetUser"/>
    /// </summary>
    public static class UserManagerExtensions
    {
        public const string ADMIN_ROLE = "admin"; //TODO should come from config

        /// <summary>
        /// Creates the application user  <see cref="Exico.Shopify.Data.Domain.DBModels.AspNetUser"/>
        /// </summary>
        /// <param name="userManager"> Microsoft.AspNetCore.Identity.UserManager</param>
        /// <param name="shop">The shop name (my shopify domain i.e. store.myshopify.com)</param>
        /// <param name="email">The email of the shopify store</param>
        /// <param name="password">The password for the user. Must be generated using default or custom implementation of <see cref="Helpers.IGenerateUserPassword"/>  </param>
        /// 
        /// <returns><c>IdentityResult</c></returns>
        public static async Task<IdentityResult> CreateAppUser(this UserManager<AspNetUser> userManager, string shop, string email, string accessToken, string password)
        {
            var user = new AspNetUser { UserName = shop, Email = email, MyShopifyDomain = shop, ShopifyAccessToken = accessToken };
            var result = await userManager.CreateAsync(user, password);
            return result;
        }

        /// <summary>
        /// Makes the user ( <see cref="Exico.Shopify.Data.Domain.DBModels.AspNetUser"/>) admin by adding the user in admin role
        /// </summary>
        /// <param name="userManager">Microsoft.AspNetCore.Identity.UserManager</param>
        /// <param name="user">The user</param>
        /// <returns><c>IdentityResult</c></returns>
        public static async Task<IdentityResult> MakeUserAdmin(this UserManager<AspNetUser> userManager, AspNetUser user)
        {
            var result = await userManager.AddToRoleAsync(user, ADMIN_ROLE);
            return result;
        }

        /// <summary>
        /// Gets the identity user .
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="userClaim">The user claim.</param>
        /// <returns>if ClaimsPrincipal (logged in user) is found in the storage then AspNetUser object is returned, null otherwise</returns>
        public static async Task<AspNetUser> GetClaimedUserData(this UserManager<AspNetUser> userManager, ClaimsPrincipal userClaim)
        {
            AspNetUser user = await userManager.GetUserAsync(userClaim);
            return user;
        }
    }
}
