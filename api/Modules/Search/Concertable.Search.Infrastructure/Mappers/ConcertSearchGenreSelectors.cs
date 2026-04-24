using System.Linq.Expressions;
using Concertable.Application.DTOs;
using Concertable.Search.Domain.Models;

namespace Concertable.Search.Infrastructure.Mappers;

internal static class ConcertSearchGenreSelectors
{
    public static Expression<Func<ConcertSearchModel, IEnumerable<GenreDto>>> FromConcert =>
        c => c.ConcertGenres.Select(cg => new GenreDto(cg.Genre.Id, cg.Genre.Name));
}
