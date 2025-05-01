using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Requests
{
    public class CreateReviewRequest
    {
        public int EventId { get; set; }
        public int Stars { get; set; }
        public string? Details { get; set; }
    }
}
