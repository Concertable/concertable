using Core.Enums;
using System.Text.Json.Serialization;
using Application.DTOs;

namespace Application.Interfaces.Payment;

[JsonDerivedType(typeof(TicketTransactionDto), "ticket")]
[JsonDerivedType(typeof(ConcertTransactionDto), "concert")]
public interface ITransaction
{
    TransactionType TransactionType { get; }
    int FromUserId { get; set; }
    int ToUserId { get; set; }
    string TransactionId { get; set; }
    long Amount { get; set; }
    string Status { get; set; }
    DateTime CreatedAt { get; set; }
}
