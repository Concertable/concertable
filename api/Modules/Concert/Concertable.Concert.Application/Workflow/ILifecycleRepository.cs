using Concertable.Application.Interfaces;

namespace Concertable.Concert.Application.Workflow;

internal interface ILifecycleRepository<TEntity> : IIdRepository<TEntity>
    where TEntity : class, ILifecycleEntity
{
    Task SaveAsync(TEntity entity);
}
