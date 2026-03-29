using Concertable.Application.Responses;

namespace Concertable.Application.DTOs;

public record MessageDto
{
    public int Id { get; set; }
    public required UserDto FromUser { get; set; }
    public ActionDto? Action { get; set; }
    public required string Content { get; set; }
}

public record MessageSummaryDto(Pagination<MessageDto> Messages, int UnreadCount);
