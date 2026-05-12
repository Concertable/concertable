namespace Concertable.Concert.Application.Interfaces;

internal interface IConcertLifecycleRepository
{
    Task<ConcertLifecycleEntity?> GetAsync(int id);
    Task AddAsync(ConcertLifecycleEntity entity);
    Task SaveChangesAsync();
}
