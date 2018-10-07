using System;
using System.ComponentModel.DataAnnotations;

namespace Exico.Shopify.Data.Domain.ViewModels
{
    public class ContactUsViewModel
    {
        public ContactUsViewModel()
        {

        }
        [Required]
        [EmailAddress]
        public string FromEmail { get; set; }
        [Required]
        public String ShopDomain { get; set; }
        [Required]
        public String PlanName { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Message { get; set; }

        [Required]
        public string Name { get; set; }

    }
}
