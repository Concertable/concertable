# Big review — Feature/tenant-scoping

**Plan anchored to commit:** `7c53b091a5776ef811b30864c281692f9438e721`  _(2026-06-12)_
Net diff reviewed: `927ac86c..7c53b091` (362 files, +5783/−1936). Move-only files skipped.
Status legend: `[ ]` not yet reviewed · `[x]` reviewed (date) · `[~]` in progress.

## Coverage
- [x] Shared foundation — `api/Shared/Concertable.Kernel`, `api/Shared/Seed`, `api/Concertable.Messaging/`, `api/Concertable.DataAccess/` (2026-06-12)
- [x] B2B tenancy core — `api/Concertable.B2B/Modules/Tenant`, `Modules/Organization`, `Modules/User` (2026-06-12)
- [ ] B2B domain modules — `Modules/Concert`, `Modules/Contract`, `Modules/Venue`, `Modules/Artist`, `Modules/Conversations`
- [ ] B2B service shell — rest of `api/Concertable.B2B/`: DataAccess, Web, Workers, Seed, Tests, AppHost, slnx, ARCHITECTURE.md, TENANCY_DESIGN.md, plans
- [ ] Customer service — `api/Concertable.Customer/`
- [ ] Adapters — `api/Concertable.Payment/`, `api/Concertable.Auth/`, `api/Concertable.Search/`
- [ ] Seed + infra + tests + frontend — AppHosts, `api/Concertable.slnx`, `api/docs/`, root docs, `api/initial-migrations.ps1`, `plans/`, `api/Shared/Tests`, `app/web`

## Cross-area notes
<!-- one-liners added during a stage for things a LATER stage must verify; struck through with the outcome when checked -->

- **→ B2B service shell**: verify `TenantResolutionMiddleware` (B2B.Web) is registered after auth and before endpoints, and that the Workers host (no `HttpContext`) really takes the `IsHost` bypass for outbox/event processing.
- **→ B2B service shell**: `TenantDevSeeder`'s premise is that tenant seeding runs *before* B2B consumes `CredentialRegisteredEvent`. If the Workers host can process Auth's seed events before `DevDbInitializer` finishes, `TenantProvisioningHandler` creates tenants with **random** ids and `SeedIfEmptyAsync` then skips the deterministic ones → seeded venues' `TenantId` (from `TenantSeedIds.For`) points at nothing. Verify the startup/host ordering actually guarantees seed-first.
- **→ Adapters (Payment)**: `TenantCreatedHandler` has no inbox/idempotency guard and `Provision*Async` unconditionally creates Stripe objects (relevant to BUG2); also `api/Concertable.Payment/CLAUDE.md` still documents `ManagerRegisteredHandler` provisioning managers per-user on `CredentialRegisteredEvent` — stale after the tenant-owner switch.

## Findings
<!-- appended per area; finding IDs continue across areas: MS#, MB#, BUG#, SEED#, CV# -->

## Shared foundation — reviewed 2026-06-12

No issues found in this area. Checked correctness, microservice isolation, module boundaries, seeding, and C# conventions across the Kernel tenancy abstractions (`ITenantScoped`, `ITenantContext`, `ITenantResolver`), the shared `TenantInterceptor`, the repository `CancellationToken` plumbing, the `FindAsync`→`FirstOrDefaultAsync` switch (intentional — guarantees tenant query filters always apply, where `FindAsync`'s change-tracker shortcut would bypass them), `TenantSeedIds`/`SeedManager`, and the Messaging migration timestamp renames (move-only).

Candidates examined and dropped below the confidence bar:
- *Tenancy types in shared Kernel/DataAccess while only B2B consumes them* — the plan doc (`TENANT_SCOPING_PLAN.md` §3.2) explicitly places the bare markers next to `IAuditable` as an opt-in mechanism, and keeps the genuinely B2B-specific `IVenueArtistTenantScoped` out of Kernel; this is not the documented `ICurrentUser`-member anti-pattern.
- *`TenantInterceptor` skips Modified entries when `TenantId` is unresolved, and never checks Deleted entries* — unreachable in practice: the named tenant query filters fail closed (`null` tenant matches no rows), so a cross-tenant entity can't be loaded for modify/delete without an explicit `IgnoreQueryFilters`.

## B2B tenancy core — reviewed 2026-06-12

Scope: Organization module deleted, new Tenant module (`tenant` schema, provisioning off `CredentialRegisteredEvent`, `/api/organizations` surface, `TenantContext` resolver), User module reworked onto the `Repository<T>` base + `owner` claim. 87 files, +1797/−636; pure migration-timestamp renames skipped.

