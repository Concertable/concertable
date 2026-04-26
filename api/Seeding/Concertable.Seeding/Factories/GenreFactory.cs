using static Concertable.Seeding.Extensions.EntityReflectionExtensions;

namespace Concertable.Seeding.Factories;

public static class GenreFactory
{
    public static GenreEntity Create(string name)
        => New<GenreEntity>()
            .With(nameof(GenreEntity.Name), name);
}
