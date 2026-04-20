namespace Concertable.Application.Interfaces;

public interface IOwnershipService
{
    Task<bool> OwnsVenueAsync(int venueId);
    Task<bool> OwnsOpportunityAsync(int opportunityId);
    Task<bool> OwnsArtistAsync(int applicationId);
    Task<bool> OwnsOpportunityByApplicationId(int applicationId);
}
