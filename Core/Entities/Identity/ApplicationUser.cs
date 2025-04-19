using Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities.Identity
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string? County { get; set; }
        public string? Town { get; set; }
        public Point? Location { get; set; }
        public string? StripeId { get; set; }
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    }

    public class Admin: ApplicationUser { }

    public class Customer : ApplicationUser
    {
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }

    public class VenueManager : ApplicationUser
    {
        public Venue? Venue { get; set; }
    }

    public class ArtistManager : ApplicationUser
    {
        public Artist? Artist { get; set; }
    }

    public class ApplicationRole : IdentityRole<int> { }
}
