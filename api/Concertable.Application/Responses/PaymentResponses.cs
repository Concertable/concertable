namespace Concertable.Application.Responses;

public record PaymentResponse
{
    public bool RequiresAction { get; set; }
    public string? ClientSecret { get; set; }
    public string? TransactionId { get; set; }
}

public record TicketPaymentResponse : PaymentResponse
{
    public IEnumerable<int> TicketIds { get; set; } = [];
    public int ConcertId { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? UserEmail { get; set; }
}
