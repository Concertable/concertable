using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.SeedData;

public class Location
{
    public required string County { get; set; }
    public required string Town { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
