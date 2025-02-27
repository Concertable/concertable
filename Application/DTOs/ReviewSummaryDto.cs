using Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ReviewSummaryDto
    {
        public int TotalReviews { get; set; }
        public double? AverageRating { get; set; }
    }
}
