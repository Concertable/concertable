using Core.Entities;
using Core.Parameters;

namespace Application.Interfaces.Search;

public interface IConcertSearchSpecification
{
    IQueryable<ConcertEntity> Apply(IQueryable<ConcertEntity> query, SearchParams searchParams);
}
