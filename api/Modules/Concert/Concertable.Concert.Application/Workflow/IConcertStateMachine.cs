namespace Concertable.Concert.Application.Workflow;

internal interface IConcertStateMachine
{
    bool CanTransitionTo(ConcertStage target, ConcertStage current);
    ConcertStage NextStage(ConcertStage current);
}
