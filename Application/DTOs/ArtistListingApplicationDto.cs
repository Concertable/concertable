using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ArtistListingApplicationDto
    {
        public int Id { get; set; }
        public ArtistDto Artist { get; set; }
        public ListingWithVenueDto ListingWithVenue { get; set; }
    }
}
