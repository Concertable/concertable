using Concertable.Search.Application.Interfaces;
using Concertable.Search.Domain.Models;
using Concertable.Search.Infrastructure.Data;
using Concertable.Search.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Search.Infrastructure.Repositories;

internal class HeaderAutocompleteRepository : IHeaderAutocompleteRepository
{
    private readonly ISearchDbContext context;
    private readonly ISearchSpecification<ArtistSearchModel> artistSearchSpecification;
    private readonly ISearchSpecification<VenueSearchModel> venueSearchSpecification;
    private readonly ISearchSpecification<ConcertSearchModel> concertSearchSpecification;

    public HeaderAutocompleteRepository(
        ISearchDbContext context,
        ISearchSpecification<ArtistSearchModel> artistSearchSpecification,
        ISearchSpecification<VenueSearchModel> venueSearchSpecification,
        ISearchSpecification<ConcertSearchModel> concertSearchSpecification)
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
