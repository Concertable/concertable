using Concertable.Application.Interfaces;
using Concertable.Shared;

namespace Concertable.Application.Diffing;

public abstract class CollectionSyncer<TEntity, TDto, TContext>
    : ICollectionSyncer<TEntity, TDto, TContext>
    where TEntity : class, IIdEntity, IEquatable<TEntity>
    where TDto : ISyncRequest
{
    private readonly IBaseRepository<TEntity> repo;

    protected CollectionSyncer(IBaseRepository<TEntity> repo) => this.repo = repo;

    public async Task SyncAsync(
        TContext context,
        IEnumerable<TEntity> current,
        IEnumerable<TDto> desired)
    {
        var currentList = current.ToList();

        var byKey = desired
            .Where(d => d.Id.HasValue)
            .ToDictionary(d => d.Id!.Value);

        var kept = new HashSet<TEntity>();

        foreach (var entity in currentList)
        {
            if (byKey.TryGetValue(entity.Id, out var dto))
            {
                await UpdateAsync(entity, dto);
                kept.Add(entity);
            }
        }

        var creates = new List<TEntity>();
        foreach (var dto in desired.Where(d => !d.Id.HasValue))
            creates.Add(await CreateAsync(context, dto));

        await repo.AddRangeAsync(creates);
        foreach (var entity in currentList.Except(kept)) await DeleteAsync(entity);
        await repo.SaveChangesAsync();
    }

    protected abstract Task<TEntity> CreateAsync(TContext context, TDto dto);
    protected abstract Task UpdateAsync(TEntity entity, TDto dto);

    protected virtual Task DeleteAsync(TEntity entity)
    {
        repo.Remove(entity);
        return Task.CompletedTask;
    }
}
