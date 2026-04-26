using Concertable.Concert.Application.Interfaces;
using Concertable.Concert.Contracts;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure;

internal sealed class ConcertWorkflowModule(
    ISettlementDispatcher settlementDispatcher,
    ICompletionDispatcher completionDispatcher) : IConcertWorkflowModule
{
    public Task SettleAsync(int bookingId, CancellationToken ct = default)
        => settlementDispatcher.SettleAsync(bookingId);

    public async Task FinishAsync(int concertId, CancellationToken ct = default)
    {
        var result = await completionDispatcher.FinishAsync(concertId);
        if (result.IsFailed)
            throw new BadRequestException(result.Errors);
    }
}
