using Concertable.Application.Interfaces.Search;
using Concertable.Core.Entities;
using Concertable.Core.Parameters;

namespace Concertable.Infrastructure.Specifications;

public class ArtistSearchSpecification : IArtistSearchSpecification
{
    private readonly ISearchSpecification<ArtistEntity> searchSpecification;

    public ArtistSearchSpecification(ISearchSpecification<ArtistEntity> searchSpecification)
    {
        this.searchSpecification = searchSpecification;
    }

    public IQueryable<ArtistEntity> Apply(IQueryable<ArtistEntity> query, SearchParams searchParams)
    {
        if (searchParams.GenreIds?.Any() == true)
            query = query.Where(a => a.ArtistGenres.Any(ag => searchParams.GenreIds.Contains(ag.GenreId)));

        return searchSpecification.Apply(query, searchParams.SearchTerm);
    }
}
