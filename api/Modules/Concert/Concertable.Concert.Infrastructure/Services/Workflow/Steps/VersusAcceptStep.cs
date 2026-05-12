using Concertable.Concert.Application.Workflow.Steps;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Steps;

internal class VersusAcceptStep : IPaidAcceptStep
{
    private readonly IDeferredConcertService deferredConcertService;

    public VersusAcceptStep(IDeferredConcertService deferredConcertService)
    {
        this.deferredConcertService = deferredConcertService;
    }

    public Task ExecuteAsync(int applicationId, string paymentMethodId) =>
        deferredConcertService.RegisterPaymentAsync(applicationId, paymentMethodId);
}
