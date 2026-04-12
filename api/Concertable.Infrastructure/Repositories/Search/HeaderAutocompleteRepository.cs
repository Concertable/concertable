using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Search;
using Concertable.Core.Entities;
using Concertable.Infrastructure.Data;
using Concertable.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories.Search;

public class HeaderAutocompleteRepository : IHeaderAutocompleteRepository
{
    private readonly ApplicationDbContext context;
    private readonly ISearchSpecification<ArtistEntity> artistSearchSpecification;
    private readonly ISearchSpecification<VenueEntity> venueSearchSpecification;
    private readonly ISearchSpecification<ConcertEntity> concertSearchSpecification;

    public HeaderAutocompleteRepository(
        ApplicationDbContext context,
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
