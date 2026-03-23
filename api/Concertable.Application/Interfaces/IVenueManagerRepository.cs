using Core.Entities;
using Core.Interfaces;

namespace Application.Interfaces;

public interface IVenueManagerRepository : IRepository<VenueManagerEntity>
{
    Task<VenueManagerEntity?> GetByConcertIdAsync(int concertId);
    Task<VenueManagerEntity?> GetByApplicationIdAsync(int applicationId);
}
