using Concertable.Concert.Application.Workflow;
using Concertable.Concert.Application.Workflow.Steps;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Workflow.StateMachines;

internal class DeferredStateMachine : IConcertStateMachine
{
    private static readonly IReadOnlyDictionary<Type, (ConcertStage From, ConcertStage To)> Transitions
        = new Dictionary<Type, (ConcertStage, ConcertStage)>
        {
            [typeof(ISimpleApplyStep)]    = (ConcertStage.None,       ConcertStage.Applied),
            [typeof(IAcceptCheckoutStep)] = (ConcertStage.Applied,    ConcertStage.CheckedOut),
            [typeof(IVerifyStep)]         = (ConcertStage.CheckedOut, ConcertStage.Verified),
            [typeof(IPaidAcceptStep)]     = (ConcertStage.Verified,   ConcertStage.Accepted),
            [typeof(ISettleStep)]         = (ConcertStage.Accepted,   ConcertStage.Settled),
            [typeof(IFinishStep)]         = (ConcertStage.Settled,    ConcertStage.Finished),
        };

    private readonly IConcertLifecycleRepository repository;

    public DeferredStateMachine(IConcertLifecycleRepository repository)
    {
        this.repository = repository;
    }

    public async Task GuardAsync<TStep>(int lifecycleId) where TStep : IConcertStep
    {
        var lifecycle = await repository.GetAsync(lifecycleId)
            ?? throw new NotFoundException("Concert lifecycle not found");

        if (!Transitions.TryGetValue(typeof(TStep), out var transition))
            throw new BadRequestException($"This contract does not support {typeof(TStep).Name}");

        if (lifecycle.Stage != transition.From)
            throw new ConflictException($"Cannot run {typeof(TStep).Name} from stage {lifecycle.Stage}");
    }

    public async Task AdvanceAsync<TStep>(int lifecycleId) where TStep : IConcertStep
    {
        var lifecycle = await repository.GetAsync(lifecycleId)
            ?? throw new NotFoundException("Concert lifecycle not found");

        var (_, to) = Transitions[typeof(TStep)];
        lifecycle.AdvanceTo(to);
        await repository.SaveChangesAsync();
    }
}
