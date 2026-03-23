using Core.Enums;

namespace Core.Entities;

public class SettlementTransactionEntity : TransactionEntity
{
    public override TransactionType TransactionType => TransactionType.Settlement;
    public int ApplicationId { get; set; }
    public ConcertApplicationEntity Application { get; set; } = null!;
}
