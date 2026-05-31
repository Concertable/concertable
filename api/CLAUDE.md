# Concertable — backend (`api/`)

The .NET app. C# code conventions: [`docs/CODE_CONVENTIONS.md`](./docs/CODE_CONVENTIONS.md) (notably: logging is source-generated — never call `logger.LogInformation/LogWarning/LogError` with an inline template; add a `[LoggerMessage]` method to the project's `Log.cs`).

## These are microservices — read [`ARCHITECTURE.md`](./ARCHITECTURE.md) before crossing a service boundary

The monorepo is a convenience only. Each service is independently owned and will split into its own repo with its own developers. Design every change as if that split already happened: *would this still work if this service lived alone?*

Two kinds of service, two rules (full rationale in [`ARCHITECTURE.md`](./ARCHITECTURE.md)):

- **Adapter services — `Auth`, `Payment`, `Notification`.** Shared runtime dependencies present in every host. A data service MAY call them synchronously (gRPC) and MAY `WaitFor` them at startup. **B2B and Customer each require Auth + Payment to be running.** So `WaitFor(auth)` / `WaitFor(paymentWeb)` belong in the shared `Concertable.AppHost.Shared` helpers and apply in every host.
- **Data services — `B2B`, `Customer`, `Search`.** They must NEVER depend on each other's runtime. **B2B and Customer require Payment + Auth, but never each other.** Cross-data-service communication is `*.Contracts` events only; when a standalone host is missing another data service's events at seed time, a `*.Seed.Simulator` replays them — you never run the other data service to fix it. A data service `WaitFor`-ing another data service is the bug to never introduce.

Note: real `Payment` only emits payment events for *live* Stripe webhooks, never for seed data. Payment is an agnostic adapter and owns no seed catalog or simulator; the seed-only payment-derived state (B2B `ConcertEntity.TicketsSold`, Customer `TicketEntity`) is inherently unreproducible historical data, reflection-seeded on each consumer's own side (see `docs/SEEDING_CONVENTIONS.md`).

## STOP — read this before any seeding work

**Before writing or modifying any `IDevSeeder` / `ITestSeeder`, and before any change that would put rows into a table whose data the production app never writes directly, read [`docs/SEEDING_CONVENTIONS.md`](./docs/SEEDING_CONVENTIONS.md) in full.** Not the summary below — the full file. Every time.

The rule: **a seeder may only write data that production code writes directly.** If production only creates this data as a *reaction* to something — an event, an outbox message, a handler firing, a webhook — the seeder must drive that same trigger, not bypass it and write the row.

Things you must NOT seed directly. Each of these has a production write path that is *only* a reaction, never a direct insert:

- **Read-model projections / event-synced replicas** — B2B's & Search's `VenueReadModel`, `ArtistReadModel`, `ConcertReadModel`, anything in a `[concert]` / `[venue]` / `[artist]` / `[search]` projection schema, **and Customer's `VenueEntity` / `ArtistEntity` / `ConcertEntity`** (named `*Entity` because in Customer's isolated context they're the only model of that concept — but they're still populated solely by `XChangedEvent` handlers, so the same rule applies). Written by `XChangedEvent` handlers, never seeded directly.
- **`UserEntity` rows** in B2B, Customer, and Payment user tables. Written by `CredentialRegisteredHandler` reacting to `CredentialRegisteredEvent` from Auth.
- **Manager profile rows** (`VenueManagerProfileEntity`, `ArtistManagerProfileEntity`, `AdminProfileEntity`). Written alongside the user by the same `CredentialRegisteredHandler`.
- **Stripe `PayoutAccount` rows** in Payment. Provisioned by `CredentialRegisteredHandler` in Payment.
- **Inbox / outbox / messaging rows.** Owned by the messaging infrastructure.
- **Anything else whose only production write is in an `IIntegrationEventHandler` / `IDomainEventHandler` / outbox dispatcher / webhook handler.**

If the table is empty at seed time and prod never writes it directly, the fix is **always** to make the trigger fire (publish the event, register the credential, etc.) — never `context.X.AddRange(...)` in a seeder.

Quick check before writing a seeder body: open the entity's repository / service / handler. If the only code that calls `.Add` / `.AddRange` on this DbSet in production is inside a handler reacting to an event, your seeder is not allowed to write it either. Drive the event instead.

This mistake has cost real time, multiple times. If unsure, re-read `SEEDING_CONVENTIONS.md` before writing the seeder body.

## Migrations

Don't add additive migrations. When the model changes, run `./initial-migrations.ps1` from `api/`
to nuke and re-scaffold every module's `InitialCreate`.

## DTOs vs Responses

Services return `Dto` types from `Module.Application/DTOs/` (or `Module.Contracts/` for cross-module
shapes). Services never return HTTP-flavoured `Response` types — keeps services callable from
non-HTTP consumers (Workers, gRPC, SignalR, etc.).

Controllers return either the Dto verbatim (default — most endpoints) or a `Response` from
`Module.Api/Responses/` if the wire shape genuinely differs from the Dto (versioning, role-based
shaping, HATEOAS, multiple endpoints rendering the same Dto differently). Don't pre-emptively
shadow every Dto with a Response.

Validators stay named `XValidators` regardless.

Drop the `Dto` suffix when the name already says what the shape is (`AcceptCheckout`, `TicketCheckout`); only keep it to disambiguate from a same-named entity (`CustomerDto` vs `CustomerEntity`).

## Seeders

`IDevSeeder` runs in dev/E2E environments via `DevDbInitializer`. `ITestSeeder` runs in integration tests only — never in E2E or dev startup. Do not create an `IDevSeeder` for data that should be created via domain events (e.g. Stripe payout accounts — those are provisioned when `CredentialRegisteredEvent` fires on user registration). Fix the event flow, don't add a seeder that bypasses it.

See [SEEDING_CONVENTIONS.md](./docs/SEEDING_CONVENTIONS.md) for the full rules.

## Module rules

See [MODULAR_MONOLITH_RULES.md](./docs/MODULAR_MONOLITH_RULES.md).
