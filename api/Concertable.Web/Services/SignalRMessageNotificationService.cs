using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Concertable.Web.Services;

public class SignalRMessageNotificationService : IMessageNotificationService
{
    private readonly IHubContext<NotificationHub> hubContext;

    public SignalRMessageNotificationService(IHubContext<NotificationHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    public async Task MessageReceivedAsync(string userId, MessageDto payload)
    {
        await hubContext.Clients.Group(userId).SendAsync("MessageReceived", payload);
    }
}
