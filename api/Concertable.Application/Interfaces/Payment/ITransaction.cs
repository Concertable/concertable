using Concertable.Application.DTOs;
using Concertable.Core.Enums;
using System.Text.Json.Serialization;

namespace Concertable.Application.Interfaces.Payment;

[JsonDerivedType(typeof(TicketTransactionDto), "ticket")]
[JsonDerivedType(typeof(SettlementTransactionDto), "settlement")]
public interface ITransaction
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
