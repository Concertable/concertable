using Concertable.Application.Interfaces;
using Concertable.Core.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Concertable.Infrastructure.Data;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser currentUser;

    public AuditInterceptor(ICurrentUser currentUser)
    {
        this.currentUser = currentUser;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var entries = eventData.Context!.ChangeTracker.Entries<IAuditable>();
        var now = DateTime.UtcNow;
        var userId = currentUser.Id?.ToString() ?? string.Empty;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = userId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModifiedAt = now;
                entry.Entity.LastModifiedBy = userId;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
