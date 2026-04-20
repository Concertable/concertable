using Concertable.Shared;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Data.Infrastructure;

public abstract class DbContextBase(DbContextOptions options, IDomainEventDispatcher? dispatcher = null)
    : DbContext(options)
{
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var events = ChangeTracker.Entries<IEventRaiser>()
            .SelectMany(e => e.Entity.DomainEvents).ToList();

        foreach (var entry in ChangeTracker.Entries<IEventRaiser>())
            entry.Entity.ClearDomainEvents();

        var result = await base.SaveChangesAsync(ct);

        if (dispatcher is not null)
            await dispatcher.DispatchAsync(events, ct);

        return result;
    }
}
