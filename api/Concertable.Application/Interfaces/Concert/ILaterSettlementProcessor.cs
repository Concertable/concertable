namespace Application.Interfaces.Concert;

public interface ILaterSettlementProcessor
{
    Task SettleAsync(int concertId);
}
