using Concertable.Core.Entities;
using static Concertable.Seeding.Extensions.EntityReflectionExtensions;

namespace Concertable.Seeding.Factories;

public static class PreferenceFactory
{
    public static PreferenceEntity Create(Guid userId, double radiusKm)
        => New<PreferenceEntity>()
            .With(nameof(PreferenceEntity.UserId), userId)
            .With(nameof(PreferenceEntity.RadiusKm), radiusKm);
}
