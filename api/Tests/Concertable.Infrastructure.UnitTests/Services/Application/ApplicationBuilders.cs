using Concertable.Core.Enums;

namespace Concertable.Infrastructure.UnitTests.Services.Application;

public static class ApplicationBuilders
{
    public static OpportunityApplicationEntity BuildAccepted(int artistId = 1, int opportunityId = 1)
    {
        var app = OpportunityApplicationEntity.Create(artistId, opportunityId);
        var booking = ConcertBookingEntity.Create(0);
        var concert = ConcertEntity.CreateDraft(0, "Test Concert", "Test About", []);
        booking.Confirm(concert);
        app.Accept(booking);
        return app;
    }

    public static OpportunityApplicationEntity BuildAwaitingPayment(int artistId = 1, int opportunityId = 1)
    {
        var app = OpportunityApplicationEntity.Create(artistId, opportunityId);
        var booking = ConcertBookingEntity.Create(0);
        booking.AwaitPayment();
        app.Accept(booking);
        return app;
    }
}
