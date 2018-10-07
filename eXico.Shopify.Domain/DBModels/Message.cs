using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Exico.Shopify.Data.Domain.DBModels
{
    public class Message
    {
        public Message()
        {
            UserInboxes = new List<UserInbox>();
        }
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Subject { get; set; }
        public bool DoPopUp { get; set; }
        [Required]
        public DateTime CreatedOn { get; set; }
        public bool IsPermanent { get; set; }
        public bool HighPriority { get; set; }
        public DateTime? PublishedOn { get; set; }
        [Required]
        public string MessageBody { get; set; }
        public  List<UserInbox> UserInboxes { get; set; }  
    }
}
