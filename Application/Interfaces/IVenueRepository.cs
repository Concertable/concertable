using Core.Entities;
using Core.Interfaces;

namespace Application.Interfaces;

public interface IVenueRepository : IRepository<VenueEntity>
{
    Task<VenueEntity?> GetByUserIdAsync(int id);
    Task<int?> GetIdByUserIdAsync(int userId);
}
