using Core.Entities;
using Core.Interfaces;

namespace Application.Interfaces;

public interface IArtistRepository : IRepository<Artist>
{
    Task<int?> GetIdByUserIdAsync(int id);
    Task<Artist?> GetByUserIdAsync(int id);
    new Task<Artist?> GetByIdAsync(int id);
}
