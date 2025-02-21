using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ApiModels
{
    public class GoogleGeocodeResponse
    {
        public List<GoogleGeocodeResult> Results { get; set; }
    }
}
