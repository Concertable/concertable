using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Enums;

namespace Concertable.Core.Entities;

public abstract class TransactionEntity : IIdEntity, IAuditable
{
    public int Id { get; set; }
    public abstract TransactionType TransactionType { get; }
    public Guid FromUserId { get; set; }
    public UserEntity FromUser { get; set; } = null!;
    public Guid ToUserId { get; set; }
    public UserEntity ToUser { get; set; } = null!;
    public required string PaymentIntentId { get; set; }
    public long Amount { get; set; }
    public TransactionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public required string CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}
