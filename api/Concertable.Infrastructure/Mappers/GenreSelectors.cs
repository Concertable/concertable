using System.Linq.Expressions;
using Concertable.Application.DTOs;
using Concertable.Core.Entities;
using LinqKit;

namespace Concertable.Infrastructure.Mappers;

public static class GenreSelectors
{
    public static Expression<Func<ArtistEntity, IEnumerable<GenreDto>>> FromArtist =>
        a => a.ArtistGenres.Select(ag => new GenreDto(ag.Genre.Id, ag.Genre.Name));

    public static Expression<Func<ArtistReadModel, IEnumerable<GenreDto>>> FromArtistReadModel =>
        a => a.Genres.Select(g => new GenreDto(g.Genre.Id, g.Genre.Name));

    public static Expression<Func<ConcertEntity, IEnumerable<GenreDto>>> FromConcert =>
        c => c.ConcertGenres.Select(cg => new GenreDto(cg.Genre.Id, cg.Genre.Name));
}
