namespace Application.Requests
{
    public record CreateReviewRequest
    {
        public int EventId { get; set; }
        public int Stars { get; set; }
        public string? Details { get; set; }
    }
}
