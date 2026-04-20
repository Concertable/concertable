namespace Concertable.Application.Interfaces;

public interface IModuleSeeder
{
    int Order { get; }
    Task SeedAsync(CancellationToken ct = default);
}

public interface IDevSeeder : IModuleSeeder { }
public interface ITestSeeder : IModuleSeeder { }
