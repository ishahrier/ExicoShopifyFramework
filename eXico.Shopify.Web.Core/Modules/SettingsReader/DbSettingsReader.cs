using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules
{

    /// <summary>
    /// Default Settings reader from the database.
    /// </summary>
    /// <seealso cref="Exico.Shopify.Web.Core.Modules.IDbSettingsReader" />
    public class DbSettingsReader : IDbSettingsReader
    {
        /// <summary>
        /// The system settings cache key, <c>DefaultSettingsReader</c> uses this value as cahching key.
        /// </summary>
        public const string SYSTEM_SETTINGS_CACHE_KEY = "SYSTEM_SETTINGS_CACHE_KEY";
        /// <summary>
        /// The cache for settings expire after 12 hours.
        /// </summary>
        public const int CACHE_EXPIRE_AFTER_HOURS = 12; //TODO config?
        private readonly ILogger<IDbSettingsReader> Logger;
        private readonly IDbService<SystemSetting> DbService;
        private readonly IMemoryCache MemoryCache;

        private Dictionary<string, Dictionary<string, SystemSetting>> _GroupedSetting { get; set; }

        /// <summary>
        /// Gets all settings defined in the database SystemSettings table.
        /// </summary>
        /// The key of the dictionary is the name of the settings group.
        /// And the value is another dictionary which has the setting's name as key
        /// and the value is the <see cref="Exico.Shopify.Data.Domain.DBModels.SystemSetting"/> object.
        /// </para>
        /// </value>
        public Dictionary<string, Dictionary<string, SystemSetting>> AllSettings => _GroupedSetting;

        /// <summary>
        /// Calls the <seealso cref="_LoadSettings(IDbService&lt;SystemSetting&rt;, IMemoryCache)" /> method.
        /// </summary>
        /// <param name="service">Service to read SystemSettings table.</param>
        /// <param name="cache">AspNet core cahcing service.</param>
        public DbSettingsReader(IDbService<SystemSetting> service, IMemoryCache cache, ILogger<IDbSettingsReader> logger)
        {
            Logger = logger;
            this.DbService = service;
            this.MemoryCache = cache;
            _LoadSettings();
        }

        /// <summary>
        /// Gets all the settings for a settings group.
        /// </summary>
        /// <param name="groupName">Name of the settings group.</param>
        /// <returns>
        /// A <c>Dictionary</c> of settings, where the key is the name of the setting and the value is <see cref="Exico.Shopify.Data.Domain.DBModels.SystemSetting"/> object
        /// if the group name is found, otherwise <c>null</c>.
        /// </returns>
        public Dictionary<String, SystemSetting> GetSettings(string groupName)
        {
            var gName = _GroupedSetting.ContainsKey(groupName);
            if (gName == false) Logger.LogWarning($"System settings group '{groupName}' not found.Returning null.");
            Logger.LogInformation($"Found system setting group '{groupName}'.");
            var root = gName ? _GroupedSetting[groupName] : null;
            return root;
        }

        /// <summary>
        /// Gets a very specific setting.
        /// </summary>
        /// <param name="groupName">Name of the group for the setting</param>
        /// <param name="settingName">Name of the setting</param>
        /// <returns>
        /// The <see cref="Exico.Shopify.Data.Domain.DBModels.SystemSetting"/> if group name and setting name supplied exist,
        /// if any one of them doesn't exist then <c>null</c> is returned.
        /// </returns>
        public SystemSetting GetSetting(string groupName, string settingName)
        {
            var gName = _GroupedSetting.ContainsKey(groupName);
            if (gName == false) Logger.LogWarning($"System settings group name '{groupName}' not found.Returning null.");
            var root = gName ? _GroupedSetting[groupName] : null;
            if (root == null) return null;
            else
            {
                var sName = root.ContainsKey(settingName);
                if (sName == false) Logger.LogWarning($"Setting name '{sName}' in '{groupName}' not found.Returning null.");
                var child = sName ? root[settingName] : null;
                return child;
            }
        }

        /// <summary>
        /// Get the value of a setting.
        /// </summary>
        /// <param name="groupName">Name of the group for the setting.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>
        /// Return Value if the setting (which is always a <c>string</c>).
        /// Returns empty string if either group name or the setting name is not found.
        /// </returns>
        public string GetValue(string groupName, string settingName)
        {
            var data = GetSetting(groupName, settingName);
            if (data != null)
            {
                var valueNotFound = (String.IsNullOrEmpty(data.Value) || String.IsNullOrWhiteSpace(data.Value));
                if (valueNotFound) Logger.LogInformation($"Returning default value for [{groupName}][{settingName}].");
                return valueNotFound ? data.DefaultValue : data.Value;
            }
            else
            {
                return string.Empty;
            }

        }

        /// <summary>
        /// Everytime it is called, it goes to database, reads in the settings and assigns into the <see cref="_GroupedSetting"/>
        /// and then saves it into cache.
        /// </summary>
        public void ReloadFromDbAndUpdateCache()
        {
            Logger.LogInformation("Starting reloading system settings from database.");
            var __GroupedSetting = DbService.FindAll()
                              .GroupBy(p => p.GroupName)
                              .ToDictionary(r => r.Key, q => q.ToDictionary(w => w.SettingName, v => v));
            Logger.LogInformation("Done reloading settings from database.");
            Logger.LogInformation($"Total {__GroupedSetting.Values.Sum(list => list.Count)} settings found in the database. ");
            Logger.LogInformation("Now saving system settings to memory cache.");
            MemoryCache.Set(SYSTEM_SETTINGS_CACHE_KEY, __GroupedSetting, new MemoryCacheEntryOptions()
            {
                Priority = CacheItemPriority.NeverRemove,
                SlidingExpiration = TimeSpan.FromHours(CACHE_EXPIRE_AFTER_HOURS)

            });
            _GroupedSetting = (Dictionary<string, Dictionary<string, SystemSetting>>)MemoryCache.Get(SYSTEM_SETTINGS_CACHE_KEY);
            Logger.LogInformation("Done saving system settings to memory cache.");


        }

        /// <summary>
        /// Checks if cache has the settings dictionary already, if not then  
        /// calls the <see cref="ReloadSettings()"/> method.
        /// </summary>
        protected void _LoadSettings()
        {
            Logger.LogInformation("Starting loading system settings.Checking cache first.");
            _GroupedSetting = (Dictionary<string, Dictionary<string, SystemSetting>>)MemoryCache.Get(SYSTEM_SETTINGS_CACHE_KEY);
            if (_GroupedSetting == null)
            {
                Logger.LogInformation("System settings memory cache is empty.Requesting to reload from db.");
                ReloadFromDbAndUpdateCache();
            }
            else
            {
                Logger.LogInformation("Skipping Reloading from db. Cache has a copy of system settings.");
                Logger.LogInformation($"Total {AllSettings.Values.Sum(list => list.Count)} settings found in the cache. ");
            }
            Logger.LogInformation("Finished loading system settings.");
        }


    }
}

