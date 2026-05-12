using Concertable.Concert.Application.Workflow.Steps;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Steps;

internal class VenueHireFinishStep : IFinishStep
{
    private readonly IImmediateConcertService immediateConcertService;

    public VenueHireFinishStep(IImmediateConcertService immediateConcertService)
    {
        this.immediateConcertService = immediateConcertService;
    }

    public Task ExecuteAsync(int concertId) =>
        immediateConcertService.FinishAsync(concertId);
}
