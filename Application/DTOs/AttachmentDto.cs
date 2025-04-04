using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class AttachmentDto
    {
        public byte[] Content { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; } = "application/pdf";
    }
}
