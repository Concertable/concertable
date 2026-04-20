using Concertable.Core.Entities;
using Concertable.Core.Parameters;

namespace Concertable.Search.Application.Interfaces;

public interface IVenueSearchSpecification
{
    IQueryable<VenueEntity> Apply(IQueryable<VenueEntity> query, SearchParams searchParams);
}
