

namespace Concertable.Search.Application.Interfaces;

internal interface IHeaderAutocompleteRepository
{
    Task<IEnumerable<AutocompleteDto>> GetAsync(string? searchTerm);
}
