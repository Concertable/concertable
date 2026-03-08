
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public  class ListingApplication : BaseEntity
    {
        public int ListingId { get; set; }
        public int ArtistId { get; set; }
        public Listing Listing { get; set; } = null!;
        public Artist Artist { get; set; } = null!;
        public Concert? Concert { get; set; }
    }
}
