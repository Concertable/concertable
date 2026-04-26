namespace Concertable.Messaging.Contracts;

public interface IMessagingModule
{
    Task SendAsync(Guid fromUserId, Guid toUserId, string content, MessageAction? action = null);
    Task SendAndNotifyAsync(Guid fromUserId, Guid toUserId, string content, MessageAction? action = null);
}
