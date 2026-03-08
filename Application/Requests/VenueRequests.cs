using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Requests;

public record CreateVenueRequest(string Name, string About, double Latitude, double Longitude);

public record VenueCreateRequest(CreateVenueRequest Venue, IFormFile Image);

public record VenueUpdateRequest
{
    public required VenueDto Venue { get; set; }
    public IFormFile? Image { get; set; }
}
