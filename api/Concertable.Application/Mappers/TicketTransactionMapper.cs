using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Entities;

namespace Concertable.Application.Mappers;

public class TicketTransactionMapper : ITransactionMapper
{
    public TransactionEntity ToEntity(ITransaction dto)
    {
        var d = (TicketTransactionDto)dto;
        return TicketTransactionEntity.Create(d.FromUserId, d.ToUserId, d.PaymentIntentId, d.Amount, d.Status, d.ConcertId);
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
