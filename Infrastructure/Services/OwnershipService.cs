using Application.Interfaces;
using Core.Entities;
using Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class OwnershipService : IOwnershipService
{
    private readonly ICurrentUser currentUser;
    private readonly IVenueService venueService;
    private readonly IArtistService artistService;
    private readonly IListingRepository listingRepository;

    public OwnershipService(
        ICurrentUser currentUser,
        IVenueService venueService,
        IArtistService artistService,
        IListingRepository listingRepository)
    {
        this.currentUser = currentUser;
        this.venueService = venueService;
        this.artistService = artistService;
        this.listingRepository = listingRepository;
    }

    public async Task<bool> OwnsVenueAsync(int venueId)
    {
        var id = await venueService.GetIdForCurrentUserAsync();

        return id == venueId;
    }

    public async Task<bool> OwnsListingAsync(int listingId)
    {
        var user = currentUser.Get();
        var listing = await listingRepository.GetWithVenueByIdAsync(listingId);
        return listing != null && listing.Venue?.UserId == user.Id;
    }

    public async Task<bool> OwnsListingByApplicationId(int applicationId)
    {
        var user = currentUser.Get();
        var listing = await listingRepository.GetByApplicationIdAsync(applicationId);
        return listing != null && listing.Venue?.UserId == user.Id;
    }

    public async Task<bool> OwnsArtistAsync(int artistId)
    {
        var id = await artistService.GetIdForCurrentUserAsync();

        return id == artistId;
    }
}
