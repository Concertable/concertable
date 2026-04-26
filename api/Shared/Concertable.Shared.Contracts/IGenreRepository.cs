using Concertable.Shared;

namespace Concertable.Application.Interfaces;

public interface IGenreRepository : IRepository<GenreEntity>
{
    new Task<IEnumerable<GenreEntity>> GetAllAsync();
    Task<IEnumerable<GenreEntity>> GetByIdsAsync(IEnumerable<int> ids);
}
