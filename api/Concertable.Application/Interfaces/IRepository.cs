namespace Concertable.Application.Interfaces;

public interface IRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, IIdEntity
{
    Task<TEntity?> GetByIdAsync(int id);
    bool Exists(int id);
}
