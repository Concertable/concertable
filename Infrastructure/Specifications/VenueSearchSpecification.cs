using Application.Interfaces.Search;
using Core.Entities;
using Core.Parameters;

namespace Infrastructure.Specifications;

public class VenueSearchSpecification : IVenueSearchSpecification
{
    private readonly ISearchSpecification<Venue> searchSpecification;

    public VenueSearchSpecification(ISearchSpecification<Venue> searchSpecification)
    {
        this.searchSpecification = searchSpecification;
    }

    public IQueryable<Venue> Apply(IQueryable<Venue> query, SearchParams searchParams) =>
        searchSpecification.Apply(query, searchParams);
}
