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
        public IEnumerable<GenreDto> Genres { get; set; }
        public double Pay { get; set; }
    }

    public record ListingWithVenueDto
    {
        public ListingDto Listing { get; set; }
        public VenueDto Venue { get; set; }
    }

    public record ListingApplicationDto
    {
        public int Id { get; set; }
        public ArtistDto Artist { get; set; }
        public ListingDto Listing { get; set; }
        public ApplicationStatus Status { get; set; }
    }

    public record ArtistListingApplicationDto
    {
        public int Id { get; set; }
        public ArtistDto Artist { get; set; }
        public ListingWithVenueDto ListingWithVenue { get; set; }
        public ApplicationStatus Status { get; set; }
    }
}
