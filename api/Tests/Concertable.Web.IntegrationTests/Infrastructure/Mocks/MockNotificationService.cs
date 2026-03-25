using Application.Interfaces;

namespace Concertable.Web.IntegrationTests.Infrastructure.Mocks;

public class MockNotificationService : IConcertNotificationService, ITicketNotificationService
{
    public readonly List<(string UserId, object Payload)> DraftCreated = [];
    public readonly List<(string UserId, object Payload)> ConcertPosted = [];
    public readonly List<(string UserId, object Payload)> TicketPurchased = [];

    public Task ConcertDraftCreatedAsync(string userId, object payload)
    {
        DraftCreated.Add((userId, payload));
        return Task.CompletedTask;
    }

    public Task ConcertPostedAsync(string userId, object payload)
    {
        ConcertPosted.Add((userId, payload));
        return Task.CompletedTask;
    }

    public Task TicketPurchasedAsync(string userId, object payload)
    {
        TicketPurchased.Add((userId, payload));
        return Task.CompletedTask;
    }

    public void Reset()
    {
        DraftCreated.Clear();
        ConcertPosted.Clear();
        TicketPurchased.Clear();
    }
}
