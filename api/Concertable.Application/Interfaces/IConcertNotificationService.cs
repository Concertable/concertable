namespace Application.Interfaces;

public interface IConcertNotificationService
{
    Task ConcertDraftCreatedAsync(string userId, object payload);
    Task ConcertPostedAsync(string userId, object payload);
}
