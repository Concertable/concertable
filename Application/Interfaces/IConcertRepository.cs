using Application.DTOs;
using Core.Entities;
using Core.Interfaces;
using Core.Parameters;

namespace Application.Interfaces
{
    public interface IConcertRepository : IRepository<Concert>
    {
        Task<IEnumerable<ConcertHeaderDto>> GetHeaders(int userId, ConcertParams concertParams);
        Task<Concert> GetByIdAsync(int id);
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
}
