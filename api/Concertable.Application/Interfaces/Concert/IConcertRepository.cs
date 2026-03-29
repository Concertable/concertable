using Concertable.Application.Interfaces;
using Concertable.Core.Entities;
using Concertable.Core.Enums;

namespace Concertable.Application.Interfaces.Concert;

public interface IConcertRepository : IRepository<Core.Entities.ConcertEntity>
{
    Task<Core.Entities.ConcertEntity?> GetDetailsByIdAsync(int id);
    Task<Core.Entities.ConcertEntity?> GetByApplicationIdAsync(int applicationId);
    Task<IEnumerable<Core.Entities.ConcertEntity>> GetUpcomingByVenueIdAsync(int id);
    Task<IEnumerable<Core.Entities.ConcertEntity>> GetUpcomingByArtistIdAsync(int id);
    Task<IEnumerable<Core.Entities.ConcertEntity>> GetHistoryByVenueIdAsync(int id);
    Task<IEnumerable<Core.Entities.ConcertEntity>> GetHistoryByArtistIdAsync(int id);
    Task<IEnumerable<Core.Entities.ConcertEntity>> GetUnpostedByArtistIdAsync(int id);
    Task<IEnumerable<Core.Entities.ConcertEntity>> GetUnpostedByVenueIdAsync(int id);
    Task<bool> ArtistHasConcertOnDateAsync(int artistId, DateTime date);
    Task<bool> OpportunityHasConcertAsync(int opportunityId);
    Task<bool> VenueHasConcertOnDateAsync(int venueId, DateTime date);
    Task<ContractType?> GetTypeByIdAsync(int id);
    Task<IEnumerable<int>> GetEndedConfirmedIdsAsync();
    Task<decimal> GetTotalRevenueByConcertIdAsync(int concertId);
}
