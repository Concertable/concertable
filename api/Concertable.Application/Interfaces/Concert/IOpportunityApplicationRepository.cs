using Concertable.Application.Interfaces;
using Concertable.Application.Models;

namespace Concertable.Application.Interfaces.Concert;

public interface IOpportunityApplicationRepository : IRepository<OpportunityApplicationEntity>
{
    Task<IEnumerable<OpportunityApplicationEntity>> GetByOpportunityIdAsync(int opportunityId);
    Task<IEnumerable<OpportunityApplicationEntity>> GetPendingByArtistIdAsync(int id);
    Task<IEnumerable<OpportunityApplicationEntity>> GetRecentDeniedByArtistIdAsync(int id);
    Task<(ArtistReadModel, VenueReadModel)?> GetArtistAndVenueByIdAsync(int id);
    Task RejectAllExceptAsync(int opportunityId, int applicationId);
}
