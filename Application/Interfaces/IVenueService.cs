using Application.DTOs;
using Application.Requests;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface IVenueService
{
    Task<VenueDto> GetDetailsByIdAsync(int id);
    Task<VenueDto?> GetDetailsForCurrentUserAsync();
    Task<VenueDto> CreateAsync(CreateVenueRequest request, IFormFile image);
    Task<VenueDto> UpdateAsync(VenueDto venueDto, IFormFile? image);
    Task<int> GetIdForCurrentUserAsync();
}
