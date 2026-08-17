# Seeding — Concertable's forbidden-table inventory

> ## ⚠ READ THIS FIRST
>
> **Never write `context.X.Add(...)`/`AddRange(...)` against a DbSet whose entity is only written by a
> handler in production.** The rule, the dev/test seeder split, the simulator pattern, the two sanctioned
> exceptions, constructor-built seed state and idempotency are the **`seeding` skill** — read it before
> writing any seeder body. This file is the inventory of which tables that means *here*.
>
> This mistake has cost real time, multiple times.

## Never seed these — each has a reaction-only production write path

- **Read-model projections and event-synced replicas** — B2B's and Search's `VenueReadModel`,
  `ArtistReadModel`, `ConcertReadModel`, anything in a `[concert]`/`[venue]`/`[artist]`/`[search]` schema,
  **and Customer's `VenueEntity`/`ArtistEntity`/`ConcertEntity`** (named `*Entity` because in Customer's
  isolated context they are the only model of that concept — but still written solely by `XChangedEvent`
  handlers, so the same rule applies).
- **`UserEntity` rows** in B2B, Customer and Payment. Written by `CredentialRegisteredHandler` reacting to
  Auth's `CredentialRegisteredEvent`.
- **`AdminProfileEntity` rows.** Written alongside admin users by `CredentialRegisteredHandler`;
  venue/artist authority lives in Tenant memberships, not manager-profile tables.
- **Stripe `PayoutAccount` rows** in Payment. Provisioned by handlers reacting to registration events.
- **Invitation rows** (`TenantInvitationEntity`, `tenant.Invitations`) **and invitation-derived
  memberships.** Created only by the invite endpoint or the `TenantProvisioningHandler` invitation branch.
  Only the **founding Owner** membership is seeded, alongside its tenant. There is no `SeedState.Invitations`;
  integration tests exercise invitations through the real invite/accept endpoints.
- **Inbox/outbox/messaging rows.** Owned by the messaging infrastructure.

## The B2B simulator is the standalone-seeding mechanism

Customer runs standalone without B2B, so its projection tables would be empty. B2B therefore ships:

- **`Concertable.B2B.Seed.Simulator`** — a Worker host that publishes the canonical B2B `XChangedEvent` set
  on startup and exits. Registered as an Aspire resource in `Concertable.Customer.AppHost`; **not** in the
  umbrella `Concertable.AppHost`, where real B2B already runs.
- **`Concertable.B2B.Seed.Contracts`** — the canonical event records both B2B's own seeders
  (`Concertable.B2B.Seed.Infrastructure.SeedState`) and the simulator derive from. Byte-for-byte, no drift.

Full design, and what the fixture holds:
[`../Concertable.B2B/src/Seed/Concertable.B2B.Seed.Simulator/AGENTS.md`](../Concertable.B2B/src/Seed/Concertable.B2B.Seed.Simulator/AGENTS.md).

**Payment owns no seed catalog and no simulator.** It is an agnostic adapter that always runs, so it has no
absent-peer problem to solve; and a catalog of ticket *purchases* would be wrong there anyway, since
purchase semantics live in the B2B/Customer consumers that read `PaymentSucceededEvent.Metadata`.

## The unreproducible-trigger exception here is past-dated ticket sales

Real Payment emits `PaymentSucceededEvent` only for a live Stripe webhook, and you cannot buy a ticket to a
concert that has already happened — so seeded sales on past-dated concerts have no replayable trigger.
Therefore B2B sets `ConcertEntity.TicketsSold` via `ConcertFactory` from a `ticketsSold` field on
`ConcertSeedSpec`, and Customer direct-inserts `SeedState.Tickets` via `TicketDevSeeder`/`TicketTestSeeder`.
Each consumer seeds its own copy on its own side; nothing crosses the boundary.

This is the **only** sanctioned reflection-seed of handler-owned state outside integration tests. Never
generalise it to live or future state, and never reintroduce a `Payment.Seed.Simulator` to "fix" missing
ticket sales.

## Where the integration-test projection seeders and the identity seeds live

The sanctioned integration-test exception is `XProjectionTestSeeder : ITestSeeder`
(`VenueProjectionTestSeeder`, `ArtistProjectionTestSeeder`, `ConcertProjectionTestSeeder`,
`SearchProjectionTestSeeder`), each driven from `Concertable.B2B.Seed.Contracts.SeedCatalog` — the same
specs the dev/E2E simulator replays. Tests reach `SeedState` handles, which for read models *are* the
catalog specs (`seedState.UpcomingFlatFeeConcert`, `seedState.Venue`), never the read-model entities.

Deterministic ids come from `Concertable.Seed.Identity.SeedUsers`/`SeedCustomers`; per-aggregate
`XFactory.Seed` statics live in `Module.Domain/Factories/` and chain
`.With(nameof(X.Id), id)` from `Concertable.Seed.Identity.Extensions.EntityReflectionExtensions` over the
domain's `Create` method. `CredentialFactory.Seed` is the canonical pattern.

Manager `User` rows are owned by `AuthDevSeeder`, which writes credentials in the Auth DB and publishes
`CredentialRegisteredEvent` through the outbox; each service's `CredentialRegisteredHandler` writes its own
row. There is no `UserEventSeeder` in the E2E projects — `[user].[Users]` and B2B's `[user].[AdminProfiles]`
stay in each `DbFixture`'s `TablesToIgnore` so those rows survive Respawner resets.
