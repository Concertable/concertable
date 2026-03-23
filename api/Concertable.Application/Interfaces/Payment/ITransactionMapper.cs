using Core.Entities;
using Core.Enums;

namespace Application.Interfaces.Payment;

public interface ITransactionMapper
{
    TransactionType TransactionType { get; }
    TransactionEntity ToEntity(ITransaction dto);
    ITransaction ToDto(TransactionEntity entity);
}
