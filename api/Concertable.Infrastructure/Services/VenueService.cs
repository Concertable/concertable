using Concertable.Core.Entities;
using Concertable.Application.Interfaces;
using Concertable.Application.DTOs;
using Concertable.Application.Mappers;
using Concertable.Application.Requests;
using Concertable.Application.Responses;
using Concertable.Core.Enums;
using Concertable.Core.Exceptions;

namespace Concertable.Infrastructure.Services;

public class VenueService : IVenueService
{
    private readonly IVenueRepository venueRepository;
    private readonly IUserRepository userRepository;
    private readonly IImageService imageService;
    private readonly ICurrentUser currentUser;
    private readonly IUserService userService;
    private readonly IUnitOfWork unitOfWork;

    public VenueService(
        IVenueRepository venueRepository,
        IUserRepository userRepository,
        IImageService imageService,
        ICurrentUser currentUser,
        IUserService userService,
        IUnitOfWork unitOfWork)
    {
        this.venueRepository = venueRepository;
        this.userRepository = userRepository;
        this.imageService = imageService;
        this.currentUser = currentUser;
        this.userService = userService;
        this.unitOfWork = unitOfWork;
    }

    public async Task<VenueDetailsResponse> GetDetailsByIdAsync(int id)
    {
        var dto = await venueRepository.GetDetailsByIdAsync(id)
            ?? throw new NotFoundException("Venue not found");
        return dto.ToDetailsResponse();
    }

    public async Task<VenueDto> CreateAsync(CreateVenueRequest request)
    {
        var venue = request.ToEntity();
        var user = currentUser.GetEntity<VenueManagerEntity>();

        venue.UserId = user.Id;
        venue.BannerUrl = await imageService.UploadAsync(request.Banner);

        await userService.UpdateLocationAsync(user, request.Latitude, request.Longitude);

        var createdVenue = await venueRepository.AddAsync(venue);
        createdVenue.User = user;
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync();

        return createdVenue.ToDto();
    }

    public async Task<VenueDto> UpdateAsync(int id, UpdateVenueRequest request)
    {
        var venue = await venueRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Venue not found");
        var user = currentUser.GetEntity();

        if (venue.UserId != user.Id)
            throw new ForbiddenException("You do not own this venue");

        venue.Name = request.Name;
        venue.About = request.About;
        venue.Approved = request.Approved;

        await userService.UpdateLocationAsync(user, request.Latitude, request.Longitude);

        if (request.Banner is not null)
            venue.BannerUrl = await imageService.ReplaceAsync(request.Banner.File, request.Banner.Url);

        if (request.Avatar is not null)
            user.Avatar = await imageService.ReplaceAsync(request.Avatar, user.Avatar);

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync();

        return venue.ToDto();
    }

    public async Task<VenueDetailsResponse> GetDetailsForCurrentUserAsync()
    {
        var user = currentUser.Get();
        var dto = await venueRepository.GetDetailsByUserIdAsync(user.Id)
            ?? throw new NotFoundException("Venue not found");
        return dto.ToDetailsResponse();
    }

    public async Task<int> GetIdForCurrentUserAsync()
    {
        var user = currentUser.Get();
        int? id = await venueRepository.GetIdByUserIdAsync(user.Id);

        if (id is null)
            throw new ForbiddenException("You do not own a Venue");

        return id.Value;
    }

    public async Task ApproveAsync(int id)
    {
        var venue = await venueRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Venue not found");

        venue.Approved = true;
        await unitOfWork.SaveChangesAsync();
    }
}
