using Application.DTOs;
using Application.Interfaces.Payment;
using Core.Entities;
using Core.Enums;

namespace Application.Mappers;

public class TicketTransactionMapper : ITransactionMapper
{
    public TransactionType TransactionType => TransactionType.Ticket;

    public TransactionEntity ToEntity(ITransaction dto)
    {
        var d = (TicketTransactionDto)dto;
        return new TicketTransactionEntity
        {
            ConcertId = d.ConcertId,
            FromUserId = d.FromUserId,
            ToUserId = d.ToUserId,
            TransactionId = d.TransactionId,
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
            ConcertId = e.ConcertId,
            FromUserId = e.FromUserId,
            ToUserId = e.ToUserId,
            TransactionId = e.TransactionId,
            Amount = e.Amount,
            Status = e.Status,
            CreatedAt = e.CreatedAt
        };
    }
}
