namespace Application.Interfaces.Concert;

public interface IImmediateSettlementProcessor
{
    Task SettleAsync(int concertId);
}
