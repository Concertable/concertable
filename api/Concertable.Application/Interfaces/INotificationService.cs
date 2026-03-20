namespace Application.Interfaces;

public interface INotificationService
{
    Task ConcertCreatedAsync(string userId, object payload);
}
