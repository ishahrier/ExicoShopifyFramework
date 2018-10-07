using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Plugins.WebMsg
{
    /// <summary>
    /// To understand this class please <see cref="IWebMsgConfig"/>.
    /// This class is registered as default <c>IWebMsgConfig</c> service in the <see cref="Extensions.AppBuilderExtensions"/>
    /// </summary>
    /// <seealso cref="Exico.Shopify.Web.Core.Plugins.WebMsg.IWebMsgConfig" />
    public class DefaultWebMsgConfig : IWebMsgConfig
    {
        /// <summary>
        /// Gets or sets the misc/additional config item.
        /// </summary>
        /// <value>
        /// The misc/additional config items.
        /// </value>
        private Dictionary<string, string> _MiscItems;
        private readonly ILogger<DefaultWebMsgConfig> _Logger;

        public DefaultWebMsgConfig(IConfiguration config, ILogger<DefaultWebMsgConfig> logger)
        {
            this._Logger = logger;
            int intVal;

            if(Int32.TryParse( config["WebMsgAutoHideInterval"],out intVal))            
                this.AutoHideInterval = intVal;            
            else
            {
                logger.LogInformation($"Appsettings.json doesnt have a valid value for 'WebMsgAutoHideInterval' item. Falling back to default value.");
                this.AutoHideInterval = 10000;
            }
            logger.LogInformation($"Auto hide interval is set to {this.AutoHideInterval}");

            this.ViewName = config["WebMsgViewName"];
            if (string.IsNullOrEmpty(this.ViewName))
            {
                logger.LogInformation($"Appsettings.json doesnt have a valid value for 'WebMsgViewName' item. Falling back to default value.");
                this.ViewName = "_ExicoWebMsg";
            }
            logger.LogInformation($"View name is set to '{this.ViewName}'.");

            this.Key = config["WebMsgKey"];
            if (string.IsNullOrEmpty(this.Key))
            {
                logger.LogInformation($"Appsettings.json doesnt have a valid value for 'WebMsgKey' item. Falling back to default value.");
                this.Key = "_ExicoWebMsgKey";
            }

            logger.LogInformation($"Key is set to '{this.Key}'.");

            this._MiscItems = new Dictionary<string, string>();
        }

        public string ViewName { get; set; }

        public int AutoHideInterval { get; set; }

        public string Key { get; set; }

        public void AddConfig(string key, string value)
        {
            _MiscItems[key] = value;
            _Logger.LogInformation($"Added config item.'{key}'=>'{value}'");
        }

        public string GetConfig(string key)
        {
            if (_MiscItems.Keys.Contains(key))
            {
                _Logger.LogInformation($"Found config key '{key}'.Returning value.");
                return _MiscItems[key];
            }
            else
            {
                _Logger.LogWarning($"Could not found config key '{key}'. Returning null.");
                return null;
            }

        }
    }
}