using Concertable.Shared;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Data.Infrastructure.Data;

internal class SharedDbContext(DbContextOptions<SharedDbContext> options)
    : DbContextBase(options)
{
    public DbSet<GenreEntity> Genres => Set<GenreEntity>();
}
