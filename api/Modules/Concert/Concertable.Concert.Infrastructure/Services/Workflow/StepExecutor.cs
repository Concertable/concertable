using Concertable.Concert.Application.Workflow;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class StepExecutor<TEntity> : IStepExecutor<TEntity> where TEntity : ILifecycleEntity
{
    private readonly ILifecycleRepository<TEntity> repository;
    private readonly IConcertStateMachineFactory stateMachines;

    public StepExecutor(
        ILifecycleRepository<TEntity> repository,
        IConcertStateMachineFactory stateMachines)
    {
        this.repository = repository;
        this.stateMachines = stateMachines;
    }

    public async Task ExecuteAsync(
        int entityId,
        ConcertStage targetStage,
        StepDispatch<TEntity> dispatch)
    {
        var entity = await LoadAsync(entityId);
        Guard(entity, targetStage);
        await dispatch(entity);
        entity.AdvanceStage(targetStage);
        await repository.SaveAsync(entity);
    }

    public async Task ExecuteAsync<TInput>(
        TEntity entity,
        ConcertStage targetStage,
        StepDispatch<TEntity, TInput> dispatch,
        TInput input)
    {
        Guard(entity, targetStage);
        await dispatch(entity, input);
        entity.AdvanceStage(targetStage);
        await repository.SaveAsync(entity);
    }

    public async Task<TResult> ExecuteAsync<TInput, TResult>(
        TEntity entity,
        ConcertStage targetStage,
        StepDispatch<TEntity, TInput, TResult> dispatch,
        TInput input)
    {
        Guard(entity, targetStage);
        var result = await dispatch(entity, input);
        entity.AdvanceStage(targetStage);
        await repository.SaveAsync(entity);
        return result;
    }

    public async Task ExecuteAsync<TInput>(
        int entityId,
        ConcertStage targetStage,
        StepDispatch<TEntity, TInput> dispatch,
        TInput input)
    {
        var entity = await LoadAsync(entityId);
        await ExecuteAsync(entity, targetStage, dispatch, input);
    }

    public async Task<TResult> ExecuteAsync<TInput, TResult>(
        int entityId,
        ConcertStage targetStage,
        StepDispatch<TEntity, TInput, TResult> dispatch,
        TInput input)
    {
        var entity = await LoadAsync(entityId);
        return await ExecuteAsync(entity, targetStage, dispatch, input);
    }

    private void Guard(TEntity entity, ConcertStage targetStage)
    {
        var stateMachine = stateMachines.Create(entity.ContractType);
        if (!stateMachine.CanTransitionTo(targetStage, entity.CurrentStage))
            throw new ConflictException($"Cannot transition from {entity.CurrentStage} to {targetStage}");
    }

    private async Task<TEntity> LoadAsync(int entityId)
        => await repository.GetByIdAsync(entityId)
            ?? throw new NotFoundException($"{typeof(TEntity).Name} {entityId} not found");
}
