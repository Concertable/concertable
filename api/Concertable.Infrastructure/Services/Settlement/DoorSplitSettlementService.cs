using Application.Interfaces.Concert;

namespace Infrastructure.Services.Settlement;

public class DoorSplitSettlementService : ISettlementStrategy
{
    public Task SettleAsync(int concertId)
    {
        throw new NotImplementedException();
    }
}
