namespace Concertable.Core.Projections;

public record RatingAggregate
{
    public int EntityId { get; init; }
    public double AverageRating { get; init; }
}
