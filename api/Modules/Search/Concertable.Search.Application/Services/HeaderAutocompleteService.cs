
using Concertable.Search.Application.Interfaces;

namespace Concertable.Search.Application.Services;

internal class HeaderAutocompleteService : IHeaderAutocompleteService
{
    private readonly IHeaderAutocompleteRepository headerAutocompleteRepository;

    public HeaderAutocompleteService(IHeaderAutocompleteRepository headerAutocompleteRepository)
    {
        this.headerAutocompleteRepository = headerAutocompleteRepository;
    }

    public async Task<IEnumerable<AutocompleteDto>> GetAsync(string? searchTerm) =>
        await headerAutocompleteRepository.GetAsync(searchTerm);
}
