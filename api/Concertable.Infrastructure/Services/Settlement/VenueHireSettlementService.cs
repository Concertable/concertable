using Application.Interfaces.Concert;

namespace Infrastructure.Services.Settlement;

public class VenueHireSettlementService : IContractSettlementStrategy
{
    public Task SettleAsync(int concertId)
    {
        throw new NotImplementedException();
    }
}
