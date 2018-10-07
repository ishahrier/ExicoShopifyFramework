using Exico.Shopify.Data;
using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Helpers
{
    /// <summary>
    /// Contains user related helper methods for current <c>HttpContext</c>.
    /// This is just a wrapper on top of the <c>HttpContext.User</c>
    /// </summary>
    public class UserInContextHelper
    {
        public const string ADMIN_ROLE = "admin"; //TDO come from config

        //public UserInContextHelper(HttpContext hca)
        //{
        //    this.HttpContext = hca;
        //}

        /// <summary>
        /// Determines whether is current user is admin by checking identity role.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if current user is admin otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCurrentUserIdAdmin(HttpContext hc) => hc.User.IsInRole(UserInContextHelper.ADMIN_ROLE);


        /// <summary>
        /// Determines whether authenticated/logged in using username and password.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if logged in; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsLoggedIn(HttpContext hc) => hc.User.Identity.IsAuthenticated;

        /// <summary>
        /// Gets the current user id (GUID number).
        /// </summary>
        /// <returns><c>GUID</c> (string value) provided for the user assigned by identity core framework</returns>
        public static string GetCurrentUserId(HttpContext hc)
        {
            if (hc.User.Identity.IsAuthenticated)
            {
                var userId = hc.User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
                return userId;
            }
            else
            {
                return null;
            }
        }

    }
}
