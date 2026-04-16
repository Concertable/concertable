using Concertable.Core.Exceptions;
using Concertable.Core.Interfaces;

namespace Concertable.Core.Extensions;

public static class GenreJoinExtensions
{
    public static void SyncGenres<TGenreJoin>(
        this IHasGenreJoins<TGenreJoin> entity,
        IEnumerable<int> genreIds)
        where TGenreJoin : class, IGenreJoin, new()
    {
        var newGenreIds = genreIds.ToHashSet();

        if (newGenreIds.Count == 0)
            throw new DomainException("At least one genre is required.");

        entity.GenreJoins.RemoveWhere(x => !newGenreIds.Contains(x.GenreId));

        var existingGenreIds = entity.GenreJoins
            .Select(x => x.GenreId)
            .ToHashSet();

        foreach (var genreId in newGenreIds)
        {
            if (!existingGenreIds.Contains(genreId))
                entity.GenreJoins.Add(new TGenreJoin { GenreId = genreId });
        }
    }
}
