using Concertable.Concert.Application.Workflow.Steps;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Steps;

internal class DeferredVerifyStep : IVerifyStep
{
    private readonly IDeferredConcertService deferredConcertService;

    public DeferredVerifyStep(IDeferredConcertService deferredConcertService)
    {
        this.deferredConcertService = deferredConcertService;
    }

    public Task ExecuteAsync(int applicationId) =>
        deferredConcertService.VerifyAsync(applicationId);
}
