using System.Collections.Concurrent;
using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Concertable.Seeding.Identity;

public sealed class SeedingIdentityInterceptor : DbCommandInterceptor
{
    private static readonly Regex InsertHeaderRegex = new(
        @"INSERT\s+INTO\s+(?<table>\[?[\w]+\]?(?:\.\[?[\w]+\]?)?)\s*\((?<cols>[^)]*)\)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly ConcurrentDictionary<Type, Dictionary<string, string>> identityTableCache = new();

    private readonly SeedingScope scope;

    public SeedingIdentityInterceptor(SeedingScope scope)
    {
        this.scope = scope;
    }

    public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        Rewrite(command, eventData);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Rewrite(command, eventData);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        Rewrite(command, eventData);
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        Rewrite(command, eventData);
        return ValueTask.FromResult(result);
    }

    private void Rewrite(DbCommand command, CommandEventData eventData)
    {
        if (!scope.IsActive) return;
        if (eventData.Context is null) return;

        var sql = command.CommandText;
        if (sql.IndexOf("INSERT INTO", StringComparison.OrdinalIgnoreCase) < 0)
            return;

        var context = eventData.Context;
        var identityTables = identityTableCache.GetOrAdd(
            context.GetType(),
            contextType => BuildIdentityTableMap(context.Model));

        if (identityTables.Count == 0) return;

        var wrappedTables = new List<string>();

        foreach (Match m in InsertHeaderRegex.Matches(sql))
        {
            var table = Normalize(m.Groups["table"].Value);
            if (!identityTables.TryGetValue(table, out var identityColumn))
                continue;
            if (!ColumnListIncludes(m.Groups["cols"].Value, identityColumn))
                continue;
            if (!wrappedTables.Contains(table))
                wrappedTables.Add(table);
        }

        if (wrappedTables.Count == 0) return;

        var on = string.Concat(wrappedTables.Select(t => $"SET IDENTITY_INSERT {t} ON;\n"));
        var off = string.Concat(wrappedTables.Select(t => $"SET IDENTITY_INSERT {t} OFF;\n"));
        command.CommandText = on + sql + "\n" + off;
    }

    private static bool ColumnListIncludes(string cols, string identityColumn)
    {
        foreach (var raw in cols.Split(','))
        {
            var trimmed = raw.Trim().Trim('[', ']');
            if (trimmed.Equals(identityColumn, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static string Normalize(string raw)
    {
        var parts = raw.Split('.');
        return string.Join('.', parts.Select(p => p.StartsWith('[') ? p : $"[{p.Trim('[', ']')}]"));
    }

    private static Dictionary<string, string> BuildIdentityTableMap(IModel model)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entity in model.GetEntityTypes())
        {
            var table = entity.GetTableName();
            if (string.IsNullOrEmpty(table)) continue;

            if (entity.GetRootType().GetTableName() != table) continue;

            var key = entity.FindPrimaryKey();
            if (key is null) continue;

            var identityProp = key.Properties.FirstOrDefault(p =>
                p.GetValueGenerationStrategy() == SqlServerValueGenerationStrategy.IdentityColumn);
            if (identityProp is null) continue;

            var schema = entity.GetSchema();
            var qualified = schema is null ? $"[{table}]" : $"[{schema}].[{table}]";
            map[qualified] = identityProp.GetColumnName();
        }
        return map;
    }
}
