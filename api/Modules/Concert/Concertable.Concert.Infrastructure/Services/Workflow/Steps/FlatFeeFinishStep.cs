using Concertable.Concert.Application.Workflow.Steps;
using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Steps;

internal class FlatFeeFinishStep : IFinishStep
{
    private readonly IBookingService bookingService;
    private readonly IEscrowModule escrowModule;

    public FlatFeeFinishStep(IBookingService bookingService, IEscrowModule escrowModule)
    {
        this.bookingService = bookingService;
        this.escrowModule = escrowModule;
    }

    public async Task ExecuteAsync(int concertId)
    {
        var booking = await bookingService.CompleteByConcertIdAsync(concertId);

        var release = await escrowModule.ReleaseByBookingIdAsync(booking.Id);
        if (release.IsFailed)
            throw new BadRequestException(release.Errors);
    }
}
