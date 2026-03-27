using Application.DTOs;
using Application.Interfaces.Concert;
using Application.Requests;
using Concertable.Web.IntegrationTests.Infrastructure;
using Core.Enums;

namespace Concertable.Web.IntegrationTests.Controllers.ConcertOpportunity;

public static class ConcertOpportunityRequestBuilders
{
    public static ConcertOpportunityRequest BuildRequest(IContract contract) =>
        new()
        {
            StartDate = DateTime.UtcNow.AddMonths(1),
            EndDate = DateTime.UtcNow.AddMonths(1).AddHours(3),
            GenreIds = [TestConstants.GenreId],
            Contract = contract
        };

    public static ConcertOpportunityRequest BuildDefaultRequest() =>
        BuildRequest(new FlatFeeContractDto { PaymentMethod = PaymentMethod.Cash, Fee = 500 });
}
