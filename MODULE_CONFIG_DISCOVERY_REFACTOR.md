# Cross-Module EF Configuration & Schema Ownership — Refactor Plan

> **Supersedes** the earlier `SEARCH_VIEW_REFACTOR.md`. The problem is broader than Search —
> `ReadDbContext`, `ApplicationDbContext`, and `SearchDbContext` all need to pick up
> `IEntityTypeConfiguration<T>` classes that live in *other* modules' `Infrastructure`
> projects, and all three solve it differently (and badly) today. The same ownership problem
> shows up for **SQL schemas**: `Concertable.Data.Infrastructure/Schema.cs` holds a single
> `"dbo"` constant shared by every module, while `ApplicationDbContext.cs` already declares
> per-module schema names (`"identity"`, `"artist"`, `"venue"`) — live drift. This plan fixes
> both in one pass.

---

## The problem, stated properly

### Problem A — Config discovery

Several `DbContext`s in this codebase need to apply EF configurations that live in foreign
modules' `Infrastructure` assemblies:

- **`SearchDbContext`** (`Concertable.Search.Infrastructure`) reads `ArtistRatingProjection` /
  `VenueRatingProjection` — tables owned by Artist / Venue — and needs their configs to map
  correctly (table name, key, value-generation, owned types, column renames).
- **`ReadDbContext`** (`Concertable.Data.Infrastructure`) — the cross-module read shim — exposes
  `IQueryable<T>` for **every** module's entity types and needs configs from every module.
- **`ApplicationDbContext`** (`Concertable.Infrastructure`) — legacy pre-extraction context —
  still owns `Messages`, `Transactions`, `StripeEvents`, etc., while declaring
  `ExcludeFromMigrations` mappings for already-extracted modules' tables so EF can resolve navs.

All three currently solve it in their own ugly way:

1. **`SearchDbContext` — explicit project references to foreign Infrastructure projects.**
   `Concertable.Search.Infrastructure.csproj` project-refs `Concertable.Artist.Infrastructure`
   and `Concertable.Venue.Infrastructure` so it can call
   `typeof(ArtistRatingProjectionConfiguration).Assembly`. **Rule-2 violation** per
   `CLAUDE.md` ("Do not reference another module's Application, Infrastructure, or Api").
   Already flagged in `memory/project_search_rating_projection_ownership.md`.

2. **`ReadDbContext` — runtime `AppDomain` scanning.**
   ```csharp
   foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()
       .Where(a => a.GetName().Name is string n
                && n.StartsWith("Concertable.")
                && n.EndsWith(".Infrastructure")
                && n != "Concertable.Search.Infrastructure"))
   {
       modelBuilder.ApplyConfigurationsFromAssembly(asm);
   }
   ```
   String-matched assembly names. Depends on CLR having loaded every module's Infrastructure
   before `OnModelCreating` runs (mostly fine in `Web` / `Workers` hosts, fragile in
   integration tests, seeding tools, and design-time scaffolding).

3. **`ApplicationDbContext` — same runtime scan, different exclusions.**
   Same `AppDomain.GetAssemblies()` filter with different strings excluded
   (`!= "Concert.Infrastructure" && != "Search.Infrastructure"`). Every time another module
   extracts, this string list grows.

### Problem B — Schema ownership

`Concertable.Data.Infrastructure/Schema.cs`:

```csharp
public static class Schema
{
    public const string Name = "dbo";
}
```

A single shared constant, referenced by:

- Every module's `DbContext`: `modelBuilder.HasDefaultSchema(Schema.Name)`.
- The recently-linter-edited `ArtistEntityConfiguration.cs`:
  `builder.ToTable("Artists", Schema.Name)` → `dbo.Artists`.
- `ApplicationDbContext.cs`'s `SharedSchema` alias for `ExcludeFromMigrations` of shared
  reference data (`Genres`).

**But** `ApplicationDbContext.cs` *also* hardcodes per-module schema strings in its
`ExcludeFromMigrations` block:

```csharp
modelBuilder.Entity<ArtistEntity>().ToTable("Artists", "artist", t => t.ExcludeFromMigrations());
modelBuilder.Entity<VenueEntity>().ToTable("Venues", "venue", t => t.ExcludeFromMigrations());
modelBuilder.Entity<UserEntity>().ToTable("Users", "identity", t => t.ExcludeFromMigrations());
// ...etc
```

