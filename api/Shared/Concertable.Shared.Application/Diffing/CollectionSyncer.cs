using Concertable.Application.Interfaces;
using Concertable.Shared;

namespace Concertable.Application.Diffing;

public abstract class CollectionSyncer<TEntity, TDto, TContext>
    : ICollectionSyncer<TEntity, TDto, TContext>
    where TEntity : class, IIdEntity
{
    private readonly IBaseRepository<TEntity> repo;

    protected CollectionSyncer(IBaseRepository<TEntity> repo) => this.repo = repo;

    public async Task SyncAsync(
        TContext context,
        IEnumerable<TEntity> current,
        IEnumerable<TDto> desired)
    {
        var byKey = desired
            .Where(d => DtoKey(d).HasValue)
            .ToDictionary(d => DtoKey(d)!.Value);

        var deletes = new List<TEntity>();

        foreach (var entity in current)
        {
            if (byKey.TryGetValue(entity.Id, out var dto))
                await UpdateAsync(entity, dto);
            else
                deletes.Add(entity);
        }

        var creates = new List<TEntity>();
        foreach (var dto in desired.Where(d => !DtoKey(d).HasValue))
            creates.Add(await CreateAsync(context, dto));

        await repo.AddRangeAsync(creates);
        foreach (var entity in deletes) await DeleteAsync(entity);
        await repo.SaveChangesAsync();
    }

    protected abstract int? DtoKey(TDto dto);
    protected abstract Task<TEntity> CreateAsync(TContext context, TDto dto);
    protected abstract Task UpdateAsync(TEntity entity, TDto dto);

    protected virtual Task DeleteAsync(TEntity entity)
    {
        repo.Remove(entity);
        return Task.CompletedTask;
    }
}
