using Concertable.Concert.Application.Workflow.Steps;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Steps;

internal class ApplyCommittedSettleStep : ISettleStep
{
    public Task ExecuteAsync(int bookingId) => Task.CompletedTask;
}
