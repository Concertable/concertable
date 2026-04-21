using Concertable.Shared;
using NetTopologySuite.Geometries;

namespace Concertable.Identity.Contracts.Events;

public record UserLocationUpdatedEvent(
    Guid UserId,
    Point Location,
    Address? Address) : IIntegrationEvent;
