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
```

## Cross-module rules — the short version

1. **Contracts is the only module a foreign caller references.** Public interfaces, module facade
   (`IXModule`), and cross-module summary DTOs live here. Nothing else.
2. **Do not reference another module's `Application` or `Infrastructure`.** If you think you need to,
   stop and consult the user — this is a design decision, not a shortcut.
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
- EF `IEntityTypeConfiguration<T>` files stay in `Concertable.Data.Infrastructure/Data/Configurations/`
  (matches Identity precedent — keeps `ReadDbContext`'s single `ApplyConfigurationsFromAssembly`
  call working). Module DbContexts import + apply them explicitly.
- Composite keys, owned types, `geography` columns, etc. configured via Fluent API so Domain stays
  free of EF Core attributes beyond `System.ComponentModel.DataAnnotations.Schema` BCL attributes.

## Internal enforcement

- Everything in `Module.Application` and `Module.Infrastructure` should be `internal`. Only
  `Module.Contracts` and the `AddXModule()` extension are public.
- Compiler-enforced boundary, not a naming-convention hope.
- Remove Identity/Artist/etc internals from `Concertable.Web/GlobalUsings.cs` — Web imports from
  `Module.Contracts` only.

## Naming

- Project: `Concertable.<Module>.<Layer>` (e.g. `Concertable.Artist.Application`).
- Namespace: rename as you move. **Do not** leave `Concertable.Application.*` on files that now live
  inside a module — Identity did this as a shortcut and paid for it later (see `IDENTITY_COMPLETION.md §4`).
- Module facade: `IXModule` in Contracts. Implementation `XModule` in Infrastructure.
- **A module can expose multiple facades.** Don't force everything through one fat `IXModule`. Split
  by concern when it reads cleaner — Identity ships both `IIdentityModule` (user lookups) and
  `IAuthModule` (auth flows) in `Identity.Contracts`. Each is its own `IYModule` + `YModule` pair.

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
