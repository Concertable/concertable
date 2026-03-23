using Core.Enums;

namespace Core.Entities;

public class ConcertTransactionEntity : TransactionEntity
{
    public override TransactionType TransactionType => TransactionType.Concert;
    public int ConcertId { get; set; }
    public ConcertEntity Concert { get; set; } = null!;
}
