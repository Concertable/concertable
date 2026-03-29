using Concertable.Core.Entities.Interfaces;

namespace Concertable.Application.Interfaces;

public interface IRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, IEntity
{
    Task<TEntity?> GetByIdAsync(int id);
    bool Exists(int id);
}
