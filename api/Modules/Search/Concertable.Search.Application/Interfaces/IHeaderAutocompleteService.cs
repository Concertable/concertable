

namespace Concertable.Search.Application.Interfaces;

internal interface IHeaderAutocompleteService
{
    Task<IEnumerable<AutocompleteDto>> GetAsync(string? searchTerm);
}
