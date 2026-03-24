using Core.Entities;
using Core.Interfaces;

namespace Application.Interfaces;

public interface IArtistRepository : IRepository<ArtistEntity>
{
    Task<int?> GetIdByUserIdAsync(Guid id);
    Task<ArtistEntity?> GetByUserIdAsync(Guid id);
    Task<ArtistEntity?> GetDetailsByIdAsync(int id);
}
