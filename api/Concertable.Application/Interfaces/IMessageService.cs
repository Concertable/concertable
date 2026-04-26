using Concertable.Application.DTOs;
using Concertable.Core.Parameters;
using Concertable.Messaging.Domain;

namespace Concertable.Application.Interfaces;

public interface IMessageService
{
    Task SendAsync(Guid fromUserId, Guid toUserId, string content, MessageAction? action = null);
    Task SendAndSaveAsync(Guid fromUserId, Guid toUserId, string content, MessageAction? action = null);
    Task<MessageSummaryDto> GetSummaryForUser();
    Task<IPagination<MessageDto>> GetForUserAsync(IPageParams pageParams);
    Task<int> GetUnreadCountForUserAsync();
    Task MarkAsReadAsync(List<int> ids);
}
