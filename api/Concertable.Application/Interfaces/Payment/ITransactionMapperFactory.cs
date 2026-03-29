using Concertable.Core.Enums;

namespace Concertable.Application.Interfaces.Payment;

public interface ITransactionMapperFactory
{
    ITransactionMapper Create(TransactionType type);
}
