using System.Linq.Expressions;
using Concertable.Application.DTOs;
using Concertable.Search.Domain.Models;

namespace Concertable.Search.Infrastructure.Mappers;

internal static class ArtistSearchGenreSelectors
{
    public static Expression<Func<ArtistSearchModel, IEnumerable<GenreDto>>> FromArtist =>
        a => a.ArtistGenres.Select(ag => new GenreDto(ag.Genre.Id, ag.Genre.Name));
}
