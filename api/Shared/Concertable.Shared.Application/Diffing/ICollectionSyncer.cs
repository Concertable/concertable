using Concertable.Shared;

namespace Concertable.Application.Diffing;

public interface ICollectionSyncer<TEntity, TDto, TContext>
    where TEntity : class, IIdEntity, IEquatable<TEntity>
    where TDto : ISyncRequest
{
    Task SyncAsync(TContext context, IEnumerable<TEntity> current, IEnumerable<TDto> desired);
}
