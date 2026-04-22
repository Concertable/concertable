namespace Concertable.Concert.Application.Interfaces;

internal interface IConcertNotificationService
{
    Task ConcertDraftCreatedAsync(string userId, object payload);
    Task ConcertPostedAsync(string userId, object payload);
}
