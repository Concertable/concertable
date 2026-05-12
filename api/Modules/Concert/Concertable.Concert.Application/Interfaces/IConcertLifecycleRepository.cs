namespace Concertable.Concert.Application.Interfaces;

internal interface IConcertLifecycleRepository
{
    Task<ConcertLifecycleEntity?> GetByIdAsync(int id);
    Task<ConcertLifecycleEntity> AddAsync(ConcertLifecycleEntity entity);
    Task SaveChangesAsync();
}
