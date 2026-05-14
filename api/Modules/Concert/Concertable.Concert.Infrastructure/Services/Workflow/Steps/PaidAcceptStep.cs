using Concertable.Concert.Application.Workflow.Steps;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Steps;

internal class PaidAcceptStep : IPaidAcceptStep
{
    private readonly IDeferredConcertService deferredConcertService;

    public PaidAcceptStep(IDeferredConcertService deferredConcertService)
    {
        this.deferredConcertService = deferredConcertService;
    }

    public Task ExecuteAsync(int applicationId, string paymentMethodId) =>
        deferredConcertService.RegisterPaymentAsync(applicationId, paymentMethodId);
}
