namespace Application.DTOs
{
    public record ReviewDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public int Stars { get; set; }
        public string? Details { get; set; }
    }

    public record ReviewSummaryDto
    {
        public int TotalReviews { get; set; }
        public double? AverageRating { get; set; }
    }
}
