namespace Concertable.Concert.Application.Workflow;

internal delegate Task StepDispatch<TEntity>(
    TEntity entity) where TEntity : ILifecycleEntity;

internal delegate Task StepDispatch<TEntity, TInput>(
    TEntity entity,
    TInput input) where TEntity : ILifecycleEntity;

internal delegate Task<TResult> StepDispatch<TEntity, TInput, TResult>(
    TEntity entity,
    TInput input) where TEntity : ILifecycleEntity;
