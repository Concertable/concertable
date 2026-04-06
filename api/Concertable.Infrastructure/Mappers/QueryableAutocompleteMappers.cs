using Concertable.Application.DTOs;
using Concertable.Core.Entities;

namespace Concertable.Infrastructure.Mappers;

public static class QueryableAutocompleteMappers
{
    public static IQueryable<AutocompleteDto> ToAutocompleteDtos(this IQueryable<ArtistEntity> query) =>
        query.Select(a => new AutocompleteDto { Id = a.Id, Name = a.Name, Type = "artist" });

    public static IQueryable<AutocompleteDto> ToAutocompleteDtos(this IQueryable<VenueEntity> query) =>
        query.Select(v => new AutocompleteDto { Id = v.Id, Name = v.Name, Type = "venue" });

    public static IQueryable<AutocompleteDto> ToAutocompleteDtos(this IQueryable<ConcertEntity> query) =>
        query.Select(c => new AutocompleteDto { Id = c.Id, Name = c.Name, Type = "concert" });
}
