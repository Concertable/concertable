using Concertable.Concert.Domain;
using Concertable.Search.Application.Interfaces;
using Concertable.Search.Domain.Models;
using Concertable.Search.Infrastructure.Data;
using Concertable.Search.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Search.Infrastructure.Repositories;

internal class HeaderAutocompleteRepository : IHeaderAutocompleteRepository
{
    private readonly SearchDbContext context;
    private readonly ISearchSpecification<ArtistSearchModel> artistSearchSpecification;
    private readonly ISearchSpecification<VenueSearchModel> venueSearchSpecification;
    private readonly ISearchSpecification<ConcertEntity> concertSearchSpecification;

    public HeaderAutocompleteRepository(
        SearchDbContext context,
        ISearchSpecification<ArtistSearchModel> artistSearchSpecification,
        ISearchSpecification<VenueSearchModel> venueSearchSpecification,
        ISearchSpecification<ConcertEntity> concertSearchSpecification)
    {
        this.context = context;
        this.artistSearchSpecification = artistSearchSpecification;
        this.venueSearchSpecification = venueSearchSpecification;
        this.concertSearchSpecification = concertSearchSpecification;
    }

    public async Task<IEnumerable<AutocompleteDto>> GetAsync(string? searchTerm) =>
        await artistSearchSpecification
            .Apply(context.Artists.AsNoTracking(), searchTerm)
            .ToAutocompleteDtos()
            .Take(20)
            .Concat(
                venueSearchSpecification
                    .Apply(context.Venues.AsNoTracking(), searchTerm)
                    .ToAutocompleteDtos()
                    .Take(20))
            .Concat(
                concertSearchSpecification
                    .Apply(context.Concerts.AsNoTracking(), searchTerm)
                    .ToAutocompleteDtos()
                    .Take(20))
            .OrderBy(r => r.Name)
            .Take(10)
            .ToListAsync();
}
