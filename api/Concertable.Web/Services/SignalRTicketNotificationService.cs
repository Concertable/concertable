using Concertable.Application.Interfaces;
using Concertable.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Concertable.Web.Services;

public class SignalRTicketNotificationService : ITicketNotificationService
{
    private readonly IHubContext<NotificationHub> hubContext;

    public SignalRTicketNotificationService(IHubContext<NotificationHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    public async Task TicketPurchasedAsync(string userId, object payload)
    {
        await hubContext.Clients.Group(userId).SendAsync("TicketPurchased", payload);
    }
}
