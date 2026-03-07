using Application.Responses;

namespace Application.DTOs
{
    public record MessageDto
    {
        public int Id { get; set; }
        public UserDto FromUser { get; set; }
        public ActionDto? Action { get; set; }
        public string Content { get; set; }
    }

    public record MessageSummaryDto
    {
        public Pagination<MessageDto> Messages { get; set; }
        public int UnreadCount { get; set; }
    }
}
