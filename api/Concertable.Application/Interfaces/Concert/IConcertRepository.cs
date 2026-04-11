using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Core.Entities;
using Concertable.Core.Enums;

namespace Concertable.Application.Interfaces.Concert;

public interface IConcertRepository : IRepository<Core.Entities.ConcertEntity>
{
    Task<ConcertEntity?> GetFullByIdAsync(int id);
    Task<ConcertDto?> GetDtoByIdAsync(int id);
    Task<ConcertSummaryDto?> GetSummaryAsync(int id);
    Task<ConcertDto?> GetDtoByApplicationIdAsync(int applicationId);
    Task<IEnumerable<ConcertSummaryDto>> GetUpcomingByVenueIdAsync(int id);
    Task<IEnumerable<ConcertSummaryDto>> GetUpcomingByArtistIdAsync(int id);
    Task<IEnumerable<ConcertSummaryDto>> GetHistoryByVenueIdAsync(int id);
    Task<IEnumerable<ConcertSummaryDto>> GetHistoryByArtistIdAsync(int id);
    Task<IEnumerable<ConcertSummaryDto>> GetUnpostedByArtistIdAsync(int id);
    Task<IEnumerable<ConcertSummaryDto>> GetUnpostedByVenueIdAsync(int id);
    Task<bool> ArtistHasConcertOnDateAsync(int artistId, DateTime date);
    Task<bool> OpportunityHasConcertAsync(int opportunityId);
    Task<bool> VenueHasConcertOnDateAsync(int venueId, DateTime date);
    Task<ContractType?> GetTypeByIdAsync(int id);
    Task<IEnumerable<int>> GetEndedConfirmedIdsAsync();
    Task<decimal> GetTotalRevenueByConcertIdAsync(int concertId);
}
