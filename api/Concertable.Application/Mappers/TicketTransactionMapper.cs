using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Entities;
using Concertable.Core.Enums;

namespace Concertable.Application.Mappers;

public class TicketTransactionMapper : ITransactionMapper
{
    public TransactionType TransactionType => TransactionType.Ticket;

    public TransactionEntity ToEntity(ITransaction dto)
    {
        var d = (TicketTransactionDto)dto;
        return new TicketTransactionEntity
        {
            CreatedBy = d.FromUserId.ToString(),
            ConcertId = d.ConcertId,
            FromUserId = d.FromUserId,
            ToUserId = d.ToUserId,
            PaymentIntentId = d.PaymentIntentId,
            Amount = d.Amount,
            Status = d.Status,
            CreatedAt = d.CreatedAt
        };
    }

    public ITransaction ToDto(TransactionEntity entity)
    {
        var e = (TicketTransactionEntity)entity;
        return new TicketTransactionDto
        {
            Id = e.Id,
            ConcertId = e.ConcertId,
            FromUserId = e.FromUserId,
            ToUserId = e.ToUserId,
            PaymentIntentId = e.PaymentIntentId,
            Amount = e.Amount,
            Status = e.Status,
            CreatedAt = e.CreatedAt
        };
    }
}
