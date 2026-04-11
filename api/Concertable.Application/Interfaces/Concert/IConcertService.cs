using Concertable.Application.DTOs;
using Concertable.Application.Requests;
using Concertable.Application.Results;
using Concertable.Core.Parameters;


namespace Concertable.Application.Interfaces.Concert;

public interface IConcertService
{
    Task<ConcertDto> GetDetailsByIdAsync(int id);
    Task<ConcertDto> GetDetailsByApplicationIdAsync(int applicationId);
    Task<IEnumerable<ConcertSummaryDto>> GetUpcomingByVenueIdAsync(int id);
    Task<IEnumerable<ConcertSummaryDto>> GetUpcomingByArtistIdAsync(int id);
    Task<int> CreateDraftAsync(int applicationId);
    Task<ConcertUpdateResult> UpdateAsync(int id, UpdateConcertRequest request);
    Task<ConcertPostResult> PostAsync(int id, UpdateConcertRequest request);
    Task<IEnumerable<ConcertHeaderDto>> GetRecommendedHeadersAsync();
    Task<IEnumerable<ConcertSummaryDto>> GetHistoryByArtistIdAsync(int id);
    Task<IEnumerable<ConcertSummaryDto>> GetHistoryByVenueIdAsync(int id);
    Task<IEnumerable<ConcertSummaryDto>> GetUnpostedByArtistIdAsync(int id);
    Task<IEnumerable<ConcertSummaryDto>> GetUnpostedByVenueIdAsync(int id);
}
