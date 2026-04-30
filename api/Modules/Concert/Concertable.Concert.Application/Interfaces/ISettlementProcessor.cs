namespace Concertable.Concert.Application.Interfaces;

internal interface ISettlementExecutor
{
    Task SettleAsync(int bookingId);
}
