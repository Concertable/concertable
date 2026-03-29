using Concertable.Application.Interfaces;
using Concertable.Core.Interfaces;

namespace Concertable.Infrastructure.Services;

public class GenreSyncService : IGenreSyncService
{
    private readonly ICollectionDiffer collectionDiffer;

    public GenreSyncService(ICollectionDiffer collectionDiffer)
    {
        this.collectionDiffer = collectionDiffer;
    }

    public void Sync<T>(ICollection<T> collection, IEnumerable<int> newGenreIds)
        where T : IGenreJoin, new()
    {
        var (toAdd, toRemove) = collectionDiffer.GetChanges(
            collection.Select(x => x.GenreId),
            newGenreIds);

        var removeSet = toRemove.ToHashSet();
        foreach (var item in collection.Where(x => removeSet.Contains(x.GenreId)).ToList())
            collection.Remove(item);

        foreach (var id in toAdd)
            collection.Add(new T { GenreId = id });
    }
}
