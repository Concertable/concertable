

namespace Concertable.Search.Application.Interfaces;

public interface IHeaderAutocompleteService
{
    Task<IEnumerable<AutocompleteDto>> GetAsync(string? searchTerm);
}
