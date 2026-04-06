using Concertable.Application.DTOs;

namespace Concertable.Application.Interfaces.Search;

public interface IHeaderAutocompleteService
{
    Task<IEnumerable<AutocompleteDto>> GetAsync(string? searchTerm);
}
