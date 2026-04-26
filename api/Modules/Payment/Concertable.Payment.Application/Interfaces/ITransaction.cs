using System.Text.Json.Serialization;
using Concertable.Payment.Application.DTOs;

namespace Concertable.Payment.Application.Interfaces;

[JsonDerivedType(typeof(TicketTransactionDto), "ticket")]
[JsonDerivedType(typeof(SettlementTransactionDto), "settlement")]
internal interface ITransaction
{
    int Id { get; set; }
    TransactionType TransactionType { get; }
    Guid FromUserId { get; set; }
    Guid ToUserId { get; set; }
    string PaymentIntentId { get; set; }
    long Amount { get; set; }
    TransactionStatus Status { get; set; }
    DateTime CreatedAt { get; set; }
}
