using Concertable.Concert.Application.Workflow;
using Concertable.Concert.Application.Workflow.Steps;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Workflow.StateMachines;

internal abstract class ConcertStateMachine : IConcertStateMachine
{
    private readonly IConcertLifecycleRepository repository;

    protected ConcertStateMachine(IConcertLifecycleRepository repository)
    {
        this.repository = repository;
    }

    protected abstract ConcertStage[] Sequence { get; }

    public async Task GuardAsync<TStep>(int lifecycleId) where TStep : IConcertStep
    {
        var lifecycle = await repository.GetAsync(lifecycleId)
            ?? throw new NotFoundException("Concert lifecycle not found");

        var expected = PreviousStage<TStep>();
        if (lifecycle.Stage != expected)
            throw new ConflictException($"Cannot run {typeof(TStep).Name} from stage {lifecycle.Stage}");
    }

    public async Task AdvanceAsync<TStep>(int lifecycleId) where TStep : IConcertStep
    {
        var lifecycle = await repository.GetAsync(lifecycleId)
            ?? throw new NotFoundException("Concert lifecycle not found");

        lifecycle.AdvanceTo(TStep.Stage);
        await repository.SaveChangesAsync();
    }

    private ConcertStage PreviousStage<TStep>() where TStep : IConcertStep
    {
        var index = Array.IndexOf(Sequence, TStep.Stage);
        if (index < 0)
            throw new BadRequestException($"Stage {TStep.Stage} is not part of this contract's pipeline");
        if (index == 0)
            throw new BadRequestException($"Stage {TStep.Stage} has no predecessor");
        return Sequence[index - 1];
    }
}
