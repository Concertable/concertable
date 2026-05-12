using Concertable.Concert.Application.Workflow.Steps;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Steps;

internal class SimpleApplyStep : ISimpleApplyStep
{
    public Task<ApplicationEntity> ExecuteAsync(int artistId, int opportunityId) =>
        Task.FromResult<ApplicationEntity>(StandardApplication.Create(artistId, opportunityId));
}
