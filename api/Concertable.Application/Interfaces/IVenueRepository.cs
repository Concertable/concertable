using Core.Entities;
using Core.Interfaces;

namespace Application.Interfaces;

public interface IVenueRepository : IRepository<VenueEntity>
{
    Task<VenueEntity?> GetByUserIdAsync(Guid id);
    Task<int?> GetIdByUserIdAsync(Guid userId);
}
