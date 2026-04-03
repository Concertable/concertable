using Concertable.Application.Interfaces;

namespace Concertable.Web.IntegrationTests.Infrastructure.Mocks;

public interface IMockNotificationService : IConcertNotificationService, IApplicationNotificationService, ITicketNotificationService, IResettable
{
    List<(string UserId, object Payload)> DraftCreated { get; }
    List<(string UserId, object Payload)> ConcertPosted { get; }
    List<(string UserId, object Payload)> ApplicationAccepted { get; }
    List<(string UserId, object Payload)> TicketPurchased { get; }
}
