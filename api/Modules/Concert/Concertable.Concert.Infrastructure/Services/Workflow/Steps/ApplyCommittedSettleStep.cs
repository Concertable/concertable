using Concertable.Concert.Application.Workflow.Steps;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Steps;

internal class ApplyCommittedSettleStep : ISettleStep
{
    private readonly IImmediateConcertService immediateConcertService;

    public ApplyCommittedSettleStep(IImmediateConcertService immediateConcertService)
    {
        this.immediateConcertService = immediateConcertService;
    }

    public Task ExecuteAsync(int bookingId) =>
        immediateConcertService.SettleAsync(bookingId);
}
