using Concertable.Core.Enums;

namespace Concertable.Core.Entities;

public class TicketTransactionEntity : TransactionEntity
{
    public override TransactionType TransactionType => TransactionType.Ticket;
    public int ConcertId { get; set; }
    public ConcertEntity Concert { get; set; } = null!;
}
