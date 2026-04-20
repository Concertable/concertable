using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Entities;
using Concertable.Application.Exceptions;
using Concertable.Identity.Contracts;

namespace Concertable.Identity.Infrastructure.Services;

public class OwnershipService : IOwnershipService
{
    private readonly ICurrentUser currentUser;
    private readonly IVenueService venueService;
    private readonly IArtistService artistService;
    private readonly IOpportunityRepository opportunityRepository;

    public OwnershipService(
        ICurrentUser currentUser,
        IVenueService venueService,
        IArtistService artistService,
        IOpportunityRepository opportunityRepository)
    {
        this.currentUser = currentUser;
        this.venueService = venueService;
        this.artistService = artistService;
        this.opportunityRepository = opportunityRepository;
    }

    public async Task<bool> OwnsVenueAsync(int venueId)
    {
        var id = await venueService.GetIdForCurrentUserAsync();
        return id == venueId;
    }

    public async Task<bool> OwnsOpportunityAsync(int opportunityId)
    {
        var userId = currentUser.GetId();
        var opportunity = await opportunityRepository.GetWithVenueByIdAsync(opportunityId);
        return opportunity != null && opportunity.Venue?.UserId == userId;
    }

    public async Task<bool> OwnsOpportunityByApplicationId(int applicationId)
    {
        var userId = currentUser.GetId();
        var opportunity = await opportunityRepository.GetByApplicationIdAsync(applicationId);
        return opportunity != null && opportunity.Venue?.UserId == userId;
    }

    public async Task<bool> OwnsArtistAsync(int artistId)
    {
        var id = await artistService.GetIdForCurrentUserAsync();
        return id == artistId;
    }
}
