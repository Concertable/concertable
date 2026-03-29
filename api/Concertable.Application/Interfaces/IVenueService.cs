using Concertable.Application.DTOs;
using Concertable.Application.Requests;

namespace Concertable.Application.Interfaces;

public interface IVenueService
{
    Task<VenueDto> GetDetailsByIdAsync(int id);
    Task<VenueDto?> GetDetailsForCurrentUserAsync();
    Task<VenueDto> CreateAsync(CreateVenueRequest request);
    Task<VenueDto> UpdateAsync(int id, UpdateVenueRequest request);
    Task<int> GetIdForCurrentUserAsync();
    Task ApproveAsync(int id);
}
