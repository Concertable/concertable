using Application.Interfaces.Search;
using Core.Entities;
using Core.Parameters;

namespace Infrastructure.Specifications;

public class ArtistSearchSpecification : IArtistSearchSpecification
{
    private readonly ISearchSpecification<Artist> searchSpecification;

    public ArtistSearchSpecification(ISearchSpecification<Artist> searchSpecification)
    {
        this.searchSpecification = searchSpecification;
    }

    public IQueryable<Artist> Apply(IQueryable<Artist> query, SearchParams searchParams)
    {
        if (searchParams.GenreIds?.Any() == true)
            query = query.Where(a => a.ArtistGenres.Any(ag => searchParams.GenreIds.Contains(ag.GenreId)));

        return searchSpecification.Apply(query, searchParams);
    }
}
