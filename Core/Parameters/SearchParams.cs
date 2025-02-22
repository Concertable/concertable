using Core.ModelBinders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Parameters
{
    public class SearchParams : PaginationParams
    {
        public string? SearchTerm { get; set; }
        public DateTime? Date { get; set; }
        public string? Sort { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? RadiusKm { get; set; }
        public int[]? GenreIds { get; set; }
    }

}
