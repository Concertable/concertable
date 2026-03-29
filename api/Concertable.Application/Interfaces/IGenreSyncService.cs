using Concertable.Core.Interfaces;

namespace Concertable.Application.Interfaces;

public interface IGenreSyncService
{
    void Sync<T>(ICollection<T> collection, IEnumerable<int> newGenreIds)
        where T : IGenreJoin, new();
}
