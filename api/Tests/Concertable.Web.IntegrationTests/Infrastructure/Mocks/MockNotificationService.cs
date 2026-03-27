using Application.Interfaces;
using Concertable.Web.IntegrationTests.Infrastructure;

namespace Concertable.Web.IntegrationTests.Infrastructure.Mocks;

public class MockNotificationService : IMockNotificationService
{
    public List<(string UserId, object Payload)> DraftCreated { get; } = [];
    public List<(string UserId, object Payload)> ConcertPosted { get; } = [];
    public List<(string UserId, object Payload)> TicketPurchased { get; } = [];

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
