using Application.DTOs;
using Application.Requests;

namespace Application.Interfaces;

public interface IVenueService
{
    Task<VenueDto> GetDetailsByIdAsync(int id);
    Task<VenueDto?> GetDetailsForCurrentUserAsync();
    Task<VenueDto> CreateAsync(CreateVenueRequest request);
    Task<VenueDto> UpdateAsync(int id, UpdateVenueRequest request);
    Task<int> GetIdForCurrentUserAsync();
}
