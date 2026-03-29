using Concertable.Core.Entities;
using Concertable.Core.Interfaces;

namespace Concertable.Application.Interfaces;

public interface IVenueRepository : IRepository<VenueEntity>
{
    Task<VenueEntity?> GetByUserIdAsync(Guid id);
    Task<int?> GetIdByUserIdAsync(Guid userId);
}
