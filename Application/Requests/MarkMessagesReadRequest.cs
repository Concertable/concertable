using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Requests
{
    public class MarkMessagesReadRequest
    {
        [MinLength(1, ErrorMessage = "Require one MessageId minimum")]
        public List<int> MessageIds { get; set; }
    }
}
