using Core.Enums;

namespace Core.Entities;

public abstract class TransactionEntity : BaseEntity
{
    public abstract TransactionType TransactionType { get; }
    public int FromUserId { get; set; }
    public UserEntity FromUser { get; set; } = null!;
    public int ToUserId { get; set; }
    public UserEntity ToUser { get; set; } = null!;
    public required string PaymentIntentId { get; set; }
    public long Amount { get; set; }
    public TransactionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
