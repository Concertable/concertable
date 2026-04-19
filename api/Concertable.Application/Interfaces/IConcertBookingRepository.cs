using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces;

public interface IConcertBookingRepository
{
    Task AddAsync(ConcertBookingEntity booking);
    Task<ConcertBookingEntity?> GetByApplicationIdAsync(int applicationId);
    Task<ConcertBookingEntity?> GetByConcertIdAsync(int concertId);
    Task SaveChangesAsync();
}
