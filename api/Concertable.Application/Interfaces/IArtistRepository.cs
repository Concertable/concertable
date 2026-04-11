using Concertable.Application.DTOs;
using Concertable.Core.Entities;
using Concertable.Core.Interfaces;

namespace Concertable.Application.Interfaces;

public interface IArtistRepository : IRepository<ArtistEntity>
{
    Task<int?> GetIdByUserIdAsync(Guid id);
    Task<ArtistEntity?> GetByUserIdAsync(Guid id);
    Task<ArtistEntity?> GetAggregateByIdAsync(int id);
    Task<ArtistSummaryDto?> GetSummaryAsync(int id);
    Task<ArtistDto?> GetDtoByIdAsync(int id);
    Task<ArtistDto?> GetDtoByUserIdAsync(Guid userId);
}
