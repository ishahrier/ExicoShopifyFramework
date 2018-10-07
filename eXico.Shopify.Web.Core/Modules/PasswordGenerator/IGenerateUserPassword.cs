using Exico.Shopify.Data.Domain.AppModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules
{
    /// <summary>
    /// Used in implementing password generation for user 
    /// </summary>
    public interface IGenerateUserPassword
    {
        /// <summary>
        /// Gets the password for an app user <see cref="Exico.Shopify.Data.Domain.DBModels.AspNetUser"/>
        /// </summary>
        /// <param name="info">The information required in generating a password.        
        /// <remarks>
        /// Please note that this method should return same password everytime for the same info <see cref="PasswordGeneratorInfo"/> everytime.
        /// </remarks>        
        /// </param>
        /// <returns>
        /// Generated password (for a user)
        /// </returns>
        string GetPassword(PasswordGeneratorInfo info);
    }

}
