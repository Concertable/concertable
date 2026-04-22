namespace Concertable.Artist.Infrastructure;

internal class ArtistModule(IArtistRepository repo) : IArtistModule
{
    public Task<int?> GetIdByUserIdAsync(Guid userId) =>
        repo.GetIdByUserIdAsync(userId);

    public Task<ArtistSummaryDto?> GetSummaryAsync(int artistId) =>
        repo.GetSummaryAsync(artistId);

    public Task<IReadOnlySet<int>> GetGenreIdsAsync(int artistId) =>
        repo.GetGenreIdsAsync(artistId);
}
