using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int FromUserId { get; set; }
        public string Action { get; set; }
        public string Content { get; set; }
    }
}
