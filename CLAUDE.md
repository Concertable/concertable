# Concertable — Modular Monolith Rules

Ongoing refactor: extracting bounded modules (Search ✅, Identity ✅, Artist 🚧, Venue, Concert, Payment) out of the
legacy `Concertable.Core` / `Concertable.Application` / `Concertable.Infrastructure` trio. Follow these rules for
**every** change until the extraction is complete.

## Module layout

```
api/Modules/<Module>/
  Concertable.<Module>.Contracts/       ← public surface; other modules reference this only
  Concertable.<Module>.Domain/          ← entities, domain events, value objects scoped to the module
  Concertable.<Module>.Application/     ← services, repositories, DTOs, requests, validators, mappers
  Concertable.<Module>.Infrastructure/  ← EF repos, external integrations, AddXModule() DI extension
  Concertable.<Module>.Api/             ← controllers for this module's HTTP endpoints
```

`Concertable.Web` is the composition host only — it wires up middleware/auth/Swagger, calls each
module's `AddXModule()`, and references each module's `.Api` so the host can discover controllers
via `ApplicationPart`. It does **not** own per-module controllers.

**Module.Api pattern status (2026-04-22):** Artist, Identity, and Search have migrated.
Venue/Concert/Payment are not yet extracted. `Concertable.Web/Controllers/` still holds legacy
controllers for those unextracted modules — that's expected, not drift.

## Cross-module rules — the short version

1. **Contracts is the only module a *foreign* caller references.** A foreign caller = another
   module's Application / Infrastructure / Api, or a cross-module orchestration inside
   `Concertable.Web`. For them, Contracts holds public interfaces (`IXModule` facade), cross-module
   DTOs, integration events — the stable shape that would become HTTP/Refit if this module
   extracted to a microservice. `IXModule` is deliberately **minimal**: cross-module lookups, not
   the full CRUD surface. Controllers for a module's own HTTP endpoints are NOT foreign callers —
   they live inside `Module.Api` and inject the module's own internal services
   (`IXService`/`IXRepository` in `Module.Application`, made visible via `InternalsVisibleTo`).
2. **Do not reference another module's `Application`, `Infrastructure`, or `Api`.** If you think
   you need to, stop and consult the user — this is a design decision, not a shortcut.
   `Concertable.Web` is allowed to reference `Module.Api` (for ApplicationPart discovery) and
   `Module.Infrastructure` (for `AddXModule()` DI), but not `Module.Application`.
3. **Domain references are tolerated as the escape hatch for entities.** `IReadDbContext` exposes
   `IQueryable<TEntity>` typed against module Domain projects, and EF nav properties across modules
   (e.g. `OpportunityApplicationEntity.Artist`) need the referenced entity type to compile. This is
   the *only* currently-accepted reason to reference another module's Domain. Plan: replace with
   Contracts-level read models (e.g. `IQueryable<ArtistReadModel>`) so Domain stops leaking. Until
   that lands, the Core → Module.Domain direction is the accepted workaround, not the target.
