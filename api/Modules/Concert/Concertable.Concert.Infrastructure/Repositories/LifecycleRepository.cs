using Concertable.Concert.Application.Workflow;
using Concertable.Concert.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Concert.Infrastructure.Repositories;

internal class LifecycleRepository<TEntity> : ILifecycleRepository<TEntity>
    where TEntity : class, ILifecycleEntity
{
    private readonly ConcertDbContext context;

    public LifecycleRepository(ConcertDbContext context)
    {
        this.context = context;
    }

    public async Task<TEntity?> GetByIdAsync(int id)
        => await context.Set<TEntity>().FindAsync(id);

    public async Task SaveAsync(TEntity entity)
    {
        if (entity.Id == 0)
            await context.Set<TEntity>().AddAsync(entity);
        await context.SaveChangesAsync();
    }
}
