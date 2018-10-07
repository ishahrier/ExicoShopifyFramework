using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Exico.Shopify.Data.Domain.DBModels
{
    public class PlanDefinition
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string OptionName { get; set; }
        [Required]
        [StringLength(100)]
        public string OptionValue { get; set; }
        [Required]
        public string Description { get; set; }

        public int PlanId { get; set; }
        public   Plan Plan { get; set; }
    }
}
