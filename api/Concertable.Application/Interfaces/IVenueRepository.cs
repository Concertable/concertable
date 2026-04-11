using Concertable.Application.DTOs;
using Concertable.Core.Entities;
using Concertable.Core.Interfaces;

namespace Concertable.Application.Interfaces;

public interface IVenueRepository : IRepository<VenueEntity>
{
    Task<VenueEntity?> GetByUserIdAsync(Guid id);
    Task<int?> GetIdByUserIdAsync(Guid userId);
    Task<VenueEntity?> GetFullByIdAsync(int id);
    Task<VenueSummaryDto?> GetSummaryAsync(int id);
    Task<VenueDto?> GetDtoByIdAsync(int id);
    Task<VenueDto?> GetDtoByUserIdAsync(Guid userId);
}
