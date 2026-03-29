using Concertable.Application.Interfaces;

namespace Concertable.Infrastructure.Services;

public class CollectionDiffer : ICollectionDiffer
{
    public (IReadOnlyList<int> ToAdd, IReadOnlyList<int> ToRemove) GetChanges(
        IEnumerable<int> existingIds,
        IEnumerable<int> newIds)
    {
        var existing = existingIds.ToHashSet();
        var incoming = newIds.ToHashSet();
        return (incoming.Except(existing).ToList(), existing.Except(incoming).ToList());
    }
}
