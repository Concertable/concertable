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
        public UserDto FromUser { get; set; }
        public ActionDto? Action { get; set; }
        public string Content { get; set; }
    }
}
