using Exico.Shopify.Web.Core.Modules;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Filters
{
    /// <summary>
    /// Compares user's IP with allowed admin IPS in the settings table in the database.
    /// The name of the setting is <see cref="CORE_SYSTEM_SETTING_NAMES.PRIVILEGED_IPS"/>.
    /// NOTE: if the request is a local request, in that case its allowed 
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Filters.ActionFilterAttribute" />
    public class IPAddressVerification : Attribute, IAuthorizationFilter,IOrderedFilter
    {
        public const int DEFAULT_ORDER = 1;
        private readonly IDbSettingsReader _Settings;
        private readonly ILogger<IPAddressVerification> _Logger;

        public IPAddressVerification(IDbSettingsReader settings, ILogger<IPAddressVerification> logger)
        {
            this._Settings = settings;
            this._Logger = logger;
        }

        public int Order { get; set; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {         
            _Logger.LogInformation("Starting admin ip verification.");
            var connection = context.HttpContext.Connection;
            bool isLocal = IslolcalRequest(context);

            if (isLocal == false)
            {
                _Logger.LogInformation("Remote IP detected.Comparing the IP with admin IPs in the settings.");
                bool isAllowed = _Settings.GetAdminIps().Contains(connection.RemoteIpAddress.ToString());
                if (isAllowed == false)
                {
                    _Logger.LogWarning($"Remote IP '{connection.RemoteIpAddress.ToString()}' is not found in the privileged IP list.Access Denied.");
                    context.Result = new UnauthorizedResult() { };
                }
                else                
                    _Logger.LogInformation($"Remote IP '{connection.RemoteIpAddress.ToString()}' is found in the privileged IP list.");                
            }
            else
                _Logger.LogInformation("Local request detected.Skipping IP checking.");                
        }
        
        private bool IslolcalRequest(AuthorizationFilterContext context)
        {
            if (context.HttpContext.Connection.RemoteIpAddress.Equals(context.HttpContext.Connection.LocalIpAddress))
            {
                return true;
            }
            if (IPAddress.IsLoopback(context.HttpContext.Connection.RemoteIpAddress))
            {
                return true;
            }
            return false;
        }
    }
}
