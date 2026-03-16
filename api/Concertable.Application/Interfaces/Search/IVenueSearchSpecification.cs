using Core.Entities;
using Core.Parameters;

namespace Application.Interfaces.Search;

public interface IVenueSearchSpecification
{
    IQueryable<VenueEntity> Apply(IQueryable<VenueEntity> query, SearchParams searchParams);
}
