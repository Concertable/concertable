using static Concertable.Seeding.Extensions.EntityReflectionExtensions;

namespace Concertable.Seeding.Factories;

public static class ReviewFactory
{
    public static ReviewEntity Create(Guid ticketId, byte stars, string? details)
        => New<ReviewEntity>()
            .With(nameof(ReviewEntity.TicketId), ticketId)
            .With(nameof(ReviewEntity.Stars), stars)
            .With(nameof(ReviewEntity.Details), details);
}
