using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Helpers
{
    /// <summary>
    /// MD5 Hash Helper class.
    /// This also add an extension to <c>string</c> so that
    /// any string can be converted to MD5 hash
    /// </summary>
    public static class HashHelper
    {

        /// <summary>
        /// String extension method, converts any string to MD5 hash value
        /// </summary>
        /// <example>
        /// <code>
        /// string s = "HelloWord";
        /// s.ToMD5();
        /// </code>
        /// </example>
        /// <param name="s">The MD5 this.</param>
        /// <returns>MD5 Hashed string</returns>
        public static string ToMd5(this string s)
        {
            return CreateMD5(s);
        }


        /// <summary>
        /// Creates the MD5 hash of a given string.
        /// </summary>
        /// <example>
        /// <code>
        /// string s = "hellow World";
        /// HashHelper.CreateMD5(s);
        /// </code>
        /// </example>
        /// <param name="s">The string that needs to be hsahed</param>
        /// <returns>MD5 Hashed string</returns>
        public static string CreateMD5(string s)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(s);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}
