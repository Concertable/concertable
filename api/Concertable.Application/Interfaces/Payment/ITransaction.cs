using Application.DTOs;
using Core.Enums;
using System.Text.Json.Serialization;

namespace Application.Interfaces.Payment;

[JsonDerivedType(typeof(TicketTransactionDto), "ticket")]
[JsonDerivedType(typeof(SettlementTransactionDto), "settlement")]
public interface ITransaction
{
    int Id { get; set; }
    TransactionType TransactionType { get; }
    int FromUserId { get; set; }
    int ToUserId { get; set; }
    string PaymentIntentId { get; set; }
    long Amount { get; set; }
    TransactionStatus Status { get; set; }
    DateTime CreatedAt { get; set; }
}
