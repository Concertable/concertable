# Seeding Conventions

> ## ⚠ READ THIS FIRST
>
> **Never write `context.X.Add(...)` or `context.X.AddRange(...)` against a DbSet whose entity is only written by a handler in production.**
>
> Read-model projections (`VenueReadModel`, `ArtistReadModel`, `ConcertReadModel`, anything in `[concert]` / `[venue]` / `[artist]` / `[search]` schemas), `UserEntity` rows, manager profile rows, Stripe `PayoutAccount` rows, inbox/outbox messages — none of these are seeded. They are written by handlers reacting to events. The seeder's job in those cases is to **drive the event**, not to write the row.
>
> If standalone Customer (or any service) lacks projection data because the producing service isn't running, the answer is **not** "seed the projection table." The answer is "stand up a seeding simulator for the producing service" — see [Standalone-service seeding](#standalone-service-seeding) below.
>
> This mistake has cost real time, multiple times. If unsure, re-read this whole file before writing the seeder body.

## Standalone-service seeding

Each microservice has its own AppHost (`Concertable.Customer.AppHost`, `Concertable.B2B.AppHost`, etc.) that runs the service standalone for dev iteration. A standalone service still needs the projection data it would receive in production — venues/artists/concerts that come from B2B's integration events.

Without B2B running:

- Customer's projection tables stay empty → Customer SPA shows nothing → useless dev environment.
- Customer E2E tests that depend on a concert fail.

The convention rules out the easy hack (`context.XReadModels.AddRange(...)` in a seeder — see banner above). The approved mechanism is a **seeding simulator** — a small Worker host owned by the producing service that publishes its integration events on startup and exits.

For the B2B case:

- `Concertable.B2B.Seed.Simulator` (Worker host) publishes the canonical B2B `XChangedEvent` set on startup.
- `Concertable.B2B.Seed.Contracts` holds the canonical event records — single source of truth that both B2B's own seeders (`Concertable.B2B.Seed.Infrastructure.SeedState`) and the simulator derive from. Byte-for-byte sync, no field drift.
- Registered in `Concertable.Customer.AppHost` as an Aspire resource. **Not** registered in the umbrella `Concertable.AppHost` (real B2B is already there).

Customer's projection handlers run unchanged in both scenarios. Same code path, same data shape.

See `api/Concertable.B2B/Concertable.B2B.Seed.Simulator/CLAUDE.md` for the full design — what the fixture holds, what it doesn't, how to add entities, what NOT to do, and how the split-repo distribution works.

The same pattern applies to any future standalone-service seeding need: the upstream service ships a simulator, downstream AppHosts reference it.

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

### The one exception: `XProjectionTestSeeder` (integration tests only)

Integration tests are the single, deliberate exception to "read models only via handlers". Spinning up the producing service's bus + handler path inside a `WebApplicationFactory` is slow and flaky, so each read-model module ships an `XProjectionTestSeeder : ITestSeeder` (e.g. `VenueProjectionTestSeeder`, `ArtistProjectionTestSeeder`, `ConcertProjectionTestSeeder`) that direct-inserts the read-model rows.

This is safe — not a violation of the spirit of the rule — because:

- The seeder is driven from the **same `Concertable.B2B.Seed.Contracts.SeedCatalog`** that drives the dev/E2E simulator. Both consume identical spec data, so the direct-inserted rows are byte-identical to what the handler path would produce. There is no second source of truth to drift from.
- It is an `ITestSeeder`, so it runs in **integration tests only** — never in dev or E2E, which keep the simulator → `XChangedEvent` → projection-handler path unchanged.
- It maps each spec to `XReadModel.Create(...)` field-for-field, exactly as the projection handler does.

Test code still never calls `db.XReadModels.Add(...)` itself. Tests reach into `SeedState` handles — and for read models those handles are the `SeedCatalog` specs (`seedState.UpcomingFlatFeeConcert`, `seedState.Venue`, …), never the read-model entities.

## Write models must not have FK constraints to read models

A navigation property from a write-model entity to a read-model projection creates a database FK from the write table to the read table. This is always wrong:

- The read table may be empty at seed time (events not yet processed).
- It couples the write model's persistence to the read model's availability.

If you see `HasOne(o => o.XReadModel).WithMany().HasForeignKey(o => o.XId)` in an EF configuration, that FK needs to be removed. `XId` stays as a plain `int` column with no constraint. Remove the navigation property from the entity too.

## SeedState is ctor-built; seeders only persist

`SeedState` is a singleton with a parameterless constructor that builds every entity it exposes from compile-time-deterministic inputs (IDs come from `Concertable.Seed.Identity.SeedUsers` / `SeedCustomers`; geometry, addresses, names, and relationships are hardcoded in the ctor). All properties are `{ get; }` — there are no setters.

Per-aggregate `XFactory.Seed` statics live in `Module.Domain/Factories/` and chain `.With(nameof(X.Id), id)` (from `Concertable.Seed.Identity.Extensions.EntityReflectionExtensions`) over the domain's `Create` method. `CredentialFactory.Seed` is the canonical pattern.

Seeders read from `SeedState` and persist; they never assign to it:

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
