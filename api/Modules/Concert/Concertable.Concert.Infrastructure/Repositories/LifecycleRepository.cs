using Concertable.Concert.Application.Workflow;
using Concertable.Concert.Infrastructure.Data;

namespace Concertable.Concert.Infrastructure.Repositories;

internal class LifecycleRepository<TEntity> : Repository<TEntity>, ILifecycleRepository<TEntity>
    where TEntity : class, ILifecycleEntity
{
    public LifecycleRepository(ConcertDbContext context) : base(context) { }

    public async Task SaveAsync(TEntity entity)
    {
        if (entity.Id == 0)
            await context.Set<TEntity>().AddAsync(entity);
        await context.SaveChangesAsync();
    }
}
