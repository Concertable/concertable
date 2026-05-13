namespace Concertable.Concert.Application.Workflow;

internal interface ILifecycleRepository<TEntity> where TEntity : ILifecycleEntity
{
    Task<TEntity?> GetByIdAsync(int id);
    Task SaveAsync(TEntity entity);
}
