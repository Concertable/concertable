using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces;

public interface IConcertBookingRepository : IRepository<ConcertBookingEntity>
{
    Task<ConcertBookingEntity?> GetByApplicationIdAsync(int applicationId);
    Task<ConcertBookingEntity?> GetByConcertIdAsync(int concertId);
}
