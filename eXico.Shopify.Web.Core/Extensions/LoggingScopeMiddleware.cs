using Exico.Shopify.Web.Core.Helpers;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Extensions
{
    /// <summary>
    /// Pushes a user and store related data context property as logging scope that can be used 
    /// by any ILogger
    /// </summary>
    public class LoggingScopeMiddleware
    {
        private readonly RequestDelegate _next;
        public const string CONTEXT_PROPERTY_NAME = "ExicoShopifyLoggingContext";

        public LoggingScopeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task InvokeAsync(HttpContext context)
        {
            using (var scope = context.RequestServices.CreateScope())
            {

                var logger = scope.ServiceProvider.GetService<ILogger<Startup>>();
                var result = Helpers.UserInContextHelper.IsLoggedIn(context);
                var data = GetLoggingContext(context, scope).Result;
                using (LogContext.PushProperty(CONTEXT_PROPERTY_NAME, data, true))
                {
                    return _next(context);
                }

            }
        }

        /// <summary>
        /// Gets a context as <see cref="LoggingContext"/> data class that has information about logged in user and store.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public async Task<LoggingContext> GetLoggingContext(HttpContext context, IServiceScope scope)
        {
            var loggedInStatus = UserInContextHelper.IsLoggedIn(context);
            if (!loggedInStatus) return new LoggingContext()
            {
                LoggedIn = false,
                IP = context.Connection.RemoteIpAddress.ToString()
            };
            else
            {
                var caching = scope.ServiceProvider.GetService<IUserCaching>();
                var planReader = scope.ServiceProvider.GetService<IPlansReader>();
                var user = await caching.GetLoggedOnUser();
                if (user != null)
                {
                    var plan = planReader[user.PlanId.HasValue ? user.PlanId.Value : 0];
                    return new LoggingContext()
                    {
                        LoggedIn = true,
                        AccessToken = user.ShopifyAccessToken,
                        Email = user.Email,
                        IP = context.Connection.RemoteIpAddress.ToString(),
                        RequestPath = context.Request.Path,
                        Shop = user.MyShopifyDomain,
                        ChargeId = user.ShopifyChargeId,
                        PlanId = user.PlanId,
                        PlanName = plan?.Name,
                        UserId = user.Id,
                    };
                }
                else
                {
                    //somethings not right, but let us cope with the situation
                    return new LoggingContext()
                    {
                        LoggedIn = true,
                        IP = context.Connection.RemoteIpAddress.ToString(),
                        UserId = UserInContextHelper.GetCurrentUserId(context),
                        Shop = context.User.Identity.Name,
                        Comment = "User is logged in but not present in the cache. Deleted from the db??"
                    };
                }
            }
        }
    }

    [Serializable]
    public class LoggingContext
    {
        public bool LoggedIn { get; set; } = false;
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Shop { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public int? PlanId { get; set; } = null;
        public string AccessToken { get; set; } = string.Empty;
        public string RequestPath { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public long? ChargeId { get; set; } = null;
        public string Comment { get; set; } = string.Empty;


    }

}
