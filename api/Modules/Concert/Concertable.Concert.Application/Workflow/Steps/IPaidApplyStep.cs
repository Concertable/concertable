namespace Concertable.Concert.Application.Workflow.Steps;

internal interface IPaidApplyStep : IConcertStep
{
    static ConcertStage IConcertStep.Stage => ConcertStage.Applied;
    Task ExecuteAsync(ApplicationEntity app);
}
