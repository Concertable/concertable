using Application.DTOs;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IListingApplicationService
    {
        Task<ListingApplicationDto> GetByIdAsync(int id);
        Task<IEnumerable<ListingApplicationDto>> GetForListingIdAsync(int id);
        Task<IEnumerable<ArtistListingApplicationDto>> GetPendingForArtistAsync();
        Task<IEnumerable<ArtistListingApplicationDto>> GetRecentDeniedForArtistAsync();
        Task ApplyForListingAsync(int listingId);
        Task<(ArtistDto, VenueDto)> GetArtistAndVenueByIdAsync(int id);
        Task<decimal> GetListingPayByIdAsync(int id);
    }
}
