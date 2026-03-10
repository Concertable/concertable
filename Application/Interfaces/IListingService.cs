using Application.DTOs;
using Core.Entities;

namespace Application.Interfaces;

public interface IListingService
{
    Task CreateAsync(ListingDto listingDto);
    Task CreateMultipleAsync(IEnumerable<ListingDto> listingsDto);
    Task<IEnumerable<ListingDto>> GetActiveByVenueIdAsync(int id);
    Task<User> GetOwnerByIdAsync(int id);
    Task<Listing> GetByIdAsync(int id);
}
