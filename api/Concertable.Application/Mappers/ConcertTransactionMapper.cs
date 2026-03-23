using Application.DTOs;
using Application.Interfaces.Payment;
using Core.Entities;
using Core.Enums;

namespace Application.Mappers;

public class SettlementTransactionMapper : ITransactionMapper
{
    public TransactionType TransactionType => TransactionType.Settlement;

    public TransactionEntity ToEntity(ITransaction dto)
    {
        var d = (SettlementTransactionDto)dto;
        return new SettlementTransactionEntity
        {
            ApplicationId = d.ApplicationId,
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
        var e = (SettlementTransactionEntity)entity;
        return new SettlementTransactionDto
        {
            Id = e.Id,
            ApplicationId = e.ApplicationId,
            FromUserId = e.FromUserId,
            ToUserId = e.ToUserId,
            PaymentIntentId = e.PaymentIntentId,
            Amount = e.Amount,
            Status = e.Status,
            CreatedAt = e.CreatedAt
        };
    }
}
