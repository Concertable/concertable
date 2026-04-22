using Concertable.Data.Application;
using Concertable.Search.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Search.Infrastructure.Repositories;

internal class HeaderAutocompleteRepository : IHeaderAutocompleteRepository
{
    private readonly IReadDbContext context;
    private readonly ISearchSpecification<ArtistEntity> artistSearchSpecification;
    private readonly ISearchSpecification<VenueEntity> venueSearchSpecification;
    private readonly ISearchSpecification<ConcertEntity> concertSearchSpecification;

    public HeaderAutocompleteRepository(
        IReadDbContext context,
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
            .Apply(context.Artists, searchTerm)
            .ToAutocompleteDtos()
            .Take(20)
            .Concat(
                venueSearchSpecification
                    .Apply(context.Venues, searchTerm)
                    .ToAutocompleteDtos()
                    .Take(20))
            .Concat(
                concertSearchSpecification
                    .Apply(context.Concerts, searchTerm)
                    .ToAutocompleteDtos()
                    .Take(20))
            .OrderBy(r => r.Name)
            .Take(10)
            .ToListAsync();
}
