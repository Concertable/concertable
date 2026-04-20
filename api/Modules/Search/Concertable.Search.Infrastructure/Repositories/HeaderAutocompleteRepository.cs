using Concertable.Core.Entities;
using Concertable.Data.Infrastructure.Data;
using Concertable.Search.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Search.Infrastructure.Repositories;

internal class HeaderAutocompleteRepository : IHeaderAutocompleteRepository
{
    private readonly ReadDbContext context;
    private readonly ISearchSpecification<ArtistEntity> artistSearchSpecification;
    private readonly ISearchSpecification<VenueEntity> venueSearchSpecification;
    private readonly ISearchSpecification<ConcertEntity> concertSearchSpecification;

    public HeaderAutocompleteRepository(
        ReadDbContext context,
        ISearchSpecification<ArtistEntity> artistSearchSpecification,
        ISearchSpecification<VenueEntity> venueSearchSpecification,
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
