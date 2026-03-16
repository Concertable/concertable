namespace Core.Entities;

public class MessageEntity : BaseEntity
{
    public required string Content { get; set; }
    public int FromUserId { get; set; }
    public int ToUserId { get; set; }
    public string? Action { get; set; }
    public int? ActionId { get; set; }
    public DateTime SentDate { get; set; }
    public bool Read { get; set; }
    public UserEntity FromUser { get; set; } = null!;
    public UserEntity ToUser { get; set; } = null!;
}
