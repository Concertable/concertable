using Core.Entities;
using Core.Interfaces;

namespace Application.Interfaces;

public interface IConcertRepository : IRepository<Concert>
{
    new Task<Concert?> GetByIdAsync(int id);
    Task<Concert?> GetByApplicationIdAsync(int applicationId);
    Task<IEnumerable<Concert>> GetUpcomingByVenueIdAsync(int id);
    Task<IEnumerable<Concert>> GetUpcomingByArtistIdAsync(int id);
    Task<IEnumerable<Concert>> GetHistoryByVenueIdAsync(int id);
    Task<IEnumerable<Concert>> GetHistoryByArtistIdAsync(int id);
    Task<IEnumerable<Concert>> GetUnpostedByArtistIdAsync(int id);
    Task<IEnumerable<Concert>> GetUnpostedByVenueIdAsync(int id);
    Task<bool> ArtistHasConcertOnDateAsync(int artistId, DateTime date);
    Task<bool> ListingHasConcertAsync(int listingId);
    Task<bool> VenueHasConcertOnDateAsync(int venueId, DateTime date);
}
