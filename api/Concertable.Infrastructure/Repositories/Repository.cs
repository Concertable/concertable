using Concertable.Application.Interfaces;
using Concertable.Infrastructure.Data;

namespace Concertable.Infrastructure.Repositories;

public class Repository<T> : BaseRepository<T>, IRepository<T> where T : class, IIdEntity
{
    public Repository(ApplicationDbContext context) : base(context) { }

    public async Task<T?> GetByIdAsync(int id) => await context.Set<T>().FindAsync(id);

    public bool Exists(int id) => context.Set<T>().Any(e => e.Id == id);
}
