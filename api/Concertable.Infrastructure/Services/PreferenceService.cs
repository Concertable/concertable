using Concertable.Application.DTOs;
using Concertable.Application.Requests;
using Concertable.Application.Interfaces;
using Concertable.Application.Mappers;
using Concertable.Core.Entities;
using Concertable.Core.Exceptions;

namespace Concertable.Infrastructure.Services;

public class PreferenceService : IPreferenceService
{
    private readonly IPreferenceRepository preferenceRepository;
    private readonly ICurrentUser currentUser;
    private readonly IGenreSyncService genreSyncService;

    public PreferenceService(
        IPreferenceRepository preferenceRepository,
        ICurrentUser currentUser,
        IGenreSyncService genreSyncService)
    {
        this.preferenceRepository = preferenceRepository;
        this.currentUser = currentUser;
        this.genreSyncService = genreSyncService;
    }

    public async Task<PreferenceDto> CreateAsync(CreatePreferenceRequest request, Guid? userId = null)
    {
        var resolvedUserId = userId ?? currentUser.GetId();

        var preference = request.ToEntity();
        preference.UserId = resolvedUserId;

        preferenceRepository.Update(preference);
        await preferenceRepository.SaveChangesAsync();

        return preference.ToDto();
    }

    public async Task<IEnumerable<PreferenceDto>> GetAsync()
    {
        var preferences = await preferenceRepository.GetAllAsync();
        return preferences.ToDtos();
    }

    public async Task<PreferenceDto?> GetByUserIdAsync(Guid userId)
    {
        var preference = await preferenceRepository.GetByUserIdAsync(userId);
        return preference?.ToDto();
    }

    public async Task<PreferenceDto?> GetByUserAsync()
    {
        var user = currentUser.Get();
        return await GetByUserIdAsync(user.Id);
    }

    public async Task<PreferenceDto> UpdateAsync(PreferenceDto preferenceDto)
    {
        var preference = await preferenceRepository.GetByIdAsync(preferenceDto.Id)
            ?? throw new NotFoundException("Preference not found");
        var userId = currentUser.Get().Id;

        if (userId != preference.User.Id)
            throw new UnauthorizedAccessException("You do not own this preference");

        genreSyncService.Sync(
            preference.GenrePreferences,
            preferenceDto.Genres.Select(g => g.Id));

        preference.RadiusKm = preferenceDto.RadiusKm;

        preferenceRepository.Update(preference);
        await preferenceRepository.SaveChangesAsync();

        var updatedPreference = await preferenceRepository.GetByIdAsync(preference.Id);
        return updatedPreference!.ToDto();
    }
}
