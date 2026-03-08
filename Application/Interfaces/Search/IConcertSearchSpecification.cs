using Core.Entities;
using Core.Parameters;

namespace Application.Interfaces.Search;

public interface IConcertSearchSpecification
{
    IQueryable<Concert> Apply(IQueryable<Concert> query, SearchParams searchParams);
}
