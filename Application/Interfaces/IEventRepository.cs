using Application.DTOs;
using Core.Entities;
using Core.Parameters;
using Application.Responses;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IEventRepository : IHeaderRepository<Event, EventHeaderDto>
    {
        Task<IEnumerable<EventHeaderDto>> GetFiltered(int userId, EventParams eventParams);
        Task<Event> GetByIdAsync(int id);
        Task<IEnumerable<EventHeaderDto>> GetFiltered(int amount);
        Task<Event?> GetByApplicationIdAsync(int applicationId);
        Task<IEnumerable<Event>> GetUpcomingByVenueIdAsync(int id);
        Task<IEnumerable<Event>> GetUpcomingByArtistIdAsync(int id);
        Task<IEnumerable<Event>> GetHistoryByVenueIdAsync(int id);
        Task<IEnumerable<Event>> GetHistoryByArtistIdAsync(int id);
        Task<IEnumerable<Event>> GetUnpostedByArtistIdAsync(int id);
        Task<IEnumerable<Event>> GetUnpostedByVenueIdAsync(int id);
        Task<bool> ArtistHasEventOnDateAsync(int artistId, DateTime date);
        Task<bool> ListingHasEventAsync(int listingId);
    }
}
