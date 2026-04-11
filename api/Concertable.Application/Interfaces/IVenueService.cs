using Concertable.Application.DTOs;
using Concertable.Application.Requests;
using Concertable.Application.Responses;

namespace Concertable.Application.Interfaces;

public interface IVenueService
{
    Task<VenueDetailsResponse> GetDetailsByIdAsync(int id);
    Task<VenueDetailsResponse> GetDetailsForCurrentUserAsync();
    Task<VenueDto> CreateAsync(CreateVenueRequest request);
    Task<VenueDto> UpdateAsync(int id, UpdateVenueRequest request);
    Task<int> GetIdForCurrentUserAsync();
    Task ApproveAsync(int id);
}
