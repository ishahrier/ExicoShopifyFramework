using Exico.Shopify.Data;
using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Helpers
{
    
    public class UserDbServiceHelper
    {
        
        /// <summary>
        /// Gets the user by user id .
        /// </summary>        
        /// <param name="userDbService">The user service.</param>
        /// <param name="userId">The user id , the GUID.</param>
        /// <returns><see cref="Exico.Shopify.Data.Domain.AppModels.AppUser"/></returns>
        public static async Task<AppUser> GetAppUserByIdAsync(IDbService<AspNetUser> userDbService, string userId)
        {
            var dbUser = userDbService.FindSingleWhere(x => x.Id == userId);
            return await _CheckIfAdmin(dbUser, userDbService);
        }

        /// <summary>
        /// Saves the shop and access code.
        /// </summary>        
        /// <param name="userDbService">The user service.</param>
        /// <param name="userId">The user identifier (GUID).</param>
        /// <param name="accessToken">The access token from shopify</param>
        /// <param name="myShopifyDomain">The my shopify domain, i.e. mystore.myshopify.com  </param>
        /// <returns><c>true</c> on success otherwise <c>false</c></returns>
        public static bool SaveShopAndAccessCode(IDbService<AspNetUser> userDbService, string userId, string accessToken, string myShopifyDomain)
        {
            var user = userDbService.FindSingleWhere(x => x.Id == userId);
            if (user == null) return false;
            else
            {
                user.ShopifyAccessToken = accessToken;
                user.MyShopifyDomain = myShopifyDomain;
                var updated = userDbService.Update(user, user.Id);
                return updated != null && updated.Id == userId;
            }
        }

        /// <summary>
        /// Sets the users charge information that has been succesfully authorized by shopify api
        /// </summary>       
        /// <param name="userDbService">The user service.</param>
        /// <param name="userId">The user identifier (GUID)</param>
        /// <param name="shopifyChargeId">The shopify approved charge identifier returned from shopify API</param>
        /// <param name="planId">The plan id associated for the approved charge</param>
        /// <param name="billingOn">The billing on date , the date when the plan charge is approved</param>
        /// <returns><c>true</c> on success otherwise <c>false</c></returns>
        public static bool SetUsersChargeInfo(IDbService<AspNetUser> userDbService, string userId, long shopifyChargeId, int? planId, DateTimeOffset? billingOn)
        {

            var user = userDbService.FindSingleWhere(x => x.Id == userId);
            if (user == null) return false;
            else
            {
                user.ShopifyChargeId = shopifyChargeId;
                user.BillingOn = billingOn.Value.DateTime;
                user.PlanId = planId;
                var updated = userDbService.Update(user, user.Id);
                return updated != null && updated.Id == userId;
            }
        }

        /// <summary>
        /// Removes user charge information. Not the user just the charge related portion.
        /// BillingOn , ShopifyChargeId  and PlanId (for the charge) will be set to null        
        /// </summary>        
        /// <param name="userDbService">The user service.</param>
        /// <param name="userId">The user identifier.</param>
        /// /// <remarks>An example usage senario is , something went wrong on previous installation (i.e. cannot upgrade plan)
        /// so you want to wipe off all charge related information and give the user another
        /// chance to try to re-install and approve the charge and new plan while retaining all important data of previous installation.</remarks>
        /// <returns><c>true</c> on success otherwise <c>false</c></returns>
        public static bool UnSetUserChargeInfo(IDbService<AspNetUser> userDbService, string userId)
        {
            var user = userDbService.FindSingleWhere(x => x.Id == userId);
            if (user == null) return false;
            else
            {
                user.ShopifyChargeId = null;
                user.BillingOn = null;
                user.PlanId = null;
                var updated = userDbService.Update(user, user.Id);
                return updated != null && updated.Id == userId;
            }
        }

        /// <summary>
        /// Gets the name of the user by shop.
        /// </summary>        
        /// <param name="userDbService">The user service.</param>
        /// <param name="myShopifyDomain">The my shopify domain, i.e. mystore.myshopify.com (this is also the username in the AspNetUsers table) </param>
        /// <returns><see  cref="Exico.Shopify.Data.Domain.DBModels.AspNetUser"/></returns>
        public static async  Task<AppUser> GetUserByShopDomain(IDbService<AspNetUser> userDbService, string myShopifyDomain)
        {
            var dbUser =  userDbService.FindSingleWhere(x => x.UserName == myShopifyDomain && x.MyShopifyDomain == myShopifyDomain);
            return await _CheckIfAdmin(dbUser, userDbService);

        }

        /// <summary>
        /// Checks if the given shopify shop domain exists in the DB and the accesstoken is not null
        /// </summary>
        /// <param name="userDbService">The user database service.</param>
        /// <param name="myShopifyDomain">Name of the shop.</param>
        /// <returns></returns>
        public static bool ShopIsAuthorized(IDbService<AspNetUser> userDbService, string myShopifyDomain)
        {
            var shop = userDbService.FindSingleWhere(x => x.MyShopifyDomain == myShopifyDomain && x.UserName == myShopifyDomain);
            if (shop != null) return shop.ShopifyAccessToken != null;
            else return false;
        }

        /// <summary>
        /// Removes the user.
        /// </summary>
        /// <param name="userDbService">The user database service.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns><c>true</c> if removed, <c>false</c> otherwise.</returns>
        public static bool RemoveUser(IDbService<AspNetUser> userDbService,string userId)
        {
            return userDbService.Delete(userId);
        }

        private static async Task<AppUser > _CheckIfAdmin(AspNetUser dbUser,IDbService<AspNetUser> userDbService)
        {
            if (dbUser != null)
            {
                Dictionary<string, object> paramDic = new Dictionary<string, object>();
                paramDic.Add("userId", dbUser.Id);
                paramDic.Add("roleName", UserInContextHelper.ADMIN_ROLE);
                var command = userDbService.GetRepo().CreateDbCommand(CommandType.Text, @"SELECT count(*) from AspNetRoles r inner join
                AspNetUserRoles u on  u.RoleId = r.Id  where r.Name = @roleName and u.UserId = @userId", paramDic);
                command.Connection.Open();
                var result = (int)await command.ExecuteScalarAsync();
                command.Connection.Close();
                return new AppUser(dbUser, result > 0);
            }
            else return null;
        }
    }
}
