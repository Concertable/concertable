namespace Application.Interfaces;

public interface ITicketNotificationService
{
    Task TicketPurchasedAsync(string userId, object payload);
}
