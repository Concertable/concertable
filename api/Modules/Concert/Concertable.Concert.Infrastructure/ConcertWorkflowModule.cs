using Concertable.Concert.Application.Interfaces;
using Concertable.Concert.Contracts;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure;

internal sealed class ConcertWorkflowModule(
    ISettlementExecutor SettlementExecutor,
    ICompletionExecutor CompletionExecutor) : IConcertWorkflowModule
{
    public Task SettleAsync(int bookingId, CancellationToken ct = default)
        => SettlementExecutor.SettleAsync(bookingId);

    public async Task FinishAsync(int concertId, CancellationToken ct = default)
    {
        var result = await CompletionExecutor.FinishAsync(concertId);
        if (result.IsFailed)
            throw new BadRequestException(result.Errors);
    }
}
