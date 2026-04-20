using Concertable.Search.Contracts;

using Concertable.Search.Application.Interfaces;

namespace Concertable.Search.Application;

public class AutocompleteModule : IAutocompleteModule
{
    private readonly IHeaderAutocompleteService autocompleteService;

    public AutocompleteModule(IHeaderAutocompleteService autocompleteService)
    {
        this.autocompleteService = autocompleteService;
    }

    public Task<IEnumerable<AutocompleteDto>> GetAsync(string? searchTerm) =>
        autocompleteService.GetAsync(searchTerm);
}
