using Concertable.Concert.Application.Workflow.Steps;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Steps;

internal class HeldSettleStep : ISettleStep
{
    private readonly IConcertDraftService concertDraftService;

    public HeldSettleStep(IConcertDraftService concertDraftService)
    {
        this.concertDraftService = concertDraftService;
    }

    public async Task ExecuteAsync(int bookingId)
    {
        var draftResult = await concertDraftService.CreateAsync(bookingId);
        if (draftResult.IsFailed)
            throw new BadRequestException(draftResult.Errors);
    }
}
