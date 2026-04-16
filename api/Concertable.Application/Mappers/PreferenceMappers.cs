using Concertable.Application.DTOs;
using Concertable.Core.Entities;

namespace Concertable.Application.Mappers;

public static class PreferenceMappers
{
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
