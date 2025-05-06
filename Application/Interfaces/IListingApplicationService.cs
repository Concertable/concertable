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
        /// <summary>
        /// Gets the list of Applications that havent expired, and dont yet have
        /// an event created
        /// </summary>
        Task<IEnumerable<ArtistListingApplicationDto>> GetPendingForArtistAsync();
        /// <summary>
        /// Gets the list of Applications where another application was accepted
        /// over these ones
        /// </summary>
        Task<IEnumerable<ArtistListingApplicationDto>> GetRecentDeniedForArtistAsync();
        Task ApplyForListingAsync(int listingId);
        Task<(ArtistDto, VenueDto)> GetArtistAndVenueByIdAsync(int id);
        Task<decimal> GetListingPayByIdAsync(int id);
    }
}
