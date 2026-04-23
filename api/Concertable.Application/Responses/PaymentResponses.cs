using Concertable.Payment.Contracts;

namespace Concertable.Application.Responses;

public record TicketPaymentResponse : PaymentResponse
{
    public IEnumerable<Guid> TicketIds { get; set; } = [];
    public int ConcertId { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? UserEmail { get; set; }
}
