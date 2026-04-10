using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Entities;
using Concertable.Core.Enums;

namespace Concertable.Application.Mappers;

public class TransactionMapper : ITransactionMapper
{
    private readonly IDictionary<TransactionType, ITransactionMapper> mappers = new Dictionary<TransactionType, ITransactionMapper>
    {
        { TransactionType.Ticket, new TicketTransactionMapper() },
        { TransactionType.Settlement, new SettlementTransactionMapper() }
    };

    public TransactionEntity ToEntity(ITransaction dto) => mappers[dto.TransactionType].ToEntity(dto);
    public ITransaction ToDto(TransactionEntity entity) => mappers[entity.TransactionType].ToDto(entity);
}
