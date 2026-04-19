namespace Concertable.Search.Contracts;

public interface IAutocompleteModule
{
    Task<IEnumerable<AutocompleteDto>> GetAsync(string? searchTerm);
}
