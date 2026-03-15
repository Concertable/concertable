using Application.DTOs;
using Application.Requests;
using Core.Entities;

namespace Application.Mappers;

public static class PreferenceMappers
{
    public static PreferenceEntity ToEntity(this CreatePreferenceRequest request) => new()
    {
        RadiusKm = request.RadiusKm,
        GenrePreferences = request.Genres.Select(g => new GenrePreferenceEntity { GenreId = g.Id }).ToList()
    };

    public static PreferenceDto ToDto(this PreferenceEntity preference) => new()
    {
        Id = preference.Id,
        RadiusKm = (int)preference.RadiusKm,
        User = preference.User.ToDto(),
        Genres = preference.GenrePreferences.Select(gp => gp.Genre.ToDto())
    };

    public static IEnumerable<PreferenceDto> ToDtos(this IEnumerable<PreferenceEntity> preferences) =>
                preferences.Select(p => p.ToDto());
}
