using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Infrastructure.ApiModels;

public class GoogleGeocodeResult
{
    public List<GoogleAddressComponent> Address_Components { get; set; } = [];
}