That's a **live drift**. Three possibilities:

1. **Every table is actually in `dbo`** (confirmed by the user's schema screenshot showing
   `dbo.Artists`, `dbo.ArtistRatingProjections`, `dbo.Venues`, etc.). Then
   `ApplicationDbContext`'s per-module schema strings are wrong — EF is trying to map entities
   to `artist.Artists` / `venue.Venues` tables that don't exist. At best this silently breaks
   `ExcludeFromMigrations` intent; at worst, any nav-driven `.Include()` through
   `ApplicationDbContext` fails at runtime.
2. **The target is per-module schemas and migrations haven't moved the tables yet.** The
   `"artist"` / `"venue"` / `"identity"` strings are aspirational; DB reality (all `dbo`) is
   the old shape.
3. **Some mix of (1) and (2).** Likely the true state given the recent migration reset
   (`project_concert_migration_reset.md`).

Either way it needs reconciling. Ownership of schema names has the same shape as ownership of
configs: **the module that owns the entity should own the schema constant used to map it.**

---

## The target

### Target A — marker-based config discovery

```csharp
// api/Concertable.Data.Infrastructure/Data/ICrossModuleReadConfigurationMarker.cs
public interface ICrossModuleReadConfigurationMarker
{
    Assembly Assembly { get; }
}
```

Lives in `Data.Infrastructure` because every module's `Infrastructure` project already
references it for `DbContextBase`. No new csproj references needed.

Each source module:
```csharp
// api/Modules/Artist/Concertable.Artist.Infrastructure/Data/ArtistReadConfigurationMarker.cs
internal sealed class ArtistReadConfigurationMarker : ICrossModuleReadConfigurationMarker
{
    public Assembly Assembly => typeof(ArtistReadConfigurationMarker).Assembly;
}

// AddArtistModule(services):
services.AddSingleton<ICrossModuleReadConfigurationMarker, ArtistReadConfigurationMarker>();
```

