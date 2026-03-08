namespace Application.DTOs
{
    public record ReviewDto
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public int Stars { get; set; }
        public string? Details { get; set; }
    }

    public record ReviewSummaryDto(int TotalReviews, double? AverageRating);
}
