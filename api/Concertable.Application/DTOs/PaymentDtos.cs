using Application.Interfaces.Payment;
using Core.Enums;

namespace Application.DTOs;

public record PaymentDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "GBP";
    public required string PaymentMethodId { get; set; }
    public required string Description { get; set; }
    public int ConcertId { get; set; }
    public Guid UserId { get; set; }
}

public record TicketTransactionDto : ITransaction
{
    public int Id { get; set; }
    public TransactionType TransactionType => TransactionType.Ticket;
    public int ConcertId { get; set; }
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public required string PaymentIntentId { get; set; }
    public long Amount { get; set; }
    public TransactionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public record SettlementTransactionDto : ITransaction
{
    public int Id { get; set; }
    public TransactionType TransactionType => TransactionType.Settlement;
    public int ApplicationId { get; set; }
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public required string PaymentIntentId { get; set; }
    public long Amount { get; set; }
    public TransactionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public record PurchaseCompleteDto
{
    public int EntityId { get; set; }
    public required string TransactionId { get; set; }
    public Guid FromUserId { get; set; }
    public required string FromEmail { get; set; }
    public Guid ToUserId { get; set; }
    public int? Quantity { get; set; }
}
