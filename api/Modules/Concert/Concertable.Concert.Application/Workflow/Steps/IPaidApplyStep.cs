namespace Concertable.Concert.Application.Workflow.Steps;

internal interface IPaidApplyStep : IConcertStep
{
    static ConcertStage IConcertStep.Stage => ConcertStage.Applied;
    Task<ApplicationEntity> ExecuteAsync(int artistId, int opportunityId, string paymentMethodId);
}
