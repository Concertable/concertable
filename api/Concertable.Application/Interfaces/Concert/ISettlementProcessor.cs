namespace Concertable.Application.Interfaces.Concert;

public interface ISettlementProcessor
{
    Task SettleAsync(int applicationId);
}
