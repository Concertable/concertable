using Concertable.Application.Interfaces;
using Concertable.Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Auth.Infrastructure.Data.Seeders;

internal class AuthTestSeeder : ITestSeeder
{
    public int Order => 8;

    private readonly AuthDbContext context;

    public AuthTestSeeder(AuthDbContext context)
    {
        this.context = context;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public Task SeedAsync(CancellationToken ct = default) => Task.CompletedTask;
}
