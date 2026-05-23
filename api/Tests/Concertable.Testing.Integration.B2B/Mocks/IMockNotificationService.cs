using Concertable.Notification.Contracts;

namespace Concertable.Testing.Integration.B2B.Mocks;

public interface IMockNotificationService : INotificationClient, IResettable
{
    List<(string UserId, object Payload)> DraftCreated { get; }
    List<(string UserId, object Payload)> ConcertPosted { get; }
    List<(string UserId, object Payload)> TicketPurchased { get; }
    List<(string UserId, string EventName, object Payload)> Other { get; }
}
