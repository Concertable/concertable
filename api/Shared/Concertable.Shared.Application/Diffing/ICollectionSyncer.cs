using Concertable.Shared;

namespace Concertable.Application.Diffing;

public interface ICollectionSyncer<TEntity, TDto, TContext>
    where TEntity : class, IIdEntity
{
    Task SyncAsync(TContext context, IEnumerable<TEntity> current, IEnumerable<TDto> desired);
}
