using Concertable.Application.DTOs;

namespace Concertable.Application.Interfaces.Search;

public interface IHeaderAutocompleteRepository
{
    Task<IEnumerable<AutocompleteDto>> GetAsync(string? searchTerm);
}
