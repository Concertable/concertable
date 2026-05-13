namespace Concertable.Concert.Application.Workflow;

internal interface IStepExecutor<TEntity> where TEntity : ILifecycleEntity
{
    Task ExecuteAsync(
        int entityId,
        ConcertStage targetStage,
        StepDispatch<TEntity> dispatch);

    Task ExecuteAsync<TInput>(
        TEntity entity,
        ConcertStage targetStage,
        StepDispatch<TEntity, TInput> dispatch,
        TInput input);

    Task<TResult> ExecuteAsync<TInput, TResult>(
        TEntity entity,
        ConcertStage targetStage,
        StepDispatch<TEntity, TInput, TResult> dispatch,
        TInput input);

    Task ExecuteAsync<TInput>(
        int entityId,
        ConcertStage targetStage,
        StepDispatch<TEntity, TInput> dispatch,
        TInput input);

    Task<TResult> ExecuteAsync<TInput, TResult>(
        int entityId,
        ConcertStage targetStage,
        StepDispatch<TEntity, TInput, TResult> dispatch,
        TInput input);
}
