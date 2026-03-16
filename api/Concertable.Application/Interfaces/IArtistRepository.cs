using Core.Entities;
using Core.Interfaces;

namespace Application.Interfaces;

public interface IArtistRepository : IRepository<ArtistEntity>
{
    Task<int?> GetIdByUserIdAsync(int id);
    Task<ArtistEntity?> GetByUserIdAsync(int id);
    Task<ArtistEntity?> GetDetailsByIdAsync(int id);
}
