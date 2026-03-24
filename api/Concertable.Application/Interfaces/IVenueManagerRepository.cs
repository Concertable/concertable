using Core.Entities;

namespace Application.Interfaces;

public interface IVenueManagerRepository : IGuidRepository<VenueManagerEntity>
{
    Task<VenueManagerEntity?> GetByConcertIdAsync(int concertId);
    Task<VenueManagerEntity?> GetByApplicationIdAsync(int applicationId);
}
