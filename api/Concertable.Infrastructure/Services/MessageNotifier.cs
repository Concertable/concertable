using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Notification.Contracts;

namespace Concertable.Infrastructure.Services;

internal class MessageNotifier : IMessageNotifier
{
    private readonly INotificationModule notifications;

    public MessageNotifier(INotificationModule notifications)
    {
        this.notifications = notifications;
    }

    public Task MessageReceivedAsync(string userId, MessageDto payload) =>
        notifications.SendAsync(userId, "MessageReceived", payload);
}
