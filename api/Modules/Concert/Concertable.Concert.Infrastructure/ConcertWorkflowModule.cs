using Concertable.Concert.Application.Interfaces;
using Concertable.Concert.Contracts;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure;

internal sealed class ConcertWorkflowModule(
    ISettlementDispatcher SettlementDispatcher,
    ICompletionDispatcher CompletionDispatcher) : IConcertWorkflowModule
{
    public Task SettleAsync(int bookingId, CancellationToken ct = default)
        => SettlementDispatcher.SettleAsync(bookingId);

    public async Task FinishAsync(int concertId, CancellationToken ct = default)
    {
        var result = await CompletionDispatcher.FinishAsync(concertId);
        if (result.IsFailed)
            throw new BadRequestException(result.Errors);
    }
}
