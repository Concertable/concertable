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
    private readonly IGenreSyncService genreSyncService;
    private readonly IUserService userService;
    private readonly IUnitOfWork unitOfWork;

    public ArtistService(
        IArtistRepository artistRepository,
        IUserRepository userRepository,
        IImageService imageService,
        ICurrentUser currentUser,
        IGenreSyncService genreSyncService,
        IUserService userService,
        IUnitOfWork unitOfWork)
    {
        this.artistRepository = artistRepository;
        this.userRepository = userRepository;
        this.imageService = imageService;
        this.currentUser = currentUser;
        this.genreSyncService = genreSyncService;
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
        var artist = request.ToEntity();
        var user = currentUser.GetEntity();
        artist.UserId = user.Id;
        artist.BannerUrl = await imageService.UploadAsync(request.Banner);

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

        artist.Name = request.Name;
        artist.About = request.About;

        genreSyncService.Sync(artist.ArtistGenres, request.Genres.Select(g => g.Id));

        await userService.UpdateLocationAsync(user, request.Latitude, request.Longitude);

        if (request.Banner is not null)
            artist.BannerUrl = await imageService.ReplaceAsync(request.Banner.File, request.Banner.Url);

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
