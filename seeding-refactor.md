# Seeding Refactor — Module-Owned Seeders

## Why

Identity module is fully extracted into `IdentityDbContext`. The only reason `ApplicationDbContext` still has a `Users` DbSet is seeding. Removing it requires moving user seeding into the Identity module itself.

More broadly: as each module is extracted, its seeding logic must move with it. The top-level initializers should orchestrate, not author.

## The Pattern

```csharp
// Concertable.Application (no EF dep)
public interface IModuleSeeder
{
    int Order { get; }
    Task SeedAsync(CancellationToken ct = default);
}

public interface IDevSeeder : IModuleSeeder { }
public interface ITestSeeder : IModuleSeeder { }
```

- `IDevSeeder` — large, realistic fake data. Only registered in dev.
- `ITestSeeder` — deterministic, minimal. Only registered in test.
- `SeedData` carries entity references between seeders (cross-module ID passing).
- `DevDbInitializer` / `TestDbInitializer` become thin: resolve the right interface, order by `Order`, call each.

## What To Build

### 1. Add abstractions
- Add `IModuleSeeder`, `IDevSeeder`, `ITestSeeder` to `Concertable.Application/Interfaces/`

### 2. Identity module seeders
- `Concertable.Identity.Infrastructure/Data/Seeders/IdentityDevSeeder.cs`
  - Implements `IDevSeeder`, `Order = 0`
  - Moves all user seeding logic from `DevDbInitializer`
  - Uses `IdentityDbContext`, populates `SeedData` with user references
- `Concertable.Identity.Infrastructure/Data/Seeders/IdentityTestSeeder.cs`
  - Implements `ITestSeeder`, `Order = 0`
  - Moves all user seeding logic from `TestDbInitializer`
  - Uses `IdentityDbContext`, populates `SeedData` with user references

### 3. Register seeders
- Identity module's DI extension registers `IdentityDevSeeder` as `IDevSeeder` (dev env only)
- Identity module's DI extension registers `IdentityTestSeeder` as `ITestSeeder` (test env only)

### 4. Update initializers
- `DevDbInitializer`: inject `IEnumerable<IDevSeeder>`, remove user seeding block, call seeders first
- `TestDbInitializer`: inject `IEnumerable<ITestSeeder>`, remove user seeding block, call seeders first
- Cross-module ID resolution (`usersByEmail` dictionary) moves into `IdentityDevSeeder` — it populates `SeedData` so subsequent blocks still work

### 5. Clean up ApplicationDbContext
- Remove `Users` DbSet from `ApplicationDbContext` — no longer needed for anything

## Done When
- `ApplicationDbContext` has no `Users` DbSet
- User seeding lives entirely in `Concertable.Identity.Infrastructure`
- Both initializers are orchestrators only
