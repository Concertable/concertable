using Concertable.Notification.Contracts;

namespace Concertable.IntegrationTests.Common.Mocks;

public interface IMockNotificationService : INotificationModule, IResettable
{
    List<(string UserId, object Payload)> DraftCreated { get; }
    List<(string UserId, object Payload)> ConcertPosted { get; }
    List<(string UserId, object Payload)> TicketPurchased { get; }
    List<(string UserId, string EventName, object Payload)> Other { get; }
}
