using Concertable.Core.Entities;
using Concertable.Application.Interfaces;
using Concertable.Application.DTOs;
using Concertable.Application.Mappers;
using Concertable.Application.Requests;
using Concertable.Application.Exceptions;

namespace Concertable.Infrastructure.Services;

public class ArtistService : IArtistService
{
    private readonly IArtistRepository artistRepository;
    private readonly IUserRepository userRepository;
    private readonly IImageService imageService;
    private readonly ICurrentUser currentUser;
    private readonly IUserService userService;
    private readonly IUnitOfWork unitOfWork;

    public ArtistService(
        IArtistRepository artistRepository,
        IUserRepository userRepository,
        IImageService imageService,
        ICurrentUser currentUser,
        IUserService userService,
        IUnitOfWork unitOfWork)
    {
        this.artistRepository = artistRepository;
        this.userRepository = userRepository;
        this.imageService = imageService;
        this.currentUser = currentUser;
        this.userService = userService;
        this.unitOfWork = unitOfWork;
    }

    public async Task<ArtistDto> GetDetailsForCurrentUserAsync()
    {
        var user = currentUser.Get();
        return await artistRepository.GetDtoByUserIdAsync(user.Id)
            ?? throw new NotFoundException("Artist not found");
    }

    public async Task<ArtistDto> GetDetailsByIdAsync(int id)
    {
        return await artistRepository.GetDtoByIdAsync(id)
            ?? throw new NotFoundException("Artist not found");
    }

    public async Task<ArtistDto> CreateAsync(CreateArtistRequest request)
    {
        var user = currentUser.GetEntity();
        var bannerUrl = await imageService.UploadAsync(request.Banner);
        var artist = ArtistEntity.Create(user.Id, request.Name, request.About, bannerUrl, request.Genres.Select(g => g.Id));

        await userService.UpdateLocationAsync(user, request.Latitude, request.Longitude);

        var createdArtist = await artistRepository.AddAsync(artist);
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync();

        return createdArtist.ToDto();
    }

    public async Task<ArtistDto> UpdateAsync(int id, UpdateArtistRequest request)
    {
        var artist = await artistRepository.GetFullByIdAsync(id)
            ?? throw new NotFoundException("Artist not found");

        var user = currentUser.GetEntity();
        if (artist.UserId != user.Id)
            throw new ForbiddenException("You do not own this Artist");

        var bannerUrl = request.Banner is not null
            ? await imageService.ReplaceAsync(request.Banner.File, request.Banner.Url)
            : artist.BannerUrl;

        artist.Update(request.Name, request.About, bannerUrl, request.Genres.Select(g => g.Id));

        await userService.UpdateLocationAsync(user, request.Latitude, request.Longitude);

        if (request.Avatar is not null)
            user.Avatar = await imageService.ReplaceAsync(request.Avatar, user.Avatar);

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync();

        return artist.ToDto();
    }

    public async Task<int> GetIdForCurrentUserAsync()
    {
        var user = currentUser.Get();
        int? id = await artistRepository.GetIdByUserIdAsync(user.Id);

        if (id is null)
            throw new ForbiddenException("You do not own an Artist");

        return id.Value;
    }
}
