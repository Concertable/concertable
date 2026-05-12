using Concertable.Concert.Application.Workflow.Steps;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Steps;

internal class DeferredSettleStep : ISettleStep
{
    private readonly IDeferredConcertService deferredConcertService;

    public DeferredSettleStep(IDeferredConcertService deferredConcertService)
    {
        this.deferredConcertService = deferredConcertService;
    }

    public Task ExecuteAsync(int bookingId) =>
        deferredConcertService.SettleAsync(bookingId);
}
