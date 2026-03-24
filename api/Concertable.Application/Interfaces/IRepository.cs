using Core.Entities.Interfaces;

namespace Application.Interfaces;

public interface IRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, IEntity
{
    Task<TEntity?> GetByIdAsync(int id);
    bool Exists(int id);
}
