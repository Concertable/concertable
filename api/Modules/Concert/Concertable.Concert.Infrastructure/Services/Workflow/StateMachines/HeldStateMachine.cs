namespace Concertable.Concert.Infrastructure.Services.Workflow.StateMachines;

internal class HeldStateMachine : ConcertStateMachine
{
    private static readonly ConcertStage[] sequence =
    [
        ConcertStage.None,
        ConcertStage.Applied,
        ConcertStage.CheckedOut,
        ConcertStage.Accepted,
        ConcertStage.Settled,
        ConcertStage.Finished
    ];

    public HeldStateMachine(IConcertLifecycleRepository repository) : base(repository) { }

    protected override ConcertStage[] Sequence => sequence;
}
