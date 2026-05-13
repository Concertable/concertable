namespace Concertable.Concert.Infrastructure.Services.Workflow.StateMachines;

internal class HeldStateMachine : ConcertStateMachine
{
    private static readonly ConcertStage[] sequence =
    [
        ConcertStage.None,
        ConcertStage.Applied,
        ConcertStage.Accepted,
        ConcertStage.Settled,
        ConcertStage.Finished
    ];

    protected override ConcertStage[] Sequence => sequence;
}
