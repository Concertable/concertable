using Application.DTOs;
using Core.Entities;
using Core.Interfaces;
using Core.Parameters;

namespace Application.Interfaces
{
    public interface IEventRepository : IRepository<Event>
    {
        Task<IEnumerable<EventHeaderDto>> GetHeaders(int userId, EventParams eventParams);
        Task<Event> GetByIdAsync(int id);
        Task<Event?> GetByApplicationIdAsync(int applicationId);
        Task<IEnumerable<Event>> GetUpcomingByVenueIdAsync(int id);
        Task<IEnumerable<Event>> GetUpcomingByArtistIdAsync(int id);
        Task<IEnumerable<Event>> GetHistoryByVenueIdAsync(int id);
        Task<IEnumerable<Event>> GetHistoryByArtistIdAsync(int id);
        Task<IEnumerable<Event>> GetUnpostedByArtistIdAsync(int id);
        Task<IEnumerable<Event>> GetUnpostedByVenueIdAsync(int id);
        Task<bool> ArtistHasEventOnDateAsync(int artistId, DateTime date);
        Task<bool> ListingHasEventAsync(int listingId);
        Task<bool> VenueHasEventOnDateAsync(int venueId, DateTime date);
    }
}
