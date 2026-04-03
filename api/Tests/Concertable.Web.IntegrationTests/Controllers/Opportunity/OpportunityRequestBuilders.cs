using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Requests;
using Concertable.Web.IntegrationTests.Infrastructure;
using Concertable.Core.Enums;

namespace Concertable.Web.IntegrationTests.Controllers.Opportunity;

public static class OpportunityRequestBuilders
{
    public static OpportunityRequest BuildRequest(IContract contract) =>
        new()
        {
            StartDate = DateTime.UtcNow.AddMonths(1),
            EndDate = DateTime.UtcNow.AddMonths(1).AddHours(3),
            GenreIds = [TestConstants.GenreId],
            Contract = contract
        };

    public static OpportunityRequest BuildDefaultRequest() =>
        BuildRequest(new FlatFeeContractDto { PaymentMethod = PaymentMethod.Cash, Fee = 500 });
}
