using Concertable.Application.DTOs;
using Concertable.Core.Entities;
using Concertable.Core.Interfaces;

namespace Concertable.Application.Interfaces;

public interface IArtistRepository : IRepository<ArtistEntity>
{
    Task<int?> GetIdByUserIdAsync(Guid id);
    Task<ArtistEntity?> GetByUserIdAsync(Guid id);
    Task<ArtistDto?> GetDetailsByIdAsync(int id);
    Task<ArtistDto?> GetDetailsByUserIdAsync(Guid userId);
}
