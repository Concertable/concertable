using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Parameters
{
    public class EventParams : IGeoParams
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? RadiusKm { get; set; } = 25;
        public IEnumerable<int> GenreIds { get; set; } = Enumerable.Empty<int>();
        public bool OrderByRecent { get; set; } = false;
        public int Take { get; set; }
    }
}
