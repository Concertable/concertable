using Concertable.Core.Enums;

namespace Concertable.Core.Entities;

public class SettlementTransactionEntity : TransactionEntity
{
    public override TransactionType TransactionType => TransactionType.Settlement;
    public int ApplicationId { get; set; }
    public ConcertApplicationEntity Application { get; set; } = null!;
}
