namespace Concertable.Artist.Contracts;

public interface IArtistModule
{
    Task<int?> GetIdByUserIdAsync(Guid userId);
    Task<ArtistSummaryDto?> GetSummaryAsync(int artistId);
    Task<IReadOnlySet<int>> GetGenreIdsAsync(int artistId);
}
