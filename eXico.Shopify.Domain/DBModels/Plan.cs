using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Exico.Shopify.Data.Domain.DBModels
{
    public class Plan
    {
        public Plan()
        {
            PlanDefinitions = new List<PlanDefinition>();
        }
        public int Id { get; set; }
        [Required]
        [MaxLength(30)]        
        public string Name { get; set; }
        public short TrialDays { get; set; }
        public bool IsTest { get; set; }
        [Required]
        public int DisplayOrder { get; set; }
        public decimal Price { get; set; }
        [Required]
        public string Description { get; set; }
        public string Footer { get; set; }
        public bool Active { get; set; }
        public bool IsDev { get; set; }
        public bool IsPopular { get; set; }
        public virtual List<PlanDefinition> PlanDefinitions { get; set; }
    }
}
