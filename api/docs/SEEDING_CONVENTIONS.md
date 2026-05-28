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

## SeedData ref assignment pattern

When a seeder needs to expose a named reference (e.g. `seedData.ArtistManager1`) for later seeders or tests to use:

1. Create the entity via its factory method (which accepts an explicit known ID).
2. Assign `seedData.X = entity` **on the factory-created object, before `SaveChangesAsync`**.
3. Conditionally call `context.X.Add(entity)` only if that ID doesn't already exist in the DB.

```csharp
// Correct
seedData.ArtistManager1 = UserEntity.FromRegistration(SeedIds.ArtistManager(1), "artistmanager1@test.com", Role.ArtistManager);
if (!existingIds.Contains(SeedIds.ArtistManager(1)))
    context.Users.Add(seedData.ArtistManager1);

await context.SaveChangesAsync(ct);
```

Never load the entity back from the DB after saving to assign the `seedData` ref:

```csharp
// Wrong — don't do this
await context.SaveChangesAsync(ct);
seedData.ArtistManager1 ??= await context.Users.SingleAsync(u => u.Id == SeedIds.ArtistManager(1), ct);
```

## Idempotency

All `IDevSeeder.SeedAsync` implementations must be idempotent — safe to run multiple times against a database that already contains seed data. Use `SeedIfEmptyAsync` for bulk inserts, or guard individual rows with `AnyAsync` / existence checks before adding.
