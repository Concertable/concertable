using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class EmailDto
    {
        public required string To { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public IEnumerable<AttachmentDto> Attachments { get; set; }
    }
}
