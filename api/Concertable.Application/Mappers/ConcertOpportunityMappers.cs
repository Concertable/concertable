using Application.DTOs;
using Application.Requests;
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

    public static OpportunityWithVenueDto ToWithVenueDto(this ConcertOpportunityEntity opportunity) =>
        new(opportunity.ToDto(), opportunity.Venue.ToDto());

    public static IEnumerable<ConcertOpportunityDto> ToDtos(this IEnumerable<ConcertOpportunityEntity> opportunities) =>
        opportunities.Select(o => o.ToDto());

    public static ConcertOpportunityEntity ToEntity(this ConcertOpportunityRequest request) => new()
    {
        StartDate = request.StartDate,
        EndDate = request.EndDate,
        OpportunityGenres = request.GenreIds.Select(g => new OpportunityGenreEntity { GenreId = g }).ToList()
    };
}
