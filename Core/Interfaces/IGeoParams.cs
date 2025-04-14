using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IGeoParams
    {
        double? Latitude { get; }
        double? Longitude { get; }
        int? RadiusKm { get; }
    }
}