- [ ] **BUG1 — HIGH — correctness (CI/test scripts)** — `unit.ps1:14`, `integration.ps1:14`, `.github/workflows/test.yml:38,70`
  All three still list `Concertable.B2B.Organization.UnitTests` / `Concertable.B2B.Organization.IntegrationTests`, deleted by this branch — `dotnet test` on a nonexistent csproj fails the unit and integration jobs (and the local scripts). The new `Concertable.B2B.Tenant.UnitTests` / `.IntegrationTests` projects are also missing from all three lists, so even after removing the stale entries the Tenant tests would never run in CI.

- [ ] **BUG2 — MEDIUM — correctness (event flow)** — `TenantProvisioningHandler.cs:42` + `TenantDevSeeder.cs:26` + `TenantEntity.cs:49`
  Every seeded tenant publishes `TenantCreatedEvent` **twice** in a fresh dev run. Seeded `TenantEntity` instances still carry the `TenantCreatedDomainEvent` raised in `Create` (deliberate — `TenantFactory`'s comment wants the seed id in the event), so `TenantDevSeeder`'s `SaveChanges` publishes once; then the manager's `CredentialRegisteredEvent` arrives, the handler finds the tenant present and `Announce()`s → second publish. The handler's justification — "Payment's own inbox dedups" — is wrong: `MessageEnvelope` mints a fresh `MessageId` per publish (`MessageEnvelope.cs:10`), so Payment's inbox can't dedup the pair, and the real `StripeAccountClient.Provision*Async` (`StripeAccountClient.cs:47-79`) unconditionally creates a **new** Stripe customer + Express account per message → duplicate/orphaned Stripe objects per seeded tenant in dev (masked in E2E only because `E2EStripeAccountClient` links pre-seeded ids idempotently). `TenantDevSeeder`'s own comment says the registration handler "finds them already present and no-ops", contradicting the re-announce. One of the two publish paths has to go: either clear domain events on seed insert and let `Announce()` be the single provisioning trigger, or make the handler skip when the tenant exists (as the seeder comment claims it does).

- [ ] **BUG3 — LOW — correctness (contract field)** — `TenantEntity.cs:40,49` → `TenantCreatedEvent.cs`
  `TenantCreatedEvent.Email` is populated with `LegalName` in both `Create` (the `legalName` param goes into the `Email` slot) and `Announce()`. It only works because legal name *is* the registration email until organization setup; after `UpdateLegalDetails` replaces it, any future `Announce()` would hand Payment a company name (e.g. "The Grand Venue Ltd") as the Stripe account email on a versioned contract (`concertable.b2b.tenant-created.v1`). The entity should carry the creator's email explicitly instead of aliasing `LegalName`.

- [ ] **CV1 — LOW — C# conventions** — `TenantDevSeeder.cs:24-25`
  Stacked two-line `//` comment — `CODE_CONVENTIONS.md` ("Comments — short, and multi-line uses `/* */`") names this exact shape as the anti-example. Its content also misstates the handler behaviour (see BUG2), so it needs a rewrite anyway.

Candidates examined and dropped below the confidence bar:
- *`TenantDevSeeder`/`TenantTestSeeder` direct-insert rows whose only production write is an event handler* — the design routes seed construction through `TenantEntity.Create`, so the same domain event → outbox path still fires (that's BUG2's other half); the direct insert exists for deterministic ids that seeded venues FK to. Deliberate, documented in the entity/factory comments — not the banned bypass shape.
- *`TenantController` is `internal`* — contradicts `MODULAR_MONOLITH_RULES.md` ("controllers are public"), but all 14 existing module controllers are `internal` registered via `AddInternalControllers`; the doc note is stale, not the diff.
- *Registering the pre-commit handler as `IDomainEventHandler<T>`* — correct: `DomainEventDispatcher` resolves `IDomainEventHandler<>` and phase-filters by runtime `IPreCommitDomainEventHandler<>` check; matches every other module.
- *`TenantDbContext` primary constructor* — the no-primary-ctor rule covers services/repos/handlers/validators; all module DbContexts use primary ctors.
- *No unique index on `Tenants.CreatedByUserId`* (one-tenant-per-operator enforced only by lookup) — concurrent duplicate creation is blocked by the inbox row's key within the same transaction; defense-in-depth suggestion, not a hit-in-practice bug.
