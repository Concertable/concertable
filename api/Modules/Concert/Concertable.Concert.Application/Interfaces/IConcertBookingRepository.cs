namespace Concertable.Concert.Application.Interfaces;

internal interface IConcertBookingRepository : IIdRepository<ConcertBookingEntity>
{
    Task<ConcertBookingEntity?> GetByApplicationIdAsync(int applicationId);
    Task<ConcertBookingEntity?> GetByConcertIdAsync(int concertId);
    Task<int?> GetContractIdByBookingIdAsync(int bookingId);
}
