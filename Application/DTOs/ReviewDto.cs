using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public int Stars { get; set; }
        public string? Details { get; set; }
    }
}
