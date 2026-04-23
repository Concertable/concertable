using Concertable.Shared;

namespace Concertable.Artist.Contracts.Events;

public record ArtistChangedEvent(
    int ArtistId,
    Guid UserId,
    string Name,
    string? Avatar,
    string? BannerUrl,
    string? County,
    string? Town,
    string? Email,
    IReadOnlyCollection<int> GenreIds) : IIntegrationEvent;
