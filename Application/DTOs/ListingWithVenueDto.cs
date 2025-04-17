using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ListingWithVenueDto
    {
        public ListingDto Listing { get; set; }
        public VenueDto Venue { get; set; }
    }
}
