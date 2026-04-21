using Concertable.Application.Interfaces;
using Concertable.Artist.Application.DTOs;

namespace Concertable.Artist.Application.Interfaces;

public interface IArtistRepository : IIdRepository<ArtistEntity>
{
    Task<int?> GetIdByUserIdAsync(Guid id);
    Task<ArtistEntity?> GetByUserIdAsync(Guid id);
    Task<ArtistEntity?> GetFullByIdAsync(int id);
    Task<ArtistSummaryDto?> GetSummaryAsync(int id);
    Task<ArtistDto?> GetDtoByIdAsync(int id);
    Task<ArtistDto?> GetDtoByUserIdAsync(Guid userId);
}
