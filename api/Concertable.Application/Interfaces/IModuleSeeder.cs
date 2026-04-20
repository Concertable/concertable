namespace Concertable.Application.Interfaces;

public interface IModuleSeeder
{
    int Order { get; }
    Task MigrateAsync(CancellationToken ct = default) => Task.CompletedTask;
    Task SeedAsync(CancellationToken ct = default);
}

public interface IDevSeeder : IModuleSeeder { }
public interface ITestSeeder : IModuleSeeder { }
