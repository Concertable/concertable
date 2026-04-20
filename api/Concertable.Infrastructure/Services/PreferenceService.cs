using Concertable.Application.DTOs;
using Concertable.Application.Requests;
using Concertable.Application.Interfaces;
using Concertable.Application.Mappers;
using Concertable.Core.Entities;
using Concertable.Application.Exceptions;
using Concertable.Identity.Contracts;

namespace Concertable.Infrastructure.Services;

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

    public async Task<PreferenceDto> CreateAsync(CreatePreferenceRequest request, Guid? userId = null)
    {
        var resolvedUserId = userId ?? currentUser.GetId();
        var preference = PreferenceEntity.Create(resolvedUserId, request.RadiusKm, request.Genres.Select(g => g.Id));

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
        return await GetByUserIdAsync(currentUser.GetId());
    }

    public async Task<PreferenceDto> UpdateAsync(PreferenceDto preferenceDto)
    {
        var preference = await preferenceRepository.GetByIdAsync(preferenceDto.Id)
            ?? throw new NotFoundException("Preference not found");
        var userId = currentUser.GetId();

        if (userId != preference.User.Id)
            throw new UnauthorizedAccessException("You do not own this preference");

        preference.Update(preferenceDto.RadiusKm, preferenceDto.Genres.Select(g => g.Id));

        preferenceRepository.Update(preference);
        await preferenceRepository.SaveChangesAsync();

        var updatedPreference = await preferenceRepository.GetByIdAsync(preference.Id);
        return updatedPreference!.ToDto();
    }
}
