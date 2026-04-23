using Concertable.Core.Parameters;
using Concertable.Search.Application.Interfaces;

namespace Concertable.Search.Infrastructure.Specifications;

internal class ArtistSearchSpecification : IArtistSearchSpecification
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
