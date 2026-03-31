using Concertable.Application.Interfaces;
using Concertable.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Concertable.Web.Services;

public class SignalRConcertNotificationService : IConcertNotificationService
{
    private readonly IHubContext<NotificationHub> hubContext;

    public SignalRConcertNotificationService(IHubContext<NotificationHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    public async Task ConcertDraftCreatedAsync(string userId, object payload)
    {
        await hubContext.Clients.Group(userId).SendAsync("ConcertDraftCreated", payload);
    }

    public async Task ConcertPostedAsync(string userId, object payload)
    {
        await hubContext.Clients.Group(userId).SendAsync("ConcertPosted", payload);
    }
}
