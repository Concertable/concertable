using Application.DTOs;
using Core.Entities;
using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IListingService
    {
        Task CreateAsync(ListingDto listingDto);
        Task CreateMultipleAsync(IEnumerable<ListingDto> listingsDto);
        Task<IEnumerable<ListingDto>> GetActiveByVenueIdAsync(int id);
        Task<VenueManager> GetOwnerByIdAsync(int id);
        Task<Listing> GetByIdAsync(int id);
    }
}
