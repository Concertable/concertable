using Concertable.Core.Parameters;
using Concertable.Search.Application.Interfaces;

namespace Concertable.Search.Infrastructure.Specifications;

internal class VenueSearchSpecification : IVenueSearchSpecification
{
    private readonly ISearchSpecification<VenueEntity> searchSpecification;

    public VenueSearchSpecification(ISearchSpecification<VenueEntity> searchSpecification)
    {
        this.searchSpecification = searchSpecification;
    }

    public IQueryable<VenueEntity> Apply(IQueryable<VenueEntity> query, SearchParams searchParams) =>
        searchSpecification.Apply(query, searchParams.SearchTerm);
}
