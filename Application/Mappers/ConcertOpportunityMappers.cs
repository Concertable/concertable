using Application.DTOs;
using Core.Entities;

namespace Application.Mappers;

public static class ConcertOpportunityMappers
{
    public static ConcertOpportunityDto ToDto(this ConcertOpportunity opportunity) => new()
    {
        Id = opportunity.Id,
        StartDate = opportunity.StartDate,
        EndDate = opportunity.EndDate,
        Pay = opportunity.Pay,
        Genres = opportunity.OpportunityGenres.Select(og => og.Genre.ToDto())
    };

    public static ConcertOpportunity ToEntity(this ConcertOpportunityDto dto) => new()
    {
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        Pay = dto.Pay,
        OpportunityGenres = dto.Genres.Select(g => new OpportunityGenre { GenreId = g.Id }).ToList()
    };

    public static OpportunityWithVenueDto ToWithVenueDto(this ConcertOpportunity opportunity) => new(
        opportunity.ToDto(),
        opportunity.Venue.ToDto()
    );

    public static IEnumerable<ConcertOpportunityDto> ToDtos(this IEnumerable<ConcertOpportunity> opportunities) =>
        opportunities.Select(o => o.ToDto());
}
