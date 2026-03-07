using Application.Interfaces.Search;
using Core.Entities;
using Core.Parameters;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Specifications
{
    public class ArtistSearchSpecification : IArtistSearchSpecification
    {
        private readonly ISearchSpecification<Artist> searchSpecification;

        public ArtistSearchSpecification(ISearchSpecification<Artist> searchSpecification)
        {
            this.searchSpecification = searchSpecification;
        }

        public IQueryable<Artist> Apply(IQueryable<Artist> query, SearchParams searchParams)
        {
            query = query
                .Include(a => a.User)
                .Include(a => a.ArtistGenres).ThenInclude(ag => ag.Genre);

            if (searchParams.GenreIds?.Any() == true)
                query = query.Where(a => a.ArtistGenres.Any(ag => searchParams.GenreIds.Contains(ag.GenreId)));

            query = searchSpecification.Apply(query, searchParams);

            return searchParams.Sort?.ToLower() switch
            {
                "name_asc" => query.OrderBy(a => a.Name),
                "name_desc" => query.OrderByDescending(a => a.Name),
                _ => query.OrderBy(a => a.Id)
            };
        }
    }
}
