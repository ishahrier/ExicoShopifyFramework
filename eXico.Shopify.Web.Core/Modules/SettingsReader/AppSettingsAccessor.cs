using Microsoft.Extensions.Configuration;
using System;

namespace Exico.Shopify.Web.Core.Modules
{
    /// <summary>
    /// Sometimes we need to use Config.Bind().
    /// But the porblem is that this Bind() method is an extension method and hard or not possible to
    /// mock using mocking framework. Thats why this interface is created for so that we can solve this 
    /// above issue.
    /// </summary>
    public interface IAppSettingsAccessor
    {
        /// <summary>
        /// This wuld read the appsettings file using the IConfiguration and
        /// bind the section (identified by the key) and return correct 
        /// object identified by T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="config"></param>
        /// <returns></returns>        
        T BindObject<T>(string key,IConfiguration config);
    }


    public class AppSettingsAccessor : IAppSettingsAccessor
    {

        public T BindObject<T>(string key, IConfiguration config)
        {
            T obj = (T)Activator.CreateInstance(typeof(T));
            config.Bind(key, obj);
            return obj;
        }
    }
}
