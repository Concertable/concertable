using Concertable.Core.Entities;
using Concertable.Application.Interfaces;
using Concertable.Application.DTOs;
using Concertable.Application.Mappers;
using Concertable.Application.Requests;
using Concertable.Core.Enums;
using Concertable.Application.Exceptions;

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

    public async Task<VenueDto> GetDetailsByIdAsync(int id)
    {
        return await venueRepository.GetDtoByIdAsync(id)
            ?? throw new NotFoundException("Venue not found");
    }

    public async Task<VenueDto> CreateAsync(CreateVenueRequest request)
    {
        var user = currentUser.GetEntity<VenueManagerEntity>();
        var bannerUrl = await imageService.UploadAsync(request.Banner);
        var venue = VenueEntity.Create(user.Id, request.Name, request.About, bannerUrl);

        await userService.UpdateLocationAsync(user, request.Latitude, request.Longitude);

        var createdVenue = await venueRepository.AddAsync(venue);
        createdVenue.User = user;
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync();

        return createdVenue.ToDto();
    }

    public async Task<VenueDto> UpdateAsync(int id, UpdateVenueRequest request)
    {
        var venue = await venueRepository.GetFullByIdAsync(id)
            ?? throw new NotFoundException("Venue not found");
        var user = currentUser.GetEntity();

        if (venue.UserId != user.Id)
            throw new ForbiddenException("You do not own this venue");

        var bannerUrl = request.Banner is not null
            ? await imageService.ReplaceAsync(request.Banner.File, request.Banner.Url)
            : venue.BannerUrl;

        venue.Update(request.Name, request.About, bannerUrl);

        await userService.UpdateLocationAsync(user, request.Latitude, request.Longitude);

        if (request.Avatar is not null)
            user.Avatar = await imageService.ReplaceAsync(request.Avatar, user.Avatar);

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync();

        return venue.ToDto();
    }

    public async Task<VenueDto> GetDetailsForCurrentUserAsync()
    {
        var user = currentUser.Get();
        return await venueRepository.GetDtoByUserIdAsync(user.Id)
            ?? throw new NotFoundException("Venue not found");
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

        venue.Approve();
        await unitOfWork.SaveChangesAsync();
    }
}
