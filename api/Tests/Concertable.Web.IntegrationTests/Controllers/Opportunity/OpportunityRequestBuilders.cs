using Concertable.Concert.Application.Requests;
using Concertable.Web.IntegrationTests.Infrastructure;

namespace Concertable.Web.IntegrationTests.Controllers.Opportunity;

internal static class OpportunityRequestBuilders
{
    public static OpportunityRequest BuildRequest(IContract contract, int rockGenreId) =>
        new()
        {
            StartDate = DateTime.UtcNow.AddMonths(1),
            EndDate = DateTime.UtcNow.AddMonths(1).AddHours(3),
            GenreIds = [rockGenreId],
            Contract = contract
        };

    public static OpportunityRequest BuildDefaultRequest(int rockGenreId) =>
        BuildRequest(new FlatFeeContract { PaymentMethod = PaymentMethod.Cash, Fee = 500 }, rockGenreId);
}
