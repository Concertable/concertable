using Microsoft.EntityFrameworkCore;

namespace Concertable.Data.Infrastructure;

public abstract class DbContextBase(DbContextOptions options) : DbContext(options)
{
}
