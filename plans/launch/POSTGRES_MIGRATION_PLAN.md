# SQL Server to Postgres migration

> **Next steps live in @plans/launch/POSTGRES_MIGRATION_PROGRESS.md → `## Next Steps`.**

## 1. Approved decision

Move all five services from SQL Server to Postgres before launch. The driver is domain fit and
development velocity, not a capability ceiling — SQL Server runs this system correctly today.

What Postgres buys that is specific to Concertable:

- **PostGIS.** Eight `geography` columns across B2B, Customer, and Search back location search.
  SQL Server spatial indexes need grid-level tuning and are frequently skipped by the optimizer;
  PostGIS GiST is the reason this data type exists. The C# side is already NetTopologySuite.
- **`EXCLUDE` constraints.** `EXCLUDE USING gist (venue_id WITH =, during WITH &&)` makes
  double-booking a venue structurally impossible in the database. SQL Server cannot express overlap
  exclusion at all, so that invariant is enforced in application code and stays racy.
- **Container ergonomics.** The mssql image is ~1.5GB and takes seconds to become healthy; postgres is
  ~150MB and near-instant. Every fixture project pays that cost on every integration run.
- **`INSERT ... ON CONFLICT`** in place of `MERGE`, which has a long correctness-bug history.
- Licensing cost at production scale.

## 2. This is a provider swap, not a data migration

**There is no production data.** The migration happens before launch, so there is no dual-write window,
no ETL, no backfill, and no compatibility layer. Each service's cut-over is: swap the provider package,
fix the provider-specific configuration, re-scaffold initial migrations against Npgsql, fix the
fixtures, and go green. This removes the hardest part of a typical Postgres migration and is the reason
the plan can be phased per service rather than as one flag day.

**Each service owns its own database.** Auth, B2B, Customer, Payment, and Search have separate
databases, connection strings, and Aspire resources, so one service can run on Postgres while the rest
run on SQL Server. There is no shared schema forcing a simultaneous flip.

## 3. Measured provider-specific surface

Inventoried against `main` on 2026-09-05:

| Surface | Count | Location |
|---|---|---|
| `SET IDENTITY_INSERT` | 8 | Application/Booking test seeders, integration fixtures |
| `sys.check_constraints` queries | 2 | integration fixtures |
| `HasColumnType("geography")` | 8 | B2B Artist/Venue/User + Concert `VenueReadModel`; Customer User; Search Artist/Concert/Venue read models |
| `HasColumnType("nvarchar…")` | 5 | `Concertable.Messaging` outbox/inbox, `DataAccess` `DbContextBase` |
| `IsRowVersion()` + `byte[] Version` | 5 implementers | `Concertable.B2B.DataAccess` `IConcurrencyVersioned` and `ConcurrencyVersionExtensions`; `InvoiceSequenceEntity` |
| Migration files to regenerate | 50 | Auth 4, B2B 24, Customer 14, Messaging 4, Payment 2, Search 2 |

The concurrency token is **B2B-local**, not shared — `IConcurrencyVersioned` and
`ConcurrencyVersionExtensions` both live in `Concertable.B2B.DataAccess`. It is handled in the B2B
phase rather than as shared prep.

## 4. Prep removes provider decisions from consumers and ships on SQL Server

Phases 1-4 change no provider. They either replace a SQL Server-specific construct with configuration EF
Core can render for either provider or centralize a shared relational semantic behind one package seam.
Each ends green on SQL Server with an unchanged schema. Only the per-service phases flip anything.

## 5. Phases

### Phase 1 — shared package column types

- [x] Replace `HasColumnType("nvarchar(450)")` and `nvarchar(max)` in `Concertable.Messaging` outbox and
  inbox configurations and in `DataAccess` `DbContextBase` with `HasMaxLength(450)` and an unbounded
  string, letting each provider render its own type.

Consumption contract: no API change. Every consuming service keeps its current SQL Server schema; the
published `Concertable.Messaging` and `Concertable.DataAccess.Infrastructure` packages gain provider
neutrality only.

Gate: re-scaffolded migrations produce an identical SQL Server schema, and the full integration matrix
stays green. Publish and platform-sync before Phase 5.

### Phase 2 — spatial configuration seam

- [x] Replace the eight `HasColumnType("geography")` calls with one shared configuration extension so
  the shared `geography` relational semantic has a single site.

Consumption contract: an extension applied to a `Point` property that preserves `geography` semantics.
SQL Server and PostGIS both support that relational type name, so the extension needs no active-provider
branch. Entity and query code stays on NetTopologySuite and does not change.

Delivery sequence: the extension is an additive public API in the published
`Concertable.DataAccess.Infrastructure` package, while service projects bind the published package rather
than its source. Land and publish the package expansion first, consume the generated platform sync, then
land the eight call-site changes and regenerated service migrations.

Gate: existing spatial queries and their integration coverage stay green on SQL Server.

### Phase 3 — seed and fixture SQL neutrality

- [x] Replace the eight `SET IDENTITY_INSERT` blocks and the two `sys.check_constraints` queries with
  provider-dispatched helpers in the shared testing library.

Consumption contract: `Concertable.Seed.Shared` decides whether explicit identity values require a SQL
Server identity window, while `Concertable.Testing.Integration` exposes `DatabaseFacade` extensions for
identity windows and temporary unvalidated check constraints. Callers provide schema, table, constraint and
provider-neutral predicate values; the helpers delimit identifiers and render the active provider's SQL.

