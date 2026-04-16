using Concertable.Core.Entities;
using Concertable.Core.Enums;

namespace Concertable.Infrastructure.UnitTests.Services.Application;

public static class ApplicationBuilders
{
    public static ConcertEntity BuildDraftConcert() =>
        ConcertEntity.CreateDraft(0, "Test Concert", "Test About", []);

    public static OpportunityApplicationEntity BuildAccepted(int artistId = 1, int opportunityId = 1)
    {
        var app = OpportunityApplicationEntity.Create(artistId, opportunityId);
        app.Accept(BuildDraftConcert());
        return app;
    }

    public static OpportunityApplicationEntity BuildAwaitingPayment(int artistId = 1, int opportunityId = 1)
    {
        var app = BuildAccepted(artistId, opportunityId);
        app.AwaitPayment();
        return app;
    }
}
