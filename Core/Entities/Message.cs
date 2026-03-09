namespace Core.Entities;

public class Message : BaseEntity
{
    public required string Content { get; set; }
    public int FromUserId { get; set; }
    public int ToUserId { get; set; }
    public string? Action { get; set; }
    public int? ActionId { get; set; }
    public DateTime SentDate { get; set; }
    public bool Read { get; set; }
    public User FromUser { get; set; } = null!;
    public User ToUser { get; set; } = null!;
}
