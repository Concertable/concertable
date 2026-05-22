using System.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Seeding.Extensions;

public static class IdentityInsertExtensions
{
    public static async Task SaveWithIdsAsync<TEntity>(
        this DbContext context,
        CancellationToken ct = default)
        where TEntity : class
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity))
            ?? throw new InvalidOperationException(
                $"{typeof(TEntity).Name} is not mapped on {context.GetType().Name}.");

        var schema = entityType.GetSchema();
        var table = schema is not null
            ? $"[{schema}].[{entityType.GetTableName()}]"
            : $"[{entityType.GetTableName()}]";
        var enable = $"SET IDENTITY_INSERT {table} ON";
        var disable = $"SET IDENTITY_INSERT {table} OFF";

        var connection = context.Database.GetDbConnection();
        var wasClosed = connection.State != ConnectionState.Open;
        if (wasClosed)
            await context.Database.OpenConnectionAsync(ct);
        try
        {
            await context.Database.ExecuteSqlRawAsync(enable, ct);
            await context.SaveChangesAsync(ct);
            await context.Database.ExecuteSqlRawAsync(disable, ct);
        }
        finally
        {
            if (wasClosed)
                await context.Database.CloseConnectionAsync();
        }
    }
}
