using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Exico.Shopify.Data.Domain.DBModels
{
    public class UserInbox
    {
        public int Id { get; set; }
        public DateTime? ReadOn { get; set; }
        public bool Sticky { get; set; }

        [StringLength(450)]
        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]        
        public AspNetUser User { get; set; }

        public int MessageId { get; set; }
        public Message Message { get; set; }
    }
}
