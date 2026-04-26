namespace Concertable.Concert.Infrastructure.Services;

internal class ConcertNotifier : IConcertNotifier
{
    private readonly INotificationModule notifications;

    public ConcertNotifier(INotificationModule notifications)
    {
        this.notifications = notifications;
    }

    public Task ConcertDraftCreatedAsync(string userId, object payload) =>
        notifications.SendAsync(userId, "ConcertDraftCreated", payload);

    public Task ConcertPostedAsync(string userId, object payload) =>
        notifications.SendAsync(userId, "ConcertPosted", payload);
}
