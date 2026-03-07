using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Requests
{
    public record CreateVenueRequest
    {
        public string Name { get; set; }
        public string About { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public record VenueCreateRequest
    {
        public CreateVenueRequest Venue { get; set; }
        public IFormFile Image { get; set; }
    }

    public record VenueUpdateRequest
    {
        public VenueDto Venue { get; set; }
        public IFormFile? Image { get; set; }
    }
}
