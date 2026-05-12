namespace Concertable.Concert.Application.Workflow.Steps;

internal interface IPaidApplyStep : IConcertStep
{
    Task<ApplicationEntity> ExecuteAsync(int artistId, int opportunityId, string paymentMethodId);
}
