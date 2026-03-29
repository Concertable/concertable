namespace Concertable.Application.Requests;

public record CreateReviewRequest
{
    public int ConcertId { get; set; }
    public int Stars { get; set; }
    public string? Details { get; set; }
}
