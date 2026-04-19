using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Data;

public class ReadDbContext : ApplicationDbContext
{
    public ReadDbContext(DbContextOptions<ReadDbContext> options) : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("ReadDbContext is read-only.");

    public override int SaveChanges()
        => throw new NotSupportedException("ReadDbContext is read-only.");

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
        => throw new NotSupportedException("ReadDbContext is read-only.");
}