Delivery sequence: publish the additive shared-package APIs from `Concertable/platform-dotnet` first, advance
the monorepo's platform train to that exact published version, then migrate the Auth and B2B fixtures on their
own branch and validate them against the published packages.

Gate: every service's integration suite stays green on SQL Server with no raw SQL Server syntax left in
the seed or fixture path.

### Phase 4 — test harness seam

- [ ] Make the Testcontainers image and the Respawn `DbAdapter` selectable per service, defaulting to
  SQL Server.

Gate: full integration matrix green on SQL Server through the new seam.

### Phase 5 — pilot cut-over: Search

Search is the pilot because it carries three spatial read models with only two migrations and no write
surface, so it proves PostGIS, the harness seam, and the migration regeneration at the smallest blast
radius.

- [ ] Swap to `Npgsql.EntityFrameworkCore.PostgreSQL` and `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`.
- [ ] Re-scaffold initial migrations, update the Aspire resource and connection string.

Gate: Search integration suite green on Postgres; spatial queries return identical results to the SQL
Server baseline for the same seed data.

### Phase 6 — Payment

Two migrations, but the heaviest `ExecuteUpdateAsync` usage in the system and real money semantics.

Gate: Payment integration suite green, including escrow, transaction, and commission-binding update paths.

### Phase 7 — Auth

Four migrations plus the Duende operational store, which is a third-party EF model and the one place a
vendor package chooses its own provider configuration.

Gate: Auth integration suite and the operational-store migration fixture green on Postgres.

### Phase 8 — Customer

Fourteen migrations, one spatial column.

Gate: Customer integration suite green on Postgres.

### Phase 9 — B2B

The largest surface: 24 migrations, four spatial columns, the tenant-filtered context stances, and the
concurrency token.

- [ ] Replace `byte[] Version` + `IsRowVersion()` with the chosen Postgres token across
  `IConcurrencyVersioned`, `ConcurrencyVersionExtensions`, the five implementers, and
  `InvoiceSequenceEntity`. Decide between Npgsql's `uint` row-version mapping to `xmin` and an explicitly
  maintained `bigint` (portable, explicit), with a written rationale before implementing. Current
  PostgreSQL freezing preserves the original `xmin`; do not base the choice on the older claim that
  `VACUUM FREEZE` rewrites it. Neither token is a durable business/document revision identifier. See
  [Npgsql concurrency](https://www.npgsql.org/efcore/modeling/concurrency.html) and
  [PostgreSQL freezing](https://www.postgresql.org/docs/current/routine-vacuuming.html#VACUUM-FOR-WRAPAROUND).
- [ ] Confirm the tenant global query filters and the `RS0030` `IgnoreQueryFilters` ban behave
  identically under Npgsql.

Gate: B2B unit, module integration, and `Process` integration suites green on Postgres.

### Phase 10 — decommission

- [ ] Remove SQL Server provider packages, Aspire resources, connection strings, and the harness
  defaults.
- [ ] `grep -rniE "sqlserver|mssql|nvarchar|rowversion|identity_insert"` over the repository returns
  zero outside an explicit written allowlist.

Gate: full solution build, full integration matrix, and merge-queue E2E green with no SQL Server
dependency anywhere.

## 6. Definition of done

- Every service runs on Postgres; no SQL Server provider package, image, or connection string remains.
- The spatial columns are PostGIS-backed and location search is covered by its existing tests.
- The concurrency token decision is recorded with its rationale, not left implicit in the diff.
- No provider-specific SQL survives outside migrations and the enumerated per-table helpers.
- The rename grep gate returns zero or an explicit allowlist.

## 7. Follow-ons this unlocks, deliberately out of scope

- `EXCLUDE USING gist` for venue double-booking, replacing the application-level overlap guard.
- Rewriting the `launch/lifecycle-seal-enforcement` per-table write-block helpers from SQL Server block
  predicates to Postgres policies plus a `BEFORE UPDATE` trigger for a loud error.
- [Versioned Deal configurations](DEAL_CONFIGURATION_PLAN.md): relational ownership/revisions and
  selected capabilities plus a typed `jsonb` term graph and full Contract snapshot. The B2B provider
  cut-over gates that plan's persistence delivery, not its typed-language work. It is not a new gate on
  this provider migration or MVP launch. Tenant authoring/entitlements and a builder remain deferred.
  Use targeted GIN, expression indexes or relational read projections for evidenced queries, not blanket
  GIN indexes. That plan selects an aggregate `bigint` edit token for its new mutable configuration/Deal
  heads; Phase 9 still owns the token decision for existing B2B aggregates.

## 8. Rejected directions

- **Dual-provider support or runtime provider switching.** There is no production data and no window in
  which both providers must serve the same service; a compatibility abstraction would be permanent cost
  for a temporary problem.
- **Data migration tooling or ETL.** Nothing to migrate before launch.
- **Lowest-common-denominator EF configuration** that avoids provider-specific features — it would
  forfeit PostGIS and `EXCLUDE`, which are two of the reasons for moving.
- **A single flag-day flip of all five services.** Each service owns its own database; a per-service
  cut-over keeps every intermediate state shippable.
- **Deferring `launch/lifecycle-seal-enforcement` until after this migration.** Its provider-specific
  surface is the per-table SQL helpers only, and waiting would leave every row written in the meantime
  unsealed.
- **Waiting for a Postgres-native reason to migrate before starting prep.** Phases 1-4 are honest
  improvements to provider neutrality that ship on SQL Server and stand on their own.
