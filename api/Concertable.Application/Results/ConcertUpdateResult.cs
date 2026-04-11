namespace Concertable.Application.Results;

public record ConcertUpdateResult
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string About { get; set; }
    public decimal Price { get; set; }
    public int TotalTickets { get; set; }
    public int AvailableTickets { get; set; }
}
