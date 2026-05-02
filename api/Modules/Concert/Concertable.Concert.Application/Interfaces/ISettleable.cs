namespace Concertable.Concert.Application.Interfaces;

internal interface ISettleable
{
    Task SettleAsync(int bookingId);
}
