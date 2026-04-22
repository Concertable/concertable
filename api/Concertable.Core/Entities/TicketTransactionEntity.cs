using Concertable.Concert.Domain;
using Concertable.Core.Enums;

namespace Concertable.Core.Entities;

public class TicketTransactionEntity : TransactionEntity
{
    private TicketTransactionEntity() { }

    private TicketTransactionEntity(Guid fromUserId, Guid toUserId, string paymentIntentId, long amount, TransactionStatus status, int concertId)
        : base(fromUserId, toUserId, paymentIntentId, amount, status)
    {
        ConcertId = concertId;
    }

    public override TransactionType TransactionType => TransactionType.Ticket;
    public int ConcertId { get; private set; }
    public ConcertEntity Concert { get; set; } = null!;

    public static TicketTransactionEntity Create(Guid fromUserId, Guid toUserId, string paymentIntentId, long amount, TransactionStatus status, int concertId)
        => new(fromUserId, toUserId, paymentIntentId, amount, status, concertId);
}
