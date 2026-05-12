using Concertable.Concert.Application.Workflow.Steps;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Steps;

internal class VenueHirePaidApplyStep : IPaidApplyStep
{
    public Task<ApplicationEntity> ExecuteAsync(int artistId, int opportunityId, string paymentMethodId) =>
        Task.FromResult<ApplicationEntity>(PrepaidApplication.Create(artistId, opportunityId, paymentMethodId));
}
