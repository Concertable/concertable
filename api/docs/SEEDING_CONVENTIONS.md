# Seeding Conventions

## IDevSeeder vs ITestSeeder

- `IDevSeeder` runs in **dev and E2E** environments via `DevDbInitializer`.
- `ITestSeeder` runs in **integration tests only** — never in E2E or dev startup.

Never confuse them. If your E2E fixture is missing data, the fix is always in an `IDevSeeder`, not an `ITestSeeder`.

## Never seed event-driven data

A large category of data exists solely because integration events were processed. Do not create seeders for this data — ever. Fix the event flow instead.

Examples of data that must **not** be manually seeded:

- **Read-model projections** — `VenueReadModel`, `ArtistReadModel`, and any other `XReadModel` in a concert/search context. These are populated by `XChangedEvent` handlers. If the table is empty at seed time, it means the event hasn't been processed yet — that is correct and expected.
- **Stripe payout accounts** — provisioned when `CredentialRegisteredEvent` fires on user registration.
- **Payment accounts / external service records** — anything provisioned by a handler reacting to a domain event.

The rule: if a record exists because *something happened* (an event was raised and handled), there is no seeder for it. If you find yourself writing `context.XReadModels.AddRange(...)` in a seeder, stop — you are bypassing the event flow.

## Write models must not have FK constraints to read models

A navigation property from a write-model entity to a read-model projection creates a database FK from the write table to the read table. This is always wrong:

- The read table may be empty at seed time (events not yet processed).
- It couples the write model's persistence to the read model's availability.

If you see `HasOne(o => o.XReadModel).WithMany().HasForeignKey(o => o.XId)` in an EF configuration, that FK needs to be removed. `XId` stays as a plain `int` column with no constraint. Remove the navigation property from the entity too.

## SeedData is ctor-built; seeders only persist

`SeedData` is a singleton with a parameterless constructor that builds every entity it exposes from compile-time-deterministic inputs (IDs come from `Concertable.Seeding.Identity.SeedUsers` / `SeedCustomers`; geometry, addresses, names, and relationships are hardcoded in the ctor). All properties are `{ get; }` — there are no setters.

Per-aggregate `XFactory.Seed` statics live in `Module.Domain/Factories/` and chain `.With(nameof(X.Id), id)` (from `Concertable.Seeding.Extensions.EntityReflectionExtensions`) over the domain's `Create` method. `CredentialFactory.Seed` is the canonical pattern.

Seeders read from `SeedData` and persist; they never assign to it:

```csharp
public async Task SeedAsync(CancellationToken ct)
{
    if (await context.Artists.AnyAsync(ct)) return;
    context.Artists.AddRange(seedData.Artists);
    await context.SaveChangesAsync(ct);
}
```

Manager `User` rows are owned by `AuthDevSeeder`, which writes credentials in the Auth DB and publishes `CredentialRegisteredEvent` per credential through the outbox. The B2B/Customer `CredentialRegisteredHandler` writes the matching `User` row in its own DB. There is no separate `UserEventSeeder` in the E2E projects — `[user].[Users]` and the manager profile tables stay in each `DbFixture`'s `TablesToIgnore`, so the rows survive Respawner resets.

## Idempotency

All `IDevSeeder.SeedAsync` implementations must be idempotent — safe to run multiple times against a database that already contains seed data. Use `SeedIfEmptyAsync` for bulk inserts, or guard individual rows with `AnyAsync` / existence checks before adding.
