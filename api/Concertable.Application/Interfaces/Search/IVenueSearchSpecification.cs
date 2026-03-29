using Concertable.Core.Entities;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces.Search;

public interface IVenueSearchSpecification
{
    IQueryable<VenueEntity> Apply(IQueryable<VenueEntity> query, SearchParams searchParams);
}
