namespace Application.Interfaces.Concert;

public interface ISettlementProcessor
{
    Task SettleAsync(int concertId);
}
