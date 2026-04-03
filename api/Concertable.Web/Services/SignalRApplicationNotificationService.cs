using Concertable.Application.Interfaces;
using Concertable.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Concertable.Web.Services;

public class SignalRApplicationNotificationService : IApplicationNotificationService
{
    private readonly IHubContext<NotificationHub> hubContext;

    public SignalRApplicationNotificationService(IHubContext<NotificationHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    public async Task ApplicationAcceptedAsync(string userId, object payload)
    {
        await hubContext.Clients.Group(userId).SendAsync("ApplicationAccepted", payload);
    }
}
