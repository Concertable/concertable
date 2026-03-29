using Concertable.Core.Interfaces;
using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Mappers;
using Concertable.Core.Entities;
using Concertable.Core.Parameters;
using Concertable.Application.Responses;

namespace Concertable.Infrastructure.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository messageRepository;
    private readonly ICurrentUser currentUser;
    private readonly TimeProvider timeProvider;

    public MessageService(
        IMessageRepository messageRepository,
        ICurrentUser currentUser,
        TimeProvider timeProvider)
    {
        this.messageRepository = messageRepository;
        this.currentUser = currentUser;
        this.timeProvider = timeProvider;
    }

    public async Task SendAsync(Guid fromUserId, Guid toUserId, string action, int actionId, string content)
    {
        var message = new MessageEntity
        {
            Content = content,
            FromUserId = fromUserId,
            ToUserId = toUserId,
            Action = action,
            ActionId = actionId,
            SentDate = timeProvider.GetUtcNow().DateTime,
            Read = false
        };

        await messageRepository.AddAsync(message);
    }

    public async Task SendAndSaveAsync(Guid fromUserId, Guid toUserId, string action, int actionId, string content)
    {
        var message = new MessageEntity
        {
            Content = content,
            FromUserId = fromUserId,
            ToUserId = toUserId,
            Action = action,
            ActionId = actionId,
            SentDate = timeProvider.GetUtcNow().DateTime,
            Read = false
        };

        await messageRepository.AddAsync(message);
        await messageRepository.SaveChangesAsync();
    }

    public async Task<Pagination<MessageDto>> GetForUserAsync(IPageParams pageParams)
    {
        var user = currentUser.Get();
        var messages = await messageRepository.GetByUserIdAsync(user.Id, pageParams);

        return new Pagination<MessageDto>(
            messages.Data.Select(m => m.ToDto()),
            messages.TotalCount,
            messages.PageNumber,
            messages.PageSize);
    }

    public async Task<MessageSummaryDto> GetSummaryForUser()
    {
        var pageParams = new PageParams { PageNumber = 1, PageSize = 5 };

        var user = currentUser.Get();
        var messages = await messageRepository.GetByUserIdAsync(user.Id, pageParams);
        var unreadCount = await messageRepository.GetUnreadCountByUserIdAsync(user.Id);

        var pagination = new Pagination<MessageDto>(
            messages.Data.Select(m => m.ToDto()),
            messages.TotalCount,
            messages.PageNumber,
            messages.PageSize
        );
        return new MessageSummaryDto(pagination, unreadCount);
    }

    public async Task<int> GetUnreadCountForUserAsync()
    {
        var user = currentUser.Get();
        return await messageRepository.GetUnreadCountByUserIdAsync(user.Id);
    }

    public async Task MarkAsReadAsync(List<int> ids)
    {
        await messageRepository.MarkAsReadAsync(ids);
    }
}
