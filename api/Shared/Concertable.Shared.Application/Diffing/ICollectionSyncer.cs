using Concertable.Shared;

namespace Concertable.Application.Diffing;

public interface ICollectionSyncer<TEntity, TDto>
    where TEntity : class, IIdEntity, IEquatable<TEntity>
    where TDto : ISyncRequest
{
    Task SyncAsync(int ownerId, IEnumerable<TEntity> current, IEnumerable<TDto> desired);
}
