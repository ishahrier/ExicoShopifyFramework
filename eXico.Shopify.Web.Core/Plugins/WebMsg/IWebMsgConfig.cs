namespace Exico.Shopify.Web.Core.Plugins.WebMsg
{
    /// <summary>
    /// Indicates the nature of the message
    /// </summary>
    public enum WEB_MSG_TYPE
    {
        SUCCESS,
        INFORMATION,
        DANGER,
        WARNING
    }

    /// <summary>
    /// The default implenetation is <see cref="DefaultWebMsgConfig"/>.
    /// This is registered in as service in the <see cref="Extensions.AppBuilderExtensions"/>.
    /// </summary>
    public interface IWebMsgConfig
    {
        /// <summary>
        /// Gets or sets the name of the view that will render the message
        /// </summary>
        /// <value>
        /// The name of the view.
        /// </value>
        string ViewName { get; set; }
        /// <summary>
        /// Gets or sets the automatic hide interval.
        /// </summary>
        /// <value>
        /// The automatic hide interval.
        /// </value>
        int AutoHideInterval { get; set; }
        /// <summary>
        /// Gets or sets the key for storing the message in Temp and in ViewData
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        string Key { get; set; }
        /// <summary>
        /// Adds custom configuration item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        void AddConfig(string key, string value);
        /// <summary>
        /// Gets the custom configuration item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The config value as string</returns>
        string GetConfig(string key);
    }
}