using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;

namespace Web.Services;

public class SignalRNotificationService : IConcertNotificationService, ITicketNotificationService
{
    private readonly IHubContext<NotificationHub> hubContext;

    public SignalRNotificationService(IHubContext<NotificationHub> hubContext)
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

    public async Task TicketPurchasedAsync(string userId, object payload)
    {
        await hubContext.Clients.Group(userId).SendAsync("TicketPurchased", payload);
    }
}
