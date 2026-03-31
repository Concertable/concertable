using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Enums;

namespace Concertable.Core.Entities;

public class MessageEntity : IEntity
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public MessageAction? Action { get; set; }
    public DateTime SentDate { get; set; }
    public bool Read { get; set; }
    public UserEntity FromUser { get; set; } = null!;
    public UserEntity ToUser { get; set; } = null!;
}
