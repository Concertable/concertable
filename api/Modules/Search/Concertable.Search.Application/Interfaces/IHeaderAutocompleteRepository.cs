

namespace Concertable.Search.Application.Interfaces;

public interface IHeaderAutocompleteRepository
{
    Task<IEnumerable<AutocompleteDto>> GetAsync(string? searchTerm);
}
