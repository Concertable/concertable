using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;

namespace Web.Services;

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<ConcertHub> hubContext;

    public SignalRNotificationService(IHubContext<ConcertHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    public async Task ConcertCreatedAsync(string userId, object payload)
        => await hubContext.Clients.Group(userId).SendAsync("ConcertCreated", payload);
}
