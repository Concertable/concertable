using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Serializers;
using Concertable.Core.Enums;
using System.Text.Json.Serialization;

namespace Concertable.Application.DTOs;

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

