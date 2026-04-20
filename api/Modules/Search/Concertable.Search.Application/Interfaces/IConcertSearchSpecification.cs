using Concertable.Core.Entities;
using Concertable.Core.Parameters;

namespace Concertable.Search.Application.Interfaces;

public interface IConcertSearchSpecification
{
    IQueryable<ConcertEntity> Apply(IQueryable<ConcertEntity> query, SearchParams searchParams);
}
