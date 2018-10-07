using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Exico.Shopify.Data.Domain.DBModels
{
    public   class SystemSetting
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string SettingName { get; set; }
        [Required]
        [StringLength(255)]
        public string DisplayName { get; set; }

                
        public string Description { get; set; }
        [Required]        
        public string GroupName { get; set; }
        [Required]        
        public string Value { get; set; }
        public string DefaultValue { get; set; }
    }
}
