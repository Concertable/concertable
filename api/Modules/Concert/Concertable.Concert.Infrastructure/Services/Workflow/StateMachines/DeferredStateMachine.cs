namespace Concertable.Concert.Infrastructure.Services.Workflow.StateMachines;

internal class DeferredStateMachine : ConcertStateMachine
{
    private static readonly ConcertStage[] sequence =
    [
        ConcertStage.None,
        ConcertStage.Applied,
        ConcertStage.CheckedOut,
        ConcertStage.Verified,
        ConcertStage.Accepted,
        ConcertStage.Settled,
        ConcertStage.Finished
    ];

    public DeferredStateMachine(IConcertLifecycleRepository repository) : base(repository) { }

    protected override ConcertStage[] Sequence => sequence;
}
