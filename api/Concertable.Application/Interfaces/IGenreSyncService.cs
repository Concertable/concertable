using Core.Interfaces;

namespace Application.Interfaces;

public interface IGenreSyncService
{
    void Sync<T>(ICollection<T> collection, IEnumerable<int> newGenreIds)
        where T : IGenreJoin, new();
}
