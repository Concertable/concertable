using Application.DTOs;
using Application.Requests;
using Application.Interfaces;
using Application.Mappers;
using Core.Entities;
using Core.Exceptions;

namespace Infrastructure.Services;

public class PreferenceService : IPreferenceService
{
    private readonly IPreferenceRepository preferenceRepository;
    private readonly ICurrentUser currentUser;

    public PreferenceService(
        IPreferenceRepository preferenceRepository,
        ICurrentUser currentUser)
    {
        this.preferenceRepository = preferenceRepository;
        this.currentUser = currentUser;
    }

    public async Task<PreferenceDto> CreateAsync(CreatePreferenceRequest request, int? userId = null)
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

    public async Task<PreferenceDto?> GetByUserIdAsync(int userId)
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

        preference.GenrePreferences.Clear();
        foreach (var genreDto in preferenceDto.Genres)
        {
            preference.GenrePreferences.Add(new GenrePreferenceEntity
            {
                PreferenceId = preference.Id,
                GenreId = genreDto.Id
            });
        }

        preference.RadiusKm = preferenceDto.RadiusKm;

        preferenceRepository.Update(preference);
        await preferenceRepository.SaveChangesAsync();

        var updatedPreference = await preferenceRepository.GetByIdAsync(preference.Id);
        return updatedPreference!.ToDto();
    }
}
