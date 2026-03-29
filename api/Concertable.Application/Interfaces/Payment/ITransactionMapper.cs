using Concertable.Core.Entities;
using Concertable.Core.Enums;

namespace Concertable.Application.Interfaces.Payment;

public interface ITransactionMapper
{
    TransactionType TransactionType { get; }
    TransactionEntity ToEntity(ITransaction dto);
    ITransaction ToDto(TransactionEntity entity);
}
