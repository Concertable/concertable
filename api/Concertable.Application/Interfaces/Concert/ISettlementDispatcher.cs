namespace Concertable.Application.Interfaces.Concert;

public interface ISettlementDispatcher
{
    Task SettleAsync(int bookingId);
}
