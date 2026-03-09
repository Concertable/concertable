namespace Core.Entities;

public class Transaction : BaseEntity
{
    public int FromUserId { get; set; }
    public User FromUser { get; set; } = null!;
    public int ToUserId { get; set; }
    public User ToUser { get; set; } = null!;
    public required string TransactionId { get; set; }
    public long Amount { get; set; }
    public required string Type { get; set; }
    public required string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
