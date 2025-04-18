using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ListingApplicationDto
    {
        public int Id { get; set; }
        public ArtistDto Artist { get; set; }
        public ListingDto Listing { get; set; }
        public ApplicationStatus Status { get; set; }
    }
}
