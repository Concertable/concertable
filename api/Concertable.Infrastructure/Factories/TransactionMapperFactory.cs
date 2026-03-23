using Application.Interfaces.Payment;
using Application.Mappers;
using Core.Enums;

namespace Infrastructure.Factories;

public class TransactionMapperFactory : ITransactionMapperFactory
{
    private readonly IDictionary<TransactionType, ITransactionMapper> mappers = new Dictionary<TransactionType, ITransactionMapper>
    {
        { TransactionType.Ticket, new TicketTransactionMapper() },
        { TransactionType.Settlement, new SettlementTransactionMapper() }
    };

    public ITransactionMapper Create(TransactionType type) => mappers[type];
}
