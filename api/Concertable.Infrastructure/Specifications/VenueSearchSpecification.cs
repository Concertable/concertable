using Application.Interfaces.Search;
using Core.Entities;
using Core.Parameters;

namespace Infrastructure.Specifications;

public class VenueSearchSpecification : IVenueSearchSpecification
{
    private readonly ISearchSpecification<VenueEntity> searchSpecification;

    public VenueSearchSpecification(ISearchSpecification<VenueEntity> searchSpecification)
    {
        this.searchSpecification = searchSpecification;
    }

    public IQueryable<VenueEntity> Apply(IQueryable<VenueEntity> query, SearchParams searchParams) =>
        searchSpecification.Apply(query, searchParams);
}
