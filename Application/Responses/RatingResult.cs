namespace Application.Responses;

public record RatingResult
{
    public int EntityId { get; init; }
    public double AverageRating { get; init; }
}
