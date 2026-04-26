namespace Concertable.Messaging.Infrastructure;

internal class MessagingModule : IMessagingModule
{
    private readonly IMessageService messageService;

    public MessagingModule(IMessageService messageService)
    {
        this.messageService = messageService;
    }

    public Task SendAsync(Guid fromUserId, Guid toUserId, string content, MessageAction? action = null) =>
        messageService.SendAsync(fromUserId, toUserId, content, action);

    public Task SendAndNotifyAsync(Guid fromUserId, Guid toUserId, string content, MessageAction? action = null) =>
        messageService.SendAndNotifyAsync(fromUserId, toUserId, content, action);
}
