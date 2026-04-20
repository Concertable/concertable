using NetTopologySuite.Geometries;

namespace Concertable.Identity.Domain.Events;

public record UserLocationUpdatedEvent(Guid UserId, Point Location, Address? Address) : IDomainEvent;
