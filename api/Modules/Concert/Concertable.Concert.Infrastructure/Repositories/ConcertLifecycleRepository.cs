using Concertable.Concert.Infrastructure.Data;

namespace Concertable.Concert.Infrastructure.Repositories;

internal class ConcertLifecycleRepository : Repository<ConcertLifecycleEntity>, IConcertLifecycleRepository
{
    public ConcertLifecycleRepository(ConcertDbContext context) : base(context) { }
}
