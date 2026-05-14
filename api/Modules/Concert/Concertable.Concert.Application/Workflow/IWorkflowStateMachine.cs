namespace Concertable.Concert.Application.Workflow;

internal interface IWorkflowStateMachine<TEntity> where TEntity : ILifecycleEntity
{
    Task TransitionAsync(TEntity entity, ConcertStage target);

    Task TransitionAsync(int entityId, ConcertStage target, Func<TEntity, Task> execute);
}
