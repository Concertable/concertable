using Application.Interfaces.Concert;
using Application.Serializers;
using Core.Enums;
using System.Text.Json.Serialization;

namespace Application.DTOs;

public record ConcertOpportunityDto
{
    public int? Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public IEnumerable<GenreDto> Genres { get; set; } = new List<GenreDto>();
}

public record OpportunityResponse
{
    public int? Id { get; set; }
    public DateTime StartDate { get; set; }
    [JsonConverter(typeof(TimeOnlyJsonConverter))]
    public TimeOnly EndTime { get; set; }
    public IEnumerable<GenreDto> Genres { get; set; } = [];
}

public record OpportunityWithVenueDto(ConcertOpportunityDto Opportunity, VenueDto Venue);

public record ConcertApplicationDto(int Id, ArtistDto Artist, ConcertOpportunityDto Opportunity, ApplicationStatus Status);

public record ArtistConcertApplicationDto(int Id, ArtistDto Artist, OpportunityWithVenueDto OpportunityWithVenue, ApplicationStatus Status);
