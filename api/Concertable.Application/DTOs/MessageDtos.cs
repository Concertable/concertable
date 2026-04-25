using Concertable.Core.Enums;

namespace Concertable.Application.DTOs;

public record MessageDto
{
    public int Id { get; set; }
    public required UserDto FromUser { get; set; }
    public MessageAction? Action { get; set; }
    public required string Content { get; set; }
}

public record MessageSummaryDto(Pagination<MessageDto> Messages, int UnreadCount);
