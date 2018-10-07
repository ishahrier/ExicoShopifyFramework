using System.Threading.Tasks;
using Exico.Shopify.Data.Domain.AppModels;

namespace Exico.Shopify.Web.Core.Modules
{
    /// <summary>
    /// An interface for implementing user caching
    /// The default implementation is <see cref="UserCaching"/> uses "In Memory Caching"
    /// </summary>
    public interface IUserCaching
    {
        /// <summary>
        /// Clears the logged on user data from cache.
        /// </summary>
        void ClearLoggedOnUser();
        
        /// <summary>
        /// Clears the logged on user data from cache. Calls the <see cref="GetUserCacheKey(string)"/> method to determind the cache key for the given user id.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        void ClearUser(string userId);

        /// <summary>
        /// Gets the logged on user's cache key.
        /// </summary>
        /// <returns>The cache key </returns>
        string GetLoggedOnUserCacheKey();

        /// <summary>
        /// Gets the logged on user data from memory cache.
        /// </summary>
        /// <param name="refresh">If set to <c>true</c>, then even if the cache data is available it will still call the <see cref="SetLoggedOnUserInCache"/> method</param>
        /// <returns>
        /// <see cref="Exico.Shopify.Data.Domain.AppModels.AppUser"/>
        /// </returns>

        Task<AppUser> GetLoggedOnUser(bool refresh = false);
        /// <summary>
        /// Gets the user's cache key.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>The cach key </returns>
        string GetUserCacheKey(string userId);

        /// <summary>
        /// Gets a copy of the user data the from DB by logged on user id and sets it into memory cache.
        /// </summary>        
        /// <returns>
        /// <see cref="Exico.Shopify.Data.Domain.AppModels.AppUser"/>
        /// </returns>
        Task<AppUser> SetLoggedOnUserInCache();
    }
}