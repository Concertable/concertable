using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Search;

namespace Concertable.Infrastructure.Services.Search;

public class HeaderAutocompleteService : IHeaderAutocompleteService
{
    private readonly IHeaderAutocompleteRepository headerAutocompleteRepository;

    public HeaderAutocompleteService(IHeaderAutocompleteRepository headerAutocompleteRepository)
    {
        this.headerAutocompleteRepository = headerAutocompleteRepository;
    }

    public async Task<IEnumerable<AutocompleteDto>> GetAsync(string? searchTerm) =>
        await headerAutocompleteRepository.GetAsync(searchTerm);
}
