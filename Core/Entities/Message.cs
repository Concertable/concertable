using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Message : BaseEntity
    {
        public string Content { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public string Action { get; set; }
        public DateTime SentDate { get; set; }
        public bool Read { get; set; }
        public ApplicationUser FromUser { get; set; }
        public ApplicationUser ToUser { get; set; }
    }
}
