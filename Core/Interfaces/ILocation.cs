using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ILocation
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
