using Application.DTOs;
using Core.Entities;

namespace Application.Mappers;

public static class ConcertOpportunityMappers
{
    public static ConcertOpportunityDto ToDto(this ConcertOpportunityEntity opportunity) => new()
    {
        Id = opportunity.Id,
        StartDate = opportunity.StartDate,
        EndDate = opportunity.EndDate,
        Genres = opportunity.OpportunityGenres.Select(og => og.Genre.ToDto())
    };

    public static ConcertOpportunityEntity ToEntity(this ConcertOpportunityDto dto) => new()
    {
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        OpportunityGenres = dto.Genres.Select(g => new OpportunityGenreEntity { GenreId = g.Id }).ToList()
    };

    public static OpportunityWithVenueDto ToWithVenueDto(this ConcertOpportunityEntity opportunity) => new(
        opportunity.ToDto(),
        opportunity.Venue.ToDto()
    );

    public static IEnumerable<ConcertOpportunityDto> ToDtos(this IEnumerable<ConcertOpportunityEntity> opportunities) =>
        opportunities.Select(o => o.ToDto());
}
