using Application.DTOs;
using Core.Entities;

namespace Application.Mappers;

public static class TransactionMappers
{
    public static TransactionDto ToDto(this TransactionEntity transaction) => new()
    {
        FromUserId = transaction.FromUserId,
        ToUserId = transaction.ToUserId,
        TransactionId = transaction.TransactionId,
        Amount = transaction.Amount,
        Type = transaction.Type,
        Status = transaction.Status,
        CreatedAt = transaction.CreatedAt
    };

    public static TransactionEntity ToEntity(this TransactionDto dto) => new()
    {
        FromUserId = dto.FromUserId,
        ToUserId = dto.ToUserId,
        TransactionId = dto.TransactionId,
        Amount = dto.Amount,
        Type = dto.Type,
        Status = dto.Status,
        CreatedAt = dto.CreatedAt
    };

    public static IEnumerable<TransactionDto> ToDtos(this IEnumerable<TransactionEntity> transactions) =>
        transactions.Select(t => t.ToDto());
}
