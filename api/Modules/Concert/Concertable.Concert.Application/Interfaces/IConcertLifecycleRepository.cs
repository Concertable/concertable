namespace Concertable.Concert.Application.Interfaces;

internal interface IConcertLifecycleRepository
{
    Task<ConcertLifecycleEntity?> GetByIdAsync(int id);
    Task<ConcertLifecycleEntity> AddAsync(ConcertLifecycleEntity entity);
    Task SaveChangesAsync();

    Task<int?> GetIdByOpportunityIdAndArtistIdAsync(int opportunityId, int artistId);
    Task<int?> GetIdByApplicationIdAsync(int applicationId);
    Task<int?> GetIdByBookingIdAsync(int bookingId);
    Task<int?> GetIdByConcertIdAsync(int concertId);
}
