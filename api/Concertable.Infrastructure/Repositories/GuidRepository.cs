using Concertable.Application.Interfaces;
using Concertable.Infrastructure.Data;

namespace Concertable.Infrastructure.Repositories;

public class GuidRepository<T> : BaseRepository<T>, IGuidRepository<T> where T : class, IGuidEntity
{
    public GuidRepository(ApplicationDbContext context) : base(context) { }

    public async Task<T?> GetByIdAsync(Guid id) => await context.Set<T>().FindAsync(id);

    public bool Exists(Guid id) => context.Set<T>().Any(e => e.Id == id);
}
