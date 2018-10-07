using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Web.Core.Extensions;
using Exico.Shopify.Web.Core.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules
{
    /// <summary>
    /// Helper class for storing a currently logged on users user and shop related data into cache ,
    /// so that we can save return trip to the actual storage.
    /// </summary>
    public class UserCaching : IUserCaching
    {

        public const string USER_CACHE_KEY_PREFIX = "_user_";//TODO from config ?        

        private readonly IHttpContextAccessor _HttpContextAccessor;
        private readonly IMemoryCache _Cache;
        private readonly SignInManager<AspNetUser> _SignInManager;
        private readonly UserManager<AspNetUser> _Store;
        private readonly ILogger<UserCaching> _Logger;
        /// <summary>
        /// Defauld logged on user data caching timeout is 10 minutes
        /// </summary>
        public readonly double USER_CACHE_EXPIRE_MINUTES = 10;

        public UserCaching(SignInManager<AspNetUser> signInManager,IHttpContextAccessor hca, IMemoryCache cache, UserManager<AspNetUser> store,   ILogger<UserCaching> logger)
        {
            _HttpContextAccessor = hca;
            _Store = store;
            _Logger = logger;
            _Cache = cache;
            _SignInManager = signInManager;

        }

        /// <summary>
        /// Gets a copy of the user data the from DB by logged on user id and sets it into memory cache.
        /// </summary>        
        /// <returns>
        /// <see cref="Exico.Shopify.Data.Domain.AppModels.AppUser"/>
        /// </returns>
        public async Task<AppUser> SetLoggedOnUserInCache()
        {
            AppUser cacheData = null;
            if (_HttpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                AspNetUser user = await _Store.GetClaimedUserData(_HttpContextAccessor.HttpContext.User);
                if (user != null)
                {
                    var cKey = GetUserCacheKey(user.Id);
                    _Logger.LogInformation($"Setting logged on user in to cache with key = '{cKey}'.");
                    cacheData = new AppUser(user, UserInContextHelper.IsCurrentUserIdAdmin(_HttpContextAccessor.HttpContext));
                    _Cache.Set<AppUser>(cKey, cacheData, new MemoryCacheEntryOptions()
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(USER_CACHE_EXPIRE_MINUTES),
                        Priority = CacheItemPriority.High,
                    });
                    _Logger.LogInformation("Returning cached logged in user {@user}.", cacheData);
                }
                else
                {
                    var errMsg = $"User {_HttpContextAccessor.HttpContext.User.Identity.Name} is logged in user but identity is not found in our database.";
                    _Logger.LogError(new Exception(errMsg), errMsg + "{@data}", _HttpContextAccessor.HttpContext.User.Identity.Name);
                    _Logger.LogWarning("Force signing out.");
                    await _SignInManager.SignOutAsync();
                    _Logger.LogInformation("Redirecting to / ");
                    _HttpContextAccessor.HttpContext.Response.Redirect("/");
                    
                };
            }
            else _Logger.LogInformation("Returning null because user is not logged in.");

            return cacheData;
        }

        /// <summary>
        /// Gets the logged on user data from memory cache.
        /// </summary>
        /// <param name="refresh">If set to <c>true</c>, then even if the cache data is available it will still call the <see cref="SetLoggedOnUserInCache"/> method</param>
        /// <returns>
        /// <see cref="Exico.Shopify.Data.Domain.AppModels.AppUser"/>
        /// </returns>
        public async Task<AppUser> GetLoggedOnUser(bool refresh = false)
        {
            AppUser cacheData = null;
            if (_HttpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                var userId = UserInContextHelper.GetCurrentUserId(_HttpContextAccessor.HttpContext);
                var key = GetUserCacheKey(userId);
                if (_Cache.TryGetValue<AppUser>(key, out cacheData))
                {
                    _Logger.LogInformation($"Data found in cache for key = {key}");

                    if (refresh)
                    {
                        _Logger.LogInformation($"Refreshing cache data for key = {key} becuase refresh is requested.");
                        cacheData = await SetLoggedOnUserInCache();
                    }
                }
                else
                {
                    _Logger.LogWarning($"Missing cache data for logged on user for key = {key}. Save logged on user data in cache now.");
                    cacheData = await SetLoggedOnUserInCache();
                }
            }
            else _Logger.LogInformation("Returning null because user is not logged in.");

            return cacheData;
        }

        /// <summary>
        /// Clears the logged on user data from cache.
        /// It can automatically determins the cach key for the current logged in user.
        /// </summary>
        public void ClearLoggedOnUser()
        {
            if (_HttpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                var userId = UserInContextHelper.GetCurrentUserId(_HttpContextAccessor.HttpContext);
                _Logger.LogInformation($"Clearning cache for user id =  {userId}");
                ClearUser(userId);
            }
            else _Logger.LogInformation("Didn't clear anything in cache, because user is not logged in.");
        }

        /// <summary>
        /// Clears the logged on user data from cache. Calls the <see cref="GetUserCacheKey(string)"/> method to determind the cache key for the given user id.
        /// </summary>
        /// <param name="userId">The user id.</param>
        public void ClearUser(string userId)
        {
            var key = GetUserCacheKey(userId);
            _Logger.LogInformation($"Clearning cache for key = {key}");
            _Cache.Remove(key);
            //this._Cache.Set<AppUser>(key, null);

        }

        /// <summary>
        /// Gets the user's cache key.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>The cach key </returns>
        public string GetUserCacheKey(string userId)
        {
            return USER_CACHE_KEY_PREFIX + userId;
        }

        /// <summary>
        /// Gets the logged on user's cache key.
        /// </summary>
        /// <returns>The cache key </returns>
        public string GetLoggedOnUserCacheKey()
        {
            if (_HttpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                return GetUserCacheKey(UserInContextHelper.GetCurrentUserId(_HttpContextAccessor.HttpContext));
            }
            else
            {
                _Logger.LogInformation("Returning null as current logged on users cache key becuase user is not logged on.");
                return null;
            }
        }

    }
}
