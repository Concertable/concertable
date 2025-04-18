using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOwnershipService
    {
        Task<bool> OwnsVenueAsync(int venueId);
        Task<bool> OwnsListingAsync(int listingId);
        Task<bool> OwnsArtistAsync(int applicationId);
        Task<bool> OwnsListingByApplicationId(int applicationId);
    }

}
