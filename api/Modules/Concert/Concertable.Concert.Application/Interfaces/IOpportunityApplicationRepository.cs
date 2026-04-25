namespace Concertable.Concert.Application.Interfaces;

internal interface IOpportunityApplicationRepository : IIdRepository<OpportunityApplicationEntity>
{
    Task<IEnumerable<OpportunityApplicationEntity>> GetByOpportunityIdAsync(int opportunityId);
    Task<IEnumerable<OpportunityApplicationEntity>> GetPendingByArtistIdAsync(int id);
    Task<IEnumerable<OpportunityApplicationEntity>> GetRecentDeniedByArtistIdAsync(int id);
    Task<(ArtistReadModel, VenueReadModel)?> GetArtistAndVenueByIdAsync(int id);
    Task RejectAllExceptAsync(int opportunityId, int applicationId);
    Task<int?> GetContractIdByApplicationIdAsync(int applicationId);
}
