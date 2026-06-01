namespace Concertable.Contracts;

public sealed record ReviewDto
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public int Stars { get; set; }
    public string? Details { get; set; }
}

public sealed record ReviewSummaryDto(int TotalReviews, double? AverageRating);
