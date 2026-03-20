using Application.DTOs;
using Application.Interfaces.Concert;
using Core.Entities;

namespace Application.Mappers;

public class ConcertOpportunityMapper : IConcertOpportunityMapper
{
    public ConcertOpportunityDto ToDto(ConcertOpportunityEntity opportunity) => new()
    {
        Id = opportunity.Id,
        StartDate = opportunity.StartDate,
        EndDate = opportunity.EndDate,
        Genres = opportunity.OpportunityGenres.Select(og => og.Genre.ToDto())
    };

    public OpportunityWithVenueDto ToWithVenueDto(ConcertOpportunityEntity opportunity) => new(
        ToDto(opportunity),
        opportunity.Venue.ToDto()
    );

    public IEnumerable<ConcertOpportunityDto> ToDtos(IEnumerable<ConcertOpportunityEntity> opportunities) =>
        opportunities.Select(ToDto);

    public ConcertOpportunityEntity ToEntity(ConcertOpportunityDto dto) => new()
    {
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        OpportunityGenres = dto.Genres.Select(g => new OpportunityGenreEntity { GenreId = g.Id }).ToList()
    };
}
