using Concertable.Shared;

namespace Concertable.Application.Interfaces;

public interface IGenreService
{
    Task<IEnumerable<GenreDto>> GetAllAsync();
}
