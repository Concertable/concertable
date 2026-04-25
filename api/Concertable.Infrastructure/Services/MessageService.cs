using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Mappers;
using Concertable.Application.Responses;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Core.Parameters;
using Concertable.Identity.Contracts;
using Concertable.Shared.Exceptions;

namespace Concertable.Infrastructure.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository messageRepository;
    private readonly IMessageNotificationService notificationService;
    private readonly ICurrentUser currentUser;
    private readonly IIdentityModule identityModule;
    private readonly TimeProvider timeProvider;

    public MessageService(
        IMessageRepository messageRepository,
        IMessageNotificationService notificationService,
        ICurrentUser currentUser,
        IIdentityModule identityModule,
        TimeProvider timeProvider)
    {
        this.messageRepository = messageRepository;
        this.notificationService = notificationService;
        this.currentUser = currentUser;
        this.identityModule = identityModule;
        this.timeProvider = timeProvider;
    }

    public async Task SendAsync(Guid fromUserId, Guid toUserId, string content, MessageAction? action = null)
    {
        var message = MessageEntity.Create(fromUserId, toUserId, content, timeProvider.GetUtcNow().DateTime, action);
        await messageRepository.AddAsync(message);
    }

    public async Task SendAndSaveAsync(Guid fromUserId, Guid toUserId, string content, MessageAction? action = null)
    {
        var message = MessageEntity.Create(fromUserId, toUserId, content, timeProvider.GetUtcNow().DateTime, action);

        await messageRepository.AddAsync(message);
        await messageRepository.SaveChangesAsync();

        var sender = await identityModule.GetUserByIdAsync(fromUserId)
            ?? throw new NotFoundException("Message sender not found");

        var fromUser = new UserDto
        {
            Id = sender.Id,
            Email = sender.Email,
            Role = sender.Role,
            Latitude = sender.Latitude,
            Longitude = sender.Longitude,
            County = sender.County,
            Town = sender.Town
        };

        await notificationService.MessageReceivedAsync(toUserId.ToString(), message.ToDto(fromUser));
    }

    public async Task<IPagination<MessageDto>> GetForUserAsync(IPageParams pageParams)
    {
        var userId = currentUser.GetId();
        var messages = await messageRepository.GetByUserIdAsync(userId, pageParams);

        return new Pagination<MessageDto>(
            messages.Data.Select(m => m.ToDto()),
            messages.TotalCount,
            messages.PageNumber,
            messages.PageSize);
    }

    public async Task<MessageSummaryDto> GetSummaryForUser()
    {
        var pageParams = new PageParams { PageNumber = 1, PageSize = 5 };

        var userId = currentUser.GetId();
        var messages = await messageRepository.GetByUserIdAsync(userId, pageParams);
        var unreadCount = await messageRepository.GetUnreadCountByUserIdAsync(userId);

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
        return await messageRepository.GetUnreadCountByUserIdAsync(currentUser.GetId());
    }

    public async Task MarkAsReadAsync(List<int> ids)
    {
        await messageRepository.MarkAsReadAsync(ids);
    }
}
