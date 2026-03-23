using Application.DTOs;
using Application.Interfaces.Payment;
using Core.Entities;
using Core.Enums;

namespace Application.Mappers;

public class ConcertTransactionMapper : ITransactionMapper
{
    public TransactionType TransactionType => TransactionType.Concert;

    public TransactionEntity ToEntity(ITransaction dto)
    {
        var d = (ConcertTransactionDto)dto;
        return new ConcertTransactionEntity
        {
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
        var e = (ConcertTransactionEntity)entity;
        return new ConcertTransactionDto
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
