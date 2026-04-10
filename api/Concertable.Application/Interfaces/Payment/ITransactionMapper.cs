using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces.Payment;

public interface ITransactionMapper
{
    TransactionEntity ToEntity(ITransaction dto);
    ITransaction ToDto(TransactionEntity entity);
}
