---
name: concertable-dotnet-seeding
description: Concertable's forbidden-table inventory — the exact tables a seeder may never insert (read-model projections and event-synced replicas, `UserEntity`, `AdminProfileEntity`, Stripe `PayoutAccount`, invitation rows and invitation-derived memberships, inbox and outbox), the B2B seed simulator that makes standalone Customer work, the downward-only direction of a producer's `Seed.Contracts`, the past-dated ticket-sales exception that is the only sanctioned reflection-seed outside integration tests, where the projection test seeders and identity seeds live, and which seeder interface runs in which environment. Use before writing or changing any seeder here, when a table is empty at seed time, or when reviewing an `AddRange` call in seed code.
---

# Seeding — Concertable's forbidden-table inventory

> **The rule is the `seeding` skill — read it before writing any seeder body.** A seeder may only write data
> production code writes directly; anything whose only production write path is a handler reacting to an
> event is driven, never inserted. This file is the inventory of which tables that means *here*.
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
- **`AdminProfileEntity` rows.** Written by B2B's `CredentialRegisteredHandler` calling
  `IAdminModule.GrantIfEligibleAsync` in the same transaction; venue/artist authority lives in Tenant
  memberships, not manager-profile tables.
- **Stripe `PayoutAccount` rows** in Payment. Provisioned by handlers reacting to registration events.
- **Invitation rows** (`TenantInvitationEntity`, `tenant.Invitations`) **and invitation-derived
  memberships.** Created only by the invite endpoint or the `TenantProvisioningHandler` invitation branch.
  Only the **founding Owner** membership is seeded, alongside its tenant. There is no
  `SeedState.Invitations`; integration tests exercise invitations through the real invite/accept endpoints.
- **Inbox/outbox/messaging rows.** Owned by the messaging infrastructure.

## The B2B simulator is the standalone-seeding mechanism

Customer runs standalone without B2B, so its projection tables would be empty. B2B therefore ships:

- **`Concertable.B2B.Seed.Simulator`** — a Worker host that publishes the canonical B2B `XChangedEvent` set
  on startup and exits. Registered as an Aspire resource in `Concertable.Customer.AppHost`; **not** in the
  umbrella `Concertable.AppHost`, where real B2B already runs.
- **`Concertable.B2B.Seed.Contracts`** — the canonical event records both B2B's own seeders
  (`Concertable.B2B.Seed.Infrastructure.SeedState`) and the simulator derive from. Byte-for-byte, no drift.

**Payment owns no seed catalog and no simulator.** It is an agnostic adapter that always runs, so it has no
absent-peer problem to solve; and a catalog of ticket *purchases* would be wrong there anyway, since purchase
semantics live in the B2B/Customer consumers that read `PaymentSucceededEvent.Metadata`.

## A producer's seed library points downward only

A data service other data services project from owns two seed projects, and they obey the same dependency
direction as everything else — **consumer to producer, never the reverse**:

- **`Concertable.X.Seed.Contracts`** — the producer's canonical seed data (`XSeedSpec` records plus their
  `ToEvent()` mappers). Anyone needing X's seed data references this: X's own seeders, downstream consumers'
  projection-test seeders, X's simulator. It is referenced by consumers and references none of them.
- **`Concertable.X.Seed.Simulator`** — the Worker that replays those contracts onto the bus and exits.

**A producer's `Seed.Contracts` must never reference a consumer's.** If producer seed data needs a foreign id
it does not own, it declares it as a literal or opaque value — it does not import a consumer's catalog to
resolve it.

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
`SearchProjectionTestSeeder`), each driven from `Concertable.B2B.Seed.Contracts.SeedCatalog` — the same specs
the dev/E2E simulator replays. Tests reach `SeedState` handles, which for read models *are* the catalog specs
(`seedState.UpcomingFlatFeeConcert`, `seedState.Venue`), never the read-model entities.

Deterministic ids come from `Concertable.Seed.Identity.SeedUsers`/`SeedCustomers`; per-aggregate `XFactory.Seed`
statics live in `Module.Domain/Factories/` and chain `.With(nameof(X.Id), id)` from
`Concertable.Seed.Identity.Extensions.EntityReflectionExtensions` over the domain's `Create` method.
`CredentialFactory.Seed` is the canonical pattern, and `CredentialFactory.Create` its non-seed counterpart.

Manager `User` rows are owned by `AuthDevSeeder`, which writes credentials in the Auth DB and publishes
`CredentialRegisteredEvent` through the outbox; each service's `CredentialRegisteredHandler` writes its own
row. B2B's handler additionally calls `IAdminModule.GrantIfEligibleAsync` in the same transaction, so an
eligible registration grants `[admin].[AdminProfiles]` atomically with the `User` row. There is no
`UserEventSeeder` in the E2E projects — `[user].[Users]` and `[admin].[AdminProfiles]` stay in each
`DbFixture`'s `TablesToIgnore` so those rows survive Respawner resets.

## Which seeder interface runs where

`IDevSeeder` runs in dev and E2E environments via `DevDbInitializer`. `ITestSeeder` runs in integration tests
only — never in E2E or dev startup.
