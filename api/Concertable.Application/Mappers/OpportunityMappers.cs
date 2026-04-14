using Concertable.Application.Requests;
using Concertable.Core.Entities;
using Concertable.Core.ValueObjects;

namespace Concertable.Application.Mappers;

public static class OpportunityMappers
{
    public static OpportunityEntity ToEntity(this OpportunityRequest request) => new()
    {
        Period = new DateRange(request.StartDate, request.EndDate),
        OpportunityGenres = request.GenreIds.Select(g => new OpportunityGenreEntity { GenreId = g }).ToList()
    };
}
