namespace Concertable.Concert.Application.Workflow;

internal delegate Task<TEntity> CreateStep<TEntity>() where TEntity : ILifecycleEntity;
internal delegate Task ExecuteStep<TEntity>(TEntity entity) where TEntity : ILifecycleEntity;

internal interface IWorkflowStateMachine<TEntity> where TEntity : ILifecycleEntity
{
    Task<TEntity> TransitionAsync(ConcertStage target, CreateStep<TEntity> create);

    Task TransitionAsync(int entityId, ConcertStage target, ExecuteStep<TEntity> execute);
}
