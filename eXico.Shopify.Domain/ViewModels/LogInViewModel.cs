using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Exico.Shopify.Data.Domain.ViewModels
{
    /// <summary>
    /// used in the logging screen to let user enter username (my shopify domain name) and password
    /// </summary>
    public class LoginViewModel
    { 
        [Required]        
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }
}
