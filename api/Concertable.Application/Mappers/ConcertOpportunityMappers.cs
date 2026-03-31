using Concertable.Application.Requests;
using Concertable.Core.Entities;

namespace Concertable.Application.Mappers;

public static class ConcertOpportunityMappers
{
    public static ConcertOpportunityEntity ToEntity(this ConcertOpportunityRequest request) => new()
    {
        StartDate = request.StartDate,
        EndDate = request.EndDate,
        OpportunityGenres = request.GenreIds.Select(g => new OpportunityGenreEntity { GenreId = g }).ToList()
    };
}
