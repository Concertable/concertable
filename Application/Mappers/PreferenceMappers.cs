using Application.DTOs;
using Application.Requests;
using Core.Entities;

namespace Application.Mappers;

public static class PreferenceMappers
{
    public static Preference ToEntity(this CreatePreferenceRequest request) => new()
    {
        RadiusKm = request.RadiusKm,
        GenrePreferences = request.Genres.Select(g => new GenrePreference { GenreId = g.Id }).ToList()
    };

    public static PreferenceDto ToDto(this Preference preference) => new()
    {
        Id = preference.Id,
        RadiusKm = (int)preference.RadiusKm,
        User = preference.User.ToDto(),
        Genres = preference.GenrePreferences.Select(gp => gp.Genre.ToDto())
    };

    public static IEnumerable<PreferenceDto> ToDtos(this IEnumerable<Preference> preferences) =>
                preferences.Select(p => p.ToDto());
}
