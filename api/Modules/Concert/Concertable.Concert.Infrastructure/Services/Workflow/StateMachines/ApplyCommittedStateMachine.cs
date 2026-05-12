namespace Concertable.Concert.Infrastructure.Services.Workflow.StateMachines;

internal class ApplyCommittedStateMachine : ConcertStateMachine
{
    private static readonly ConcertStage[] sequence =
    [
        ConcertStage.None,
        ConcertStage.CheckedOut,
        ConcertStage.Applied,
        ConcertStage.Accepted,
        ConcertStage.Settled,
        ConcertStage.Finished
    ];

    public ApplyCommittedStateMachine(IConcertLifecycleRepository repository) : base(repository) { }

    protected override ConcertStage[] Sequence => sequence;
}
