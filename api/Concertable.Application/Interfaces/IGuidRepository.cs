using Core.Entities.Interfaces;

namespace Application.Interfaces;

public interface IGuidRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, IGuidEntity
{
    Task<TEntity?> GetByIdAsync(Guid id);
    bool Exists(Guid id);
}
