using Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces;
using Exico.Shopify.Web.Core.Modules;

namespace Exico.Shopify.Web.Core.Helpers
{
    /// <summary>
    /// Name of the folders found under a shopify theme
    /// </summary>
    public enum THEME_FOLDER_NAMES
    {
        layout,
        templates,
        assets,
        sections,
        snippets
    };

    /// <summary>
    /// Little helper class for creating shopify related urls
    /// </summary>
    /// //TODO unit test this class
    public class ShopifyUrlHelper
    {
        /// <summary>
        /// Gets the url for handing authenticatoin result coming from shopify API
        /// </summary>
        /// <param name="reader">The settings reader.</param>
        /// <returns>
        /// url as <c>string</c>
        /// </returns>
        public static string GetAuthResultHandlerUrl(IDbSettingsReader reader)
            => $"{reader.GetAppBaseUrl()}/{reader.GetShopifyControllerName()}/{SHOPIFY_ACTIONS.AuthResult}";

        /// <summary>
        /// Gets the url for handing payment charge result coming from shopify API
        /// </summary>
        /// <param name="reader">The settings reader.</param>
        /// <returns>
        /// url as <c>string</c>
        /// </returns>
        public static string GetChargeResultHandlerUrl(IDbSettingsReader reader)
            => $"{reader.GetAppBaseUrl()}/{reader.GetShopifyControllerName()}/{SHOPIFY_ACTIONS.ChargeResult}";

        /// <summary>
        /// Gets the url navigating shopify store admin.
        /// </summary>
        /// <param name="reader">The settings reader.</param>
        /// <returns>
        /// url as <c>string</c>
        /// </returns>
        public static string GetAdminUrl(string myShopifyDomain) => $"https://{myShopifyDomain}/admin";

        /// <summary>
        /// Gets the url for handling user's selected plan
        /// </summary>
        /// <param name="reader">The settings reader.</param>
        /// <returns>
        /// url as <c>string</c>
        /// </returns>
        public static string GetSelectedPlanHandlerUrl(IDbSettingsReader reader, int planId)
            => $"{reader.GetAppBaseUrl()}/{reader.GetShopifyControllerName()}/{SHOPIFY_ACTIONS.SelectedPlan}?planId={planId}";

        /// <summary>
        /// Gets the url for presenting plans to user
        /// </summary>
        /// <param name="reader">The settings reader.</param>
        /// <returns>
        /// url as <c>string</c>
        /// </returns>
        public static string GetPlanChoosingUrl(IDbSettingsReader reader)
            => $"{reader.GetAppBaseUrl()}/{reader.GetShopifyControllerName()}/{SHOPIFY_ACTIONS.ChoosePlan}";

        /// <summary>
        /// Gets the liquid file link. CLicking the link should take the user to that liquid file in the shopify admin site.
        /// Would require user to login as admin if not already.
        /// </summary>
        /// <param name="folder">The theme folder. <see cref="THEME_FOLDER_NAMES"/>.</param>
        /// <param name="myShopifyDomain">My shopify domain url i.e mystore.myshopify.com</param>
        /// <param name="fileName">Name of the file. i.e. cart.</param>
        /// <param name="extension">The extension.Default is .liquid.</param>
        /// <returns>
        /// The url as <c>HtmlString</c>
        /// </returns>
        public static string GetLiquidFileLink(THEME_FOLDER_NAMES folder, string myShopifyDomain, string fileName, string extension = ".liquid")
            => $"https://{myShopifyDomain}/admin/themes/current/?key={folder.ToString()}/{fileName}{extension}";

        /// <summary>
        /// Get the apps rating page url in shopify app store.
        /// </summary>
        /// <param name="reader">The settings reader.</param>
        /// <param name="storeFriendlyAppName">Store friendly Name of the application. i.e. shopify app store friendly verison of "Exico Cross Sell" is "exico-cross-sell"
        /// <see cref="GetStoreFriendlyAppName(string appName, char[])"/>
        /// </param>
        /// <returns>
        /// The url as <c>HtmlString</c>
        /// </returns>
        public static string GetRateMyAppUrl(IDbSettingsReader reader, string storeFriendlyAppName)
            => $"{reader.GetShopifyAppStoreUrl()}/{storeFriendlyAppName}";

        /// <summary>
        /// Converts the app name to shopify app store friendly version.
        /// For example store friendly verison of "Exico Cross Sell" is "exico-cross-sell"
        /// </summary>
        /// <param name="reader">The settings reader.</param>
        /// <param name="replaceWithDashes">Array of characters that must be replaced with '-' in addition to ' ' (space cahracter)</param>
        /// <returns>
        /// App store friendly app name
        /// </returns>
        public static string MakeStoreFriendlyAppName(string appName, char[] replaceWithDashes = null)
        {
            var dashedAppName = appName.Replace(' ', '-');
            if (replaceWithDashes != null)
                foreach (var c in replaceWithDashes)
                    dashedAppName = dashedAppName.Replace(c, '-');

            return dashedAppName.ToLower();
        }

        /// <summary>
        /// Gets the application uninstall web hook URL.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public static string GetAppUninstallWebHookUrl(IDbSettingsReader settings, string userId)
            => GetAppUninstallWebHookUrl($"{settings.GetAppBaseUrl()}/{settings.GetAppUninstallControllerName()}/{UNINSTALL_ACTIONS.AppUninstalled}",userId);

        /// <summary>
        /// This actually formats the uninstall web hook call back url.
        /// It only adds a userId param at the end of the call back url.
        /// i.e. https://www.site.com/uninstallcontroller/action will be formatted as https://www.site.com/uninstallcontroller/action?userId=123
        /// </summary>
        /// <param name="baseCallbackUrl"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static string GetAppUninstallWebHookUrl(string baseCallbackUrl, string userId)
            => $"{baseCallbackUrl}?userId={userId}";
    }
}
