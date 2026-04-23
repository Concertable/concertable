using Concertable.Core.Enums;
using static Concertable.Seeding.Extensions.EntityReflectionExtensions;

namespace Concertable.Seeding.Factories;

public static class OpportunityApplicationFactory
{
    public static OpportunityApplicationEntity Create(int artistId, int opportunityId)
        => OpportunityApplicationEntity.Create(artistId, opportunityId);

    public static OpportunityApplicationEntity Accepted(int artistId, int opportunityId, ConcertBookingEntity booking)
    {
        var app = New<OpportunityApplicationEntity>()
            .With(nameof(OpportunityApplicationEntity.ArtistId), artistId)
            .With(nameof(OpportunityApplicationEntity.OpportunityId), opportunityId)
            .With(nameof(OpportunityApplicationEntity.Status), ApplicationStatus.Accepted);
        app.Booking = booking;
        return app;
    }
}
