using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Web.Extentions;

namespace Web.Hubs
{
    [Authorize]
    public class PaymentHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            string userId = Context.User?.GetId();

            if(!string.IsNullOrEmpty(userId)) await Groups.AddToGroupAsync(Context.ConnectionId, userId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string userId = Context.User?.GetId();

            if (!string.IsNullOrEmpty(userId)) await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);

            await base.OnDisconnectedAsync(exception);
        }

    }
}
