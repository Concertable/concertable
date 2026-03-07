using Application.DTOs;
using Application.Responses;
using Core.Parameters;

namespace Application.Interfaces
{
    public interface IEventService
    {
        Task<EventDto> GetDetailsByIdAsync(int id);
        Task<EventDto> GetDetailsByApplicationIdAsync(int applicationId);
        Task<IEnumerable<EventDto>> GetUpcomingByVenueIdAsync(int id);
        Task<IEnumerable<EventDto>> GetUpcomingByArtistIdAsync(int id);
        Task<ListingApplicationPurchaseResponse> BookAsync(EventBookingParams bookingParams);
        Task<ListingApplicationPurchaseResponse> CompleteAsync(PurchaseCompleteDto purchaseCompleteDto);
        Task<EventDto> UpdateAsync(EventDto eventDto);
        Task<EventPostResponse> PostAsync(EventDto eventDto);
        Task<IEnumerable<EventHeaderDto>> GetRecommendedHeadersAsync();
        Task<IEnumerable<EventDto>> GetHistoryByArtistIdAsync(int id);
        Task<IEnumerable<EventDto>> GetHistoryByVenueIdAsync(int id);
        Task<IEnumerable<EventDto>> GetUnpostedByArtistIdAsync(int id);
        Task<IEnumerable<EventDto>> GetUnpostedByVenueIdAsync(int id);
    }
}
