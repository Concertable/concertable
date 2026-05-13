namespace Concertable.Concert.Infrastructure.Services.Workflow.StateMachines;

internal class DeferredStateMachine : ConcertStateMachine
{
    private static readonly ConcertStage[] sequence =
    [
        ConcertStage.None,
        ConcertStage.Applied,
        ConcertStage.Verified,
        ConcertStage.Accepted,
        ConcertStage.Settled,
        ConcertStage.Finished
    ];

    protected override ConcertStage[] Sequence => sequence;
}
