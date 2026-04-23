using Concertable.Core.Parameters;

namespace Concertable.Search.Application.Interfaces;

internal interface IConcertSearchSpecification
{
    IQueryable<ConcertEntity> Apply(IQueryable<ConcertEntity> query, SearchParams searchParams);
}
