using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Entities;
using Concertable.Application.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Infrastructure.Services;

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
        var user = currentUser.Get();
        var opportunity = await opportunityRepository.GetWithVenueByIdAsync(opportunityId);
        return opportunity != null && opportunity.Venue?.UserId == user.Id;
    }

    public async Task<bool> OwnsOpportunityByApplicationId(int applicationId)
    {
        var user = currentUser.Get();
        var opportunity = await opportunityRepository.GetByApplicationIdAsync(applicationId);
        return opportunity != null && opportunity.Venue?.UserId == user.Id;
    }

    public async Task<bool> OwnsArtistAsync(int artistId)
    {
        var id = await artistService.GetIdForCurrentUserAsync();

        return id == artistId;
    }
}
