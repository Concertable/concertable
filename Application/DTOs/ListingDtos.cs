using Application.Serializers;
using Core.Enums;
using System.Text.Json.Serialization;

namespace Application.DTOs
{
    public record ListingDto
    {
        public int? Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IEnumerable<GenreDto> Genres { get; set; } = new List<GenreDto>();
        public decimal Pay { get; set; }
    }

    public record ListingResponse
    {
        public int? Id { get; set; }
        public DateTime StartDate { get; set; }
        [JsonConverter(typeof(TimeOnlyJsonConverter))]
        public TimeOnly EndTime { get; set; }
        public IEnumerable<GenreDto> Genres { get; set; } = [];
        public double Pay { get; set; }
    }

    public record ListingWithVenueDto(ListingDto Listing, VenueDto Venue);

    public record ListingApplicationDto(int Id, ArtistDto Artist, ListingDto Listing, ApplicationStatus Status);

    public record ArtistListingApplicationDto(int Id, ArtistDto Artist, ListingWithVenueDto ListingWithVenue, ApplicationStatus Status);
}