Every cross-module reader context (`SearchDbContext`, `ReadDbContext`, `ApplicationDbContext`)
injects `IEnumerable<ICrossModuleReadConfigurationMarker>` and loops it in `OnModelCreating`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(XDbContext).Assembly);
    foreach (var marker in markers)
        modelBuilder.ApplyConfigurationsFromAssembly(marker.Assembly);
    // ExcludeFromMigrations + schema declarations stay the consumer's call.
}
```

Configs themselves **stay in their owning module's `Infrastructure` project** (per
`CLAUDE.md` §Database + `feedback_module_owns_its_configs.md`). The marker publishes the
assembly; it does not move the files.

### Target B — per-module schema ownership

Each module owns its own schema constant:

```
api/Modules/Artist/Concertable.Artist.Infrastructure/Schema.cs    → "artist"
api/Modules/Venue/Concertable.Venue.Infrastructure/Schema.cs      → "venue"
api/Modules/Concert/Concertable.Concert.Infrastructure/Schema.cs  → "concert"
api/Modules/Identity/Concertable.Identity.Infrastructure/Schema.cs → "identity"
api/Concertable.Data.Infrastructure/Schema.cs                     → "dbo"   (reference-data default; see below)
```

- Each module's `DbContext` sets `modelBuilder.HasDefaultSchema(Schema.Name)` using **its own**
  `Schema.Name`.
- Each module's configs that call `ToTable("X", Schema.Name)` use **their own module's**
  `Schema.Name`.
- `ApplicationDbContext`'s `ExcludeFromMigrations` block references each module's schema via
  `Artist.Infrastructure.Schema.Name` / `Venue.Infrastructure.Schema.Name` / etc., rather than
  hardcoded `"artist"` / `"venue"` strings. If `Concertable.Web` / the target DbContext doesn't
  already have a project reference to that module's `Infrastructure` (e.g.
  `ApplicationDbContext` currently does via DI, not direct ref), the simpler path is to let
  each module expose its schema alongside its `IModuleSchema` marker (see §"Optional tighten"
  below) — or just hardcode the string once, because `ApplicationDbContext` itself is headed
  for retirement.
- `SharedDbContext` (per `MM_NORTH_STAR.md §6`) keeps `"dbo"` as the shared-reference-data
  schema — `Genres`, future `Countries` / `Currencies`. `Data.Infrastructure/Schema.cs`
  narrows to "the schema `SharedDbContext` owns", not "the default for everyone." When
  `ApplicationDbContext` retires and its remaining entities extract to their own modules,
  this constant either moves to `SharedDbContext`'s project or stays in `Data.Infrastructure`
  as the host's default for reference data — minor detail to decide at retirement time.

### Target B drift fix

Before anything moves, **verify which schemas the tables are actually in**. The DB screenshot
shows `dbo.Artists`, `dbo.ArtistRatingProjections`, `dbo.Venues`, etc. — all `dbo`. So Target B
is really two sub-decisions:

- **B1 (short-term — reconcile the lie):** make every `ApplicationDbContext`
  `ExcludeFromMigrations` string say `"dbo"`, matching reality. No DB changes, no migration.
  Everything keeps working, the lie goes away, schemas stay flat.
- **B2 (long-term — actually adopt per-module schemas):** migrate every module's tables from
  `dbo` into its owning schema (`artist.Artists`, `venue.Venues`, etc.) via EF migrations,
  then make the per-module `Schema.cs` file the source of truth. DB-level schema separation,
  matches what the current-but-wrong code already declares.

Both are valid. B1 is a 5-minute fix. B2 is a real schema migration with DBA implications and
integration-test churn. **Default recommendation: B1 now, B2 if/when there's a reason (e.g.
per-module DB users, separate schema-level permissions).** Plan structured so B1 is required
and B2 is an optional later stage.

---

## Stages

Each stage leaves the tree building + tests green. Order is Stage 1 → 8; 5+ are independently
separable.

### Stage 1 — Introduce the marker interface

- Add `api/Concertable.Data.Infrastructure/Data/ICrossModuleReadConfigurationMarker.cs`.
- No behaviour change. One commit.

### Stage 2 — Each source module publishes its marker

One commit per module. Order: Artist → Venue → Concert → Identity. Payment when extracted.

Per module:
1. `XReadConfigurationMarker.cs` under `Module.Infrastructure/Data/` (`internal sealed`,
   `Assembly => typeof(XReadConfigurationMarker).Assembly`).
2. Register in `AddXModule()`:
   ```csharp
   services.AddSingleton<ICrossModuleReadConfigurationMarker, XReadConfigurationMarker>();
   ```
3. Still no behaviour change — nothing consumes markers yet.

### Stage 3 — Migrate consumer contexts to marker-based discovery

One commit per context.

**3a — `SearchDbContext`**
1. Inject `IEnumerable<ICrossModuleReadConfigurationMarker> markers`.
2. Replace the two explicit `typeof(ArtistRatingProjectionConfiguration).Assembly` /
   `typeof(VenueRatingProjectionConfiguration).Assembly` calls with a marker loop.
3. Remove `using Concertable.Artist.Infrastructure.Data.Configurations;` +
   `using Concertable.Venue.Infrastructure.Data.Configurations;`.
4. Drop `Concertable.Search.Infrastructure.csproj` refs to `Concertable.Artist.Infrastructure`
   + `Concertable.Venue.Infrastructure`.
5. Keep (or add if missing) direct `Concertable.Artist.Domain` + `Concertable.Venue.Domain`
   refs for the entity types themselves. Rule-3 tolerated escape hatch.
6. Clears `memory/project_search_rating_projection_ownership.md`.

**3b — `ReadDbContext`** *(while it lives, per `MM_NORTH_STAR.md` Corollary 1 it retires)*
1. Inject markers.
2. Replace `AppDomain.CurrentDomain.GetAssemblies()` scan with marker loop.
3. Since Search doesn't register a marker (it's a consumer, not a source), the explicit
   `Search.Infrastructure` exclusion in the old filter goes away naturally.

**3c — `ApplicationDbContext`**
1. Inject markers.
2. Replace `AppDomain` scan + name-exclusion list with marker loop.
3. `ExcludeFromMigrations` block for Identity/Artist/Venue tables **stays** — separate concern
   (that's about schema ownership, addressed in Stages 6+7).

### Stage 4 — Remove dead discovery code + dead strings

- Delete all `AppDomain.CurrentDomain.GetAssemblies()` + string-filter blocks.
- Integration-test assertion: `IEnumerable<ICrossModuleReadConfigurationMarker>` resolves
  non-empty and includes every extracted module's marker. One test per registered module is
  enough.
- Remove any now-redundant `using Concertable.*.Infrastructure.Data.Configurations;` lines.

### Stage 5 — Inventory & relocate pre-extraction configs

Current `api/Concertable.Data.Infrastructure/Data/Configurations/`:
- `MiscEntityConfigurations.cs`
- `TransactionConfigurations.cs`
- `UserHierarchyConfigurations.cs`

Per `CLAUDE.md`: "The configs sitting in `Data.Infrastructure/Data/Configurations/` today are
pre-extraction carryover." Audit each file:

1. If the entity has moved to an extracted module → move the config to that module's
   `Infrastructure/Data/Configurations/` (`internal`).
2. If the entity is still `ApplicationDbContext`-owned (Message, Transaction, Preference,
   StripeEvent) → leave the config in place until that module extracts.
3. `UserHierarchyConfigurations.cs` — Identity is extracted, so user-scoped configs should
   have already moved. Double-check nothing's been left behind.

Each moved config file is one commit.

### Stage 6 — Per-module schema constants (B target, no DB migration)

Scope: code-only reshuffle. DB schemas unchanged.

1. **Create `Schema.cs` in each module's `Infrastructure`** with the intended module name:
   - `Concertable.Artist.Infrastructure/Schema.cs` → `"artist"` (or `"dbo"` if sticking with
     B1 — see Stage 7 decision below; recommend defer string choice until Stage 7).
   - Same for Venue, Concert, Identity.
2. **Update each module's `DbContext`** to use its own `Schema.Name`:
   ```csharp
   modelBuilder.HasDefaultSchema(Schema.Name); // now the module's own constant
   ```
3. **Update each module's configs** that explicitly pass schema (e.g. the recently-edited
   `ArtistEntityConfiguration.cs` `ToTable("Artists", Schema.Name)`) — `Schema.Name` now
   resolves to the module's own constant.
4. **`Data.Infrastructure/Schema.cs`** stays for the moment. It continues to carry `"dbo"`
   for `SharedDbContext` usage + any `ApplicationDbContext`-owned reference data. Retire /
   relocate when `ApplicationDbContext` retires.

### Stage 7 — Reconcile the schema drift (B1 vs B2 decision point)

**Decision required from you before executing.** Check current DB schema first:

```sql
SELECT name FROM sys.schemas;  -- does "artist", "venue", "identity", "concert" exist?
SELECT SCHEMA_NAME(schema_id) AS sch, name FROM sys.tables ORDER BY sch, name;
```

User's earlier screenshot showed all tables in `dbo`. Confirm with the above.

**B1 path (recommended default):**
- Every module's `Schema.cs` = `"dbo"`.
- `ApplicationDbContext`'s `ExcludeFromMigrations` block changes `"artist"` / `"venue"` /
  `"identity"` strings to `"dbo"` (or better, references the module's `Schema.Name` constant
  — see §"Optional tighten").
- Zero DB migrations. The lie disappears. Per-module *constants* exist for future
  extensibility but they all currently point at `"dbo"`.

**B2 path (optional, later):**
- Each module's `Schema.cs` = its own name (`"artist"`, etc.).
- Write migration per module: `ALTER SCHEMA artist TRANSFER dbo.Artists;` and so on for every
  table + index. Multi-step, touches every module's `__EFMigrationsHistory`.
- `ApplicationDbContext` `ExcludeFromMigrations` calls now actually match reality.
- Worth doing if the project ever wants per-schema DB users or permission-scoped access.
  Otherwise, deferrable indefinitely.

Pick B1 unless there's a concrete reason for B2.

### Stage 8 — Docs + memory

- **`CLAUDE.md`** — §Database rules:
  - Add subsection "Cross-module EF configuration discovery" — markers via
    `ICrossModuleReadConfigurationMarker`, not project refs to foreign Infrastructure.
  - Add subsection "Schema ownership" — each module owns its `Schema.cs`; current value
    `"dbo"` (flat) until B2; per-module schema adoption deferred.
- **`MM_NORTH_STAR.md`** — Corollary 1: reference marker pattern as the replacement for
  cross-module config discovery (distinct from cross-module data reads).
- **Memory index:**
  - Add `feedback_cross_module_config_marker.md` — the marker pattern.
  - Add `feedback_module_owns_schema.md` — each module's `Schema.cs` in its own
    `Infrastructure`; central `Schema.cs` in `Data.Infrastructure` narrows to reference-data
    default and retires with `ApplicationDbContext`.
  - Update `feedback_module_owns_its_configs.md` — note that discovery is now marker-based.
  - Resolve / delete `project_search_rating_projection_ownership.md`.
  - Append to `project_modular_monolith.md` — "Cross-module config discovery + schema
    ownership refactor ✅ (date)".

---

## Optional tighten — share the schema constant via the same marker

If `ApplicationDbContext` ends up needing to reference per-module schema names frequently,
the marker can carry schema alongside assembly:

```csharp
public interface ICrossModuleReadConfigurationMarker
{
    Assembly Assembly { get; }
    string Schema { get; }
}
```

Then `ApplicationDbContext`'s `ExcludeFromMigrations` block can loop markers and use
`marker.Schema` per entity. Consider if Stage 7 picks B2; skip if B1 (all `dbo` anyway).

---

## What this does NOT change

- Where configs live: still in their owning module's `Infrastructure/Data/Configurations/`
  (internal).
- Entity types: still in each module's `Domain`. Cross-module type references remain the
  tolerated rule-3 escape hatch.
- `ExcludeFromMigrations` declarations stay each consumer's call.
- `ReadDbContext` retirement timeline: unchanged. Marker cleanup makes it nicer while it
  lives; does not accelerate its death.
- Integration events + domain events: out of scope.
- DB structure: Stage 7 B1 leaves it entirely alone; B2 is explicitly optional.

---

## Risk / blast radius

| Stage | Risk | Mitigation |
|---|---|---|
| 1 | Trivial (one interface file). | — |
| 2 | Mechanical per-module. | Commit per module; build between each. |
| 3a Search | Medium — csproj edit + DI wiring + drops two refs. | Integration tests on every Search endpoint before + after. |
| 3b ReadDbContext | Low — drop-in scan replacement. | Same test coverage as today. |
| 3c ApplicationDbContext | Low — same shape as 3b. | Startup-time assertion that marker count ≥ number of extracted modules. |
| 4 | Mechanical cleanup. | — |
| 5 | File moves only. | Build between each move. |
| 6 | Per-module constants. Zero DB effect. | — |
| 7 B1 | String edit. | — |
| 7 B2 | Real DB migration. Touches every table. | Sequential per-module migration; smoke-test each in a restored backup first. |
| 8 | Docs. | — |

---

## Open questions to resolve before / during execution

1. **Schema drift — B1 or B2?** Confirm current DB state with the SQL in Stage 7 and pick a
   path. Recommendation: B1 unless there's a concrete reason for B2.
2. **Should `ApplicationDbContext` continue to pick up Concert configs via the marker loop?**
   Today its `AppDomain` scan excludes `Concert.Infrastructure`. If Concert registers a marker
   and `ApplicationDbContext` indiscriminately applies all markers, it'd start pulling Concert
   configs. Decide: filter marker list per-context (e.g. marker has a `Scope` enum or
   consumers use a tag), or let all contexts see all markers and let `ExcludeFromMigrations`
   on the consumer side handle the downstream schema-ownership question. Leaning: simpler is
   better — all markers, all contexts, consumer decides migrations.
3. **Does `Concertable.Workers` spin up any cross-module read context of its own?** If yes, it
   needs the same marker wiring. Audit during Stage 3.
4. **`ApiFixture.ReadDbContext`** — continues to work through the refactor; marker change is
   internal to `OnModelCreating`. When `ReadDbContext` eventually retires, that's a separate
   concern outside this plan.

---

## Order of execution

1. Stage 1 — marker interface. *(1 commit)*
2. Stage 2 — module markers. *(1 commit per module)*
3. Stage 3a — SearchDbContext. *(1 commit)*
4. Stage 3b — ReadDbContext. *(1 commit)*
5. Stage 3c — ApplicationDbContext. *(1 commit)*
6. Stage 4 — cleanup. *(1 commit)*
7. Stage 5 — config relocation. *(1 commit per moved file)*
8. Stage 6 — per-module `Schema.cs`. *(1 commit per module)*
9. Stage 7 — drift decision + fix. *(1 commit for B1; multi-commit for B2 if chosen.)*
10. Stage 8 — docs + memory. *(1 commit)*

Valid stopping points: after Stage 3a (clears the Search rule-2 violation), after Stage 4
(all consumers on markers), after Stage 7 B1 (drift reconciled). B2 and Stage 8 can land
whenever.
