using Application.Interfaces;
using Core.Entities.Interfaces;
using Infrastructure.Data.Identity;

namespace Infrastructure.Repositories;

public class Repository<T> : BaseRepository<T>, IRepository<T> where T : class, IEntity
{
    public Repository(ApplicationDbContext context) : base(context) { }

    public async Task<T?> GetByIdAsync(int id) => await context.Set<T>().FindAsync(id);

    public bool Exists(int id) => context.Set<T>().Any(e => e.Id == id);
}
