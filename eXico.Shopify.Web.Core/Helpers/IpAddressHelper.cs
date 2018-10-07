using Exico.Shopify.Web.Core.Modules;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Helpers
{
    /// <summary>
    /// IP Address Helper
    /// </summary>
    public class IPAddressHelper
    {
        
        /// <summary>
        /// Determines whether given ip address is an admin IP
        /// </summary>
        /// <param name="ipAddress">The ip address you want to check.</param>
        /// <param name="settings">The settings reader.</param>
        /// <returns>
        ///   <c>true</c> if given IP is an admin IP; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInPrivilegedIpList(string ipAddress, IDbSettingsReader settings)
        {
            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                var addresses = settings.GetAdminIps();
                return addresses.Where(a => a.Trim().Equals(ipAddress, StringComparison.InvariantCultureIgnoreCase)).Any();
            }
            return false;
        }

        /// <summary>
        /// Determines whether current user's (user in context, the logged in user) ip address is an admin IP
        /// </summary>
        /// <param name="context"> The http context. <seealso  cref="HttpContext"/></param>
        /// <param name="settings">The settings.</param>
        /// <returns>
        ///   <c>true</c> if current user's IP is an admin IP; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCurrentUserIpPrivileged(HttpContext context, IDbSettingsReader settings) 
            => IsInPrivilegedIpList(GetCurrentUserIP(context), settings);

        /// <summary>
        /// Gets current user's (user in context, the logged in user) IP address.
        /// </summary>
        /// <param name="context">The http context. <seealso cref="HttpContext"/></param>
        /// <returns>
        /// IP Address
        /// </returns>
        public static string GetCurrentUserIP(HttpContext context)
            => context.Connection.RemoteIpAddress.ToString();
    }

}
