namespace Application.Interfaces;

public interface ICollectionDiffer
{
    (IReadOnlyList<int> ToAdd, IReadOnlyList<int> ToRemove) GetChanges(
        IEnumerable<int> existingIds,
        IEnumerable<int> newIds);
}
