using Exico.Shopify.Data.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Exico.Shopify.Web.Core.Modules
{

    /// <summary>
    /// The core default setting items comes with the framework
    /// </summary>
    public enum CORE_SYSTEM_SETTING_NAMES
    {
        API_KEY,
        SECRET_KEY,
        APP_BASE_URL,
        SHOPIFY_CONTROLLER,
        UNINSTALL_CONTROLLER,
        DASHBOARD_CONTOLLER,
        ACCOUNT_CONTOLLER,
        APP_NAME,
        WELCOME_EMAIL_TEMPLATE,
        SHOPIFY_EVENT_EMAIL_SUBSCRIBERS,
        SHOPIFY_EMAILS_FROM_ADDRESS,
        PRIVILEGED_IPS,        
        APP_SUPPORT_EMAIL_ADDRESS,
        APP_VERSION,
        SEEDER_FRAMEWORK_VERSION,/*framework version that seeded this database*/
        SHOPIFY_APP_STOER_URL,
        MY_PROFILE_CONTOLLER,
        SEND_GRID_API_KEY,
        PASSWORD_SALT
    }
    public static class DbSettingsReaderExtensions
    {
        ///// <summary>
        ///// The version number of the framework.This is changed manually by the framework author.
        ///// This is not same as the nuget version nor same as the build number.
        ///// </summary>
        //internal const string FRAMEWORK_VERSION = "1.0.0";

        /// <summary>
        /// The core settings group name
        /// </summary>
        public const string CORE_SETTINGS_GROUP_NAME = "CORE";
        public static string GetValue(this IDbSettingsReader reader, CORE_SYSTEM_SETTING_NAMES settingName) =>
            reader.GetValue(CORE_SETTINGS_GROUP_NAME.ToString(), settingName.ToString());

        public static Dictionary<String, SystemSetting> GetCoreGroupSettings(this IDbSettingsReader reader) =>
            reader.GetSettings(CORE_SETTINGS_GROUP_NAME);


        public static SystemSetting GetSetting(this IDbSettingsReader reader, CORE_SYSTEM_SETTING_NAMES settingName) =>
            reader.GetSetting(CORE_SETTINGS_GROUP_NAME, settingName.ToString());

        public static List<string> GetAdminIps(this IDbSettingsReader reader) =>
            reader.GetValue(CORE_SYSTEM_SETTING_NAMES.PRIVILEGED_IPS)
                  .Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                  .ToList();


        public static string GetPasswordSalt(this IDbSettingsReader reader) =>
            reader.GetValue(CORE_SYSTEM_SETTING_NAMES.PASSWORD_SALT);

        public static string GetAppBaseUrl(this IDbSettingsReader reader) =>
            reader.GetValue(CORE_SYSTEM_SETTING_NAMES.APP_BASE_URL);

        public static string GetAppName(this IDbSettingsReader reader) =>
            reader.GetValue(CORE_SYSTEM_SETTING_NAMES.APP_NAME);

        public static string GetShopifyControllerName(this IDbSettingsReader reader) =>
            reader.GetValue(CORE_SYSTEM_SETTING_NAMES.SHOPIFY_CONTROLLER);

        public static string GetShopifyAppStoreUrl(this IDbSettingsReader reader) =>
            reader.GetValue(CORE_SYSTEM_SETTING_NAMES.SHOPIFY_APP_STOER_URL);

        public static List<string> GetShopifyEmailSubscriberList(this IDbSettingsReader reader) =>
            reader.GetValue(CORE_SYSTEM_SETTING_NAMES.SHOPIFY_EVENT_EMAIL_SUBSCRIBERS)
                  .Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries)
                  .ToList();

        public static string GetShopifySecretKey(this IDbSettingsReader reader) =>
            reader.GetValue(CORE_SYSTEM_SETTING_NAMES.SECRET_KEY);

        public static string GetShopifyApiKey(this IDbSettingsReader reader) =>
         reader.GetValue(CORE_SYSTEM_SETTING_NAMES.API_KEY);

        public static string GetShopifyFromEmailAddress(this IDbSettingsReader reader) =>
            reader.GetValue(CORE_SYSTEM_SETTING_NAMES.SHOPIFY_EMAILS_FROM_ADDRESS);

        public static string GetAppSupportEmailAddress(this IDbSettingsReader reader) =>
         reader.GetValue(CORE_SYSTEM_SETTING_NAMES.APP_SUPPORT_EMAIL_ADDRESS);

        public static string GetAppDashboardControllerName(this IDbSettingsReader reader) =>
            reader.GetValue(CORE_SYSTEM_SETTING_NAMES.DASHBOARD_CONTOLLER);

        public static string GetAppMyProfileControllerName(this IDbSettingsReader reader) =>
            reader.GetValue(CORE_SYSTEM_SETTING_NAMES.MY_PROFILE_CONTOLLER);

        public static string GetAppUninstallControllerName(this IDbSettingsReader reader) =>
            reader.GetValue(CORE_SYSTEM_SETTING_NAMES.UNINSTALL_CONTROLLER);

        public static string GetAccountControllerName(this IDbSettingsReader reader) =>
            reader.GetValue(CORE_SYSTEM_SETTING_NAMES.ACCOUNT_CONTOLLER);

        public static string GetGetSendGridApiKey(this IDbSettingsReader reader) =>
            reader.GetValue(CORE_SYSTEM_SETTING_NAMES.SEND_GRID_API_KEY);


        ///// <summary>
        ///// Returns the framework version.This is not same as the framework build number but it is same as the
        ///// value returned by <see cref="GetSeederFrameworkVersion()"/> method. Also nuget version wont match with this one.
        ///// And lastly this is set manually by the framework author.
        ///// </summary>
        ///// <returns>version number in major.minor.patch pattern.</returns>
        //public static string GerFrameWorkVersion(this IDbSettingsReader reader)
        //{
        //    return FRAMEWORK_VERSION;
        //}

        /// <summary>
        /// This is the version of the application that is built using this framework.
        /// This is set by the app developer not the framework.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static string GetAppVersion(this IDbSettingsReader reader) =>
             reader.GetValue(CORE_SYSTEM_SETTING_NAMES.APP_VERSION);

        /// <summary>
        /// This indicates the framework version that initialized/created the database.
        /// This should not be set manually in the database. But only by the data seeder (<see cref="Extensions.AppBuilderExtensions.UseExicoShopifyFramework(Microsoft.AspNetCore.Builder.IApplicationBuilder)"/>) of the
        /// framework itself so that the framework <seealso cref="FRAMEWORK_VERSION"/> and this
        /// value are same.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static string GetDataSeederFrameworkVersion(this IDbSettingsReader reader) =>
             reader.GetValue(CORE_SYSTEM_SETTING_NAMES.SEEDER_FRAMEWORK_VERSION);
    }
}

