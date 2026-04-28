using static Concertable.Seeding.Extensions.EntityReflectionExtensions;

namespace Concertable.Seeding.Factories;

public static class ApplicationFactory
{
    public static ApplicationEntity Create(int artistId, int opportunityId)
        => ApplicationEntity.Create(artistId, opportunityId);

    public static ApplicationEntity Accepted(int artistId, int opportunityId, BookingEntity booking)
    {
        var app = New<ApplicationEntity>()
            .With(nameof(ApplicationEntity.ArtistId), artistId)
            .With(nameof(ApplicationEntity.OpportunityId), opportunityId)
            .With(nameof(ApplicationEntity.Status), ApplicationStatus.Accepted);
        app.Booking = booking;
        return app;
    }
}
