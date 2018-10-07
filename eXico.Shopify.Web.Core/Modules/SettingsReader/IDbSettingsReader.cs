using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules
{


    /// <summary>
    ///  This interface is used to read settings from database SystemSettings table.
    ///  The default implementation is <see cref="DbSettingsReader"/>.
    ///  If you are replacing <see cref="DbSettingsReader"/> then you should
    ///  use this interface to implement your own system settings reader service.
    ///  <para>Also please look at the docmention of the <see cref="DbSettingsReader"/> to get an 
    ///  idea on how to implement your own</para>
    /// </summary>
    public interface IDbSettingsReader
    {
        /// <summary>
        ///  The key is the name of the Group.
        ///  The value is another Dictionary where the key is the name of the setting
        ///  and the value is the <see cref="Exico.Shopify.Data.Domain.DBModels.SystemSetting"/> object.
        /// </summary>
        Dictionary<String, Dictionary<String, SystemSetting>> AllSettings { get; }
        Dictionary<String,SystemSetting> GetSettings(string groupName);
        SystemSetting GetSetting(string groupName, string settingName);
        string GetValue(string groupName, string settingName);
        /// <summary>
        /// Reloading must always goto database and load a fresh copy and update cache.
        /// <see cref="AllSettings"/> must returns that fresh/updated copy of just loaded settings.
        /// </summary>
        void ReloadFromDbAndUpdateCache( );

    }
}
