using Concertable.Notification.Contracts;
using Concertable.Notification.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Concertable.Notification.Infrastructure;

internal class SignalRNotificationModule : INotificationModule
{
    private readonly IHubContext<NotificationHub> hubContext;

    public SignalRNotificationModule(IHubContext<NotificationHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    public Task SendAsync(string userId, string eventName, object payload) =>
        hubContext.Clients.Group(userId).SendAsync(eventName, payload);
}
