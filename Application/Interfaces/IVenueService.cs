using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface IVenueService
    {
        Task<VenueDto> GetDetailsByIdAsync(int id);
        Task<VenueDto?> GetDetailsForCurrentUserAsync();
        Task<VenueDto> CreateAsync(CreateVenueDto venue, IFormFile image);
        Task<VenueDto> UpdateAsync(VenueDto venueDto, IFormFile? image);
        Task<int> GetIdForCurrentUserAsync();
    }
}
