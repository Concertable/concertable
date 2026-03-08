using Core.Entities;
using Core.Parameters;

namespace Application.Interfaces.Search;

public interface IVenueSearchSpecification
{
    IQueryable<Venue> Apply(IQueryable<Venue> query, SearchParams searchParams);
}
