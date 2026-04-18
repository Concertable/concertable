using Concertable.Core.Enums;

namespace Concertable.Core.Entities;

public class SettlementTransactionEntity : TransactionEntity
{
    private SettlementTransactionEntity() { }

    private SettlementTransactionEntity(Guid fromUserId, Guid toUserId, string paymentIntentId, long amount, TransactionStatus status, int applicationId)
        : base(fromUserId, toUserId, paymentIntentId, amount, status)
    {
        ApplicationId = applicationId;
    }

    public override TransactionType TransactionType => TransactionType.Settlement;
    public int ApplicationId { get; private set; }
    public OpportunityApplicationEntity Application { get; set; } = null!;

    public static SettlementTransactionEntity Create(Guid fromUserId, Guid toUserId, string paymentIntentId, long amount, TransactionStatus status, int applicationId)
        => new(fromUserId, toUserId, paymentIntentId, amount, status, applicationId);
}
