using Concertable.Search.Application.Interfaces;

namespace Concertable.Search.Application.Services;

internal class AllAutocompleteService : IAutocompleteService
{
    private readonly IArtistAutocompleteRepository artistAutocompleteRepository;
    private readonly IVenueAutocompleteRepository venueAutocompleteRepository;
    private readonly IConcertAutocompleteRepository concertAutocompleteRepository;

    public AllAutocompleteService(
        IArtistAutocompleteRepository artistAutocompleteRepository,
        IVenueAutocompleteRepository venueAutocompleteRepository,
        IConcertAutocompleteRepository concertAutocompleteRepository)
    {
        this.artistAutocompleteRepository = artistAutocompleteRepository;
        this.venueAutocompleteRepository = venueAutocompleteRepository;
        this.concertAutocompleteRepository = concertAutocompleteRepository;
    }

    public async Task<IEnumerable<AutocompleteDto>> GetAsync(string? searchTerm)
    {
        var artists = await artistAutocompleteRepository.GetAsync(searchTerm);
        var venues = await venueAutocompleteRepository.GetAsync(searchTerm);
        var concerts = await concertAutocompleteRepository.GetAsync(searchTerm);

        return artists
            .Concat(venues)
            .Concat(concerts)
            .OrderBy(r => r.Name)
            .Take(10);
    }
}
