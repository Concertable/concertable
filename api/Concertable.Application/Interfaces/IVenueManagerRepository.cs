using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces;

public interface IVenueManagerRepository : IGuidRepository<VenueManagerEntity>
{
    Task<VenueManagerEntity?> GetByConcertIdAsync(int concertId);
    Task<VenueManagerEntity?> GetByApplicationIdAsync(int applicationId);
}
