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
        public string? Location { get; set; }
        public DateTime? Date { get; set; }
        public string? Sort { get; set; }

        public int[] GenreIds { get; set; }
    }

}