4. **Never reference across modules in the wrong direction.** Module.Domain must not reference
   `Concertable.Core` (that's the cycle that motivated Artist Stage 0). If a shared entity is in the
   way, move it to `Concertable.Shared.Domain` (e.g. `GenreEntity`) rather than re-coupling.
5. **Shared is for primitives only.** Value objects, marker interfaces (`IIdEntity`, `IHasName`,
   `IHasLocation`), domain-event primitives, reference-data entities that every module reads but no
   module owns writes for (`GenreEntity`). No concrete services, no module-specific DTOs. If only
   one module uses it, it belongs in that module.
6. **Inbound coupling gets fixed by the caller's module extraction, not the callee's.** If Concert
   repos have `.Include(x => x.Artist)` chains, leave them alone when extracting Artist — they get
   rewritten when Concert is extracted to talk to `IArtistModule` instead.

## Database rules

- Module write contexts inherit `DbContextBase` — **never** `ApplicationDbContext`.
- A module DbContext owns only its module's entities. Cross-module FKs are plain primitives
  (`Guid`, `int`) with no nav property and no foreign `DbSet` on that context.
- `IReadDbContext` (in `Concertable.Data.Application`) is the cross-module read shim — it projects
  `IQueryable<TEntity>` for every module. Use it for reads that span modules.
- EF `IEntityTypeConfiguration<T>` files **live in the module that owns the entity**, under
  `Module.Infrastructure/Data/Configurations/`, declared `internal`. Module DbContexts apply
  them explicitly via `modelBuilder.ApplyConfiguration(new XConfiguration())` — not via
  `ApplyConfigurationsFromAssembly`. **Do not** add new module-owned configs to
  `Concertable.Data.Infrastructure/Data/Configurations/` "for symmetry with what's there." The
  configs sitting in `Data.Infrastructure/Data/Configurations/` today are pre-extraction
  carryover, applied by `ApplicationDbContext` via `ApplyConfigurationsFromAssembly` and by
  `ReadDbContext` the same way; they retire alongside `ApplicationDbContext` (Concert Step 13
  and equivalents). When you author a NEW config or extract an existing one, it goes to the
  owning module's Infrastructure project. The only configs that should remain in
  `Data.Infrastructure/Data/Configurations/` are ones for entities still owned by
  `ApplicationDbContext` (Message, Transaction, Preference, StripeEvent, etc., until those
  modules extract).
- Composite keys, owned types, `geography` columns, etc. configured via Fluent API so Domain stays
  free of EF Core attributes beyond `System.ComponentModel.DataAnnotations.Schema` BCL attributes.
- **Migration history is disposable during the refactor.** Seeders own fixture data, so migration
  files don't need to preserve incremental change history. When schema drifts, **delete every
  module's `Migrations/` folder and re-scaffold a fresh `InitialCreate` per context** rather than
  adding additive migrations. Scaffold order: Shared → Identity → Artist → Venue → Concert →
  AppDb (modules before AppDb; AppDb last). This stays in force until production writes matter;
  at that point switch to additive-only migrations and note it here.

## Internal enforcement

- Everything in `Module.Application` and `Module.Infrastructure` should be `internal`. Only
  `Module.Contracts` and the `AddXModule()` extension are public.
- Compiler-enforced boundary, not a naming-convention hope.
- Remove Identity/Artist/etc internals from `Concertable.Web/GlobalUsings.cs` — Web imports from
  `Module.Contracts` (and optionally `Module.Api` for controller ApplicationPart wiring) only.
- `IXService`, `IXRepository`, validators, mappers — all `internal` to `Module.Application`. They
  are visible to `Module.Infrastructure` and `Module.Api` via `[assembly: InternalsVisibleTo(...)]`
  entries (put these in an `AssemblyInfo.cs` inside the owning project, matching the Identity
  precedent at `api/Modules/Identity/Concertable.Identity.Infrastructure/AssemblyInfo.cs`).
  `Module.Api` controllers inject `IXService`/`IXRepository` because Api is same-module, not
  foreign.
- DTOs, requests, responses **consumed by the module's own controllers** can stay `internal` in
  `Module.Application` if they're not consumed by any foreign caller. Move them to
  `Module.Contracts` only when another module actually needs them (e.g. a summary DTO referenced by
  another module's DTO, or a DTO returned from `IXModule`).

## Naming

- Project: `Concertable.<Module>.<Layer>` (e.g. `Concertable.Artist.Application`).
- Namespace: rename as you move. **Do not** leave `Concertable.Application.*` on files that now live
  inside a module — Identity did this as a shortcut and paid for it later (see `IDENTITY_COMPLETION.md §4`).
- Module facade: `IXModule` in Contracts. Implementation `XModule` in Infrastructure.
  **The facade is the cross-module surface only** — the methods OTHER modules (Concert, Identity,
  Payment, etc.) call to look things up. It is intentionally minimal and stable: the shape you'd
  turn into an HTTP/Refit contract if this module extracted to a microservice. HTTP endpoints
  the module itself exposes are backed by `IXService` inside `Module.Application` and called by
  controllers in `Module.Api`, **not** via `IXModule`. Don't put `CreateAsync`/`UpdateAsync` on
  `IXModule` just because the Web UI uses them — other modules don't create or update foreign
  aggregates, only the owning module's controllers do.
- **A module can expose multiple facades.** Don't force everything through one fat `IXModule`. Split
  by concern when it reads cleaner — Identity ships both `IIdentityModule` (user lookups) and
  `IAuthModule` (auth flows) in `Identity.Contracts`. Each is its own `IYModule` + `YModule` pair.
  (Identity's `AuthController`/`UsersController` currently still bind to `IAuthModule` directly —
  that's carryover from the pre-Api-pilot shape. `IAuthModule` should shrink to cross-module auth
  lookups only; controller-facing concerns belong on an internal `IAuthService` in
  `Identity.Application`. Deferred until we touch Auth flows again.)

## Integration events come from domain events

Cross-module integration events (`XChangedEvent`, etc.) must be raised from an entity domain event,
not published directly from service or seeder code.

- Entities implement `IEventRaiser` (`private readonly EventRaiser _events = new();`).
- State-change methods (`Create` / `Update` / `Approve` / `SyncGenres`) raise
  `XChangedDomainEvent(this)` after mutating. The domain event carries the entity reference, not an
  ID primitive — on `Create` the `Id` is 0 at raise time and only gets populated during
  `SaveChanges`; the handler reads live state at dispatch time.
- `internal class XChangedDomainEventHandler(IIntegrationEventBus bus) : IDomainEventHandler<XChangedDomainEvent>`
  in `Module.Infrastructure/Events/` translates to the integration event. Registered in
  `AddXModule()`.
- `DomainEventDispatchInterceptor` is wired on every module `DbContext`, so every
  `SaveChangesAsync` — service, seeder, worker — fires the handler exactly once.
- Seeders construct entities via the factory method so the event raises. `VenueFaker` / `ArtistFaker`
  use `.CustomInstantiator(f => XEntity.Create(...))`, not the reflection `New<XEntity>()` path.
- A service-layer `eventBus.PublishAsync(new XChangedEvent(...))` is a bug — the entity should be
  raising it. The only legitimate `eventBus.PublishAsync` callers are domain-event handlers.

Reference impls: `VenueEntity` + `VenueChangedDomainEvent` + `VenueChangedDomainEventHandler` in
the Venue module; same shape for Artist; `UserEntity.UpdateLocation` +
`UserLocationUpdatedDomainEventHandler` in Identity (primitive payload — works there because the
entity already has an Id at raise time).

## When in doubt

- **Ask the user.** A cross-module reference that isn't through Contracts is a design decision. The
  only currently-accepted exception is Core/module → Module.Domain for entity types (see rule 3).
  Any other cross-module Application/Infrastructure reference needs an explicit conversation.

## Reference implementations in-repo

- `Concertable.Search.*` — query-side module, skips Domain (no owned writes).
- `Concertable.Identity.*` — full 4-layer module, includes `IIdentityModule` + `IAuthModule` facades
  and `ICurrentUser` in Contracts.

## Staged plans (do not delete)

- `ARTIST_MODULE_REFACTOR.md` — active.
- `MM_NORTH_STAR.md` — corollaries + long-term targets (rating-pipeline rewrite, per-module review
  tables, read-model replacement for Module.Domain references).
- Memory: `project_modular_monolith.md` for agreed stage progress.
