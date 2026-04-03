using Concertable.Application.Requests;
using Concertable.Core.Entities;

namespace Concertable.Application.Mappers;

public static class OpportunityMappers
{
    public static OpportunityEntity ToEntity(this OpportunityRequest request) => new()
    {
        StartDate = request.StartDate,
        EndDate = request.EndDate,
        OpportunityGenres = request.GenreIds.Select(g => new OpportunityGenreEntity { GenreId = g }).ToList()
    };
}
