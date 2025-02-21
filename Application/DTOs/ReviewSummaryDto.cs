using Core.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ReviewSummaryDto
    {
        public double AverageRating { get; set; }
        public PaginationResponse<ReviewDto> Reviews { get; set; }
    }
}
