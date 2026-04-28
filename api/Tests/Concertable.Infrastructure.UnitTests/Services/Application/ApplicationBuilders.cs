namespace Concertable.Infrastructure.UnitTests.Services.Application;

public static class ApplicationBuilders
{
    public static ApplicationEntity BuildAccepted(int artistId = 1, int opportunityId = 1)
    {
        var app = ApplicationEntity.Create(artistId, opportunityId);
        var booking = BookingEntity.Create(0);
        var concert = ConcertEntity.CreateDraft(0, artistId, 1, DateTime.UtcNow, DateTime.UtcNow.AddHours(2), "Test Concert", "Test About", []);
        booking.Confirm(concert);
        app.Accept(booking);
        return app;
    }

    public static ApplicationEntity BuildAwaitingPayment(int artistId = 1, int opportunityId = 1)
    {
        var app = ApplicationEntity.Create(artistId, opportunityId);
        var booking = BookingEntity.Create(0);
        booking.AwaitPayment();
        app.Accept(booking);
        return app;
    }
}
