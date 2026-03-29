using Concertable.Core.Entities;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces.Search;

public interface IConcertSearchSpecification
{
    IQueryable<ConcertEntity> Apply(IQueryable<ConcertEntity> query, SearchParams searchParams);
}
