using NetTopologySuite.Geometries;

namespace Concertable.Identity.Domain.Events;

public record UserLocationUpdatedDomainEvent(Guid UserId, Point Location, Address? Address) : IDomainEvent;
