# Big review — Feature/payment-proxy

**Plan anchored to commit:** `aa5ccef01ce7fa0a0d198648e34bf3c34f45854f`  _(2026-06-21)_
Net diff reviewed: `6c37c5b6..aa5ccef0` (170 files, +2220/-493). Move-only files skipped.
Status legend: `[ ]` not yet reviewed · `[x]` reviewed (date) · `[~]` in progress (incomplete — re-review).

Feature: USER_MODEL_PLAN Phases 1–5 — tenant membership table + persona, permission
policies replacing `[VenueManager]`/`[ArtistManager]`, ownership `UserId → TenantId`,
active-tenant resolution + multi-membership, and the Stripe payout **proxy** (B2B → Payment
adapter over gRPC) + removal of the B2B owner claim.

## Summary — big review complete (all 7 areas reviewed, 2026-06-21)

**13 findings, none a production-breaking bug.** By severity: **2 MEDIUM** (`MB1`, `MB3` — both module-boundary
calls, not correctness), **11 LOW** (`MB2`, `BUG1`, `BUG2`, `CV1`–`CV7`, `DOC1`). By lens: microservice isolation
**0** (the headline risk — B2B↔Customer↔Payment — is clean: sync gRPC to the Payment *adapter* is allowed, Payment
stays tenancy-agnostic, Customer needed no change); module boundaries **3**; correctness **2** (both LOW/latent);
seeding **0**; C# conventions **7** (six are the same stacked-`//`-vs-`/* */` / `base.`-qualifier nits); docs **1**.

The feature is architecturally sound — the payout proxy, persona/permission cutover, active-tenant resolution,
and `owner`-claim removal all hold up; migrations were re-scaffolded not additive; seeding stays event-driven.

**The handful worth acting on:**
- **`MB1` (MEDIUM, decide consciously)** — `[HasPermission]` lives in `Tenant.Api`, so every gating module took a
  module-Api→module-Api `ProjectReference` on `Tenant.Api` (a coupling `MODULAR_MONOLITH_RULES.md` doesn't permit;
  the permission *model* is already in `Tenant.Contracts`). Spreads as more modules adopt it. Move the attribute to
  `Tenant.Contracts` or a shared authz lib.
- **`MB3` (MEDIUM, decide consciously)** — the tenant store landed in *universal* `app/web/shared` and now compiles
  into the customer bundle; inject via `configure*Api` instead so universal shared never names "tenant".
- **`MB2` + `BUG1` (LOW, trivial)** — drop `Venue.Api`'s dead `User.Api` reference; make `MockPayoutAccountClient`
  `IResettable` to stop `LastOwnerId` leaking across tests. Both are safe one-liners CI won't catch.
- Everything else (`CV1`–`CV7`, `BUG2`, `DOC1`) is LOW/borderline — convention spelling, a latent Phase-6 logout
  cleanup, and a doc-wording fix; dismiss or sweep at leisure.

All Cross-area notes are resolved.

## Resolution — addressed 2026-06-21

All findings actioned. Backend `dotnet build` green + Tenant unit tests 53/53; all four web builds green (the
`tsc -b` boundary gate). Finding checkboxes ticked below.

- **CV1–CV3 (Payment)** — `ToProtoStatus` moved to `PaymentMappers` (the gRPC service calls
  `status.ToProtoStatus()`); `request.OwnerId.ParseOrThrow<Guid>(…)` replaces raw `Guid.Parse` (×4); the HTTP
  `StripeAccountController` WHY-comment is now a `/* */` block.
- **MB1** — `HasPermissionAttribute` moved to `Tenant.Contracts` (added the `Microsoft.AspNetCore.App`
  FrameworkReference, matching shared `Concertable.Contracts`). `Concert.Api`/`Artist.Api`/`Venue.Api` now
  reference `Tenant.Contracts` instead of `Tenant.Api`; the stale `using …Tenant.Api.Authorization;` dropped
  from all 8 controllers. The module-Api→module-Api coupling for permissions is gone.
- **MB2 — FALSE POSITIVE, not actioned.** `VenueController.Approve` uses `[Admin]`, which lives in
  `Concertable.B2B.User.Api.Authorization` — so `Venue.Api`'s `User.Api` reference and `using` are **live**, not
  dead (the review's grep missed the `[Admin]` usage). Removing them breaks the build; reference retained.
- **MB3 + BUG2 (frontend)** — active-tenant store + `TENANT_HEADER` moved to `app/web/b2b/shared/features/tenant`;
  universal `shared/lib/axios`+`paymentAxios` reverted to clean auth-only. New `app/web/b2b/shared/lib/b2bAxios.ts`
  is the B2B client setup: configures the shared `api`+`paymentApi` instances for the B2B host, stamps
  `X-Tenant-Id` (covers the payout proxy), and clears the store on `UserUnloaded` (BUG2). venue+artist import it
  instead of `shared/lib/axios`/`paymentAxios`. Tenant code no longer compiles into the customer bundle.
- **BUG1** — `MockPayoutAccountClient` is now `: IResettable` (`Reset()` nulls `LastOwnerId`) and added to
  `AddResettables(…)`.
- **CV4–CV7** — single-statement `else` braces dropped; stacked-`//` WHY-comments converted to `/* */` (User
  claims controller + service extensions, Tenant provisioning test); `base.CurrentTenant` qualifier in the
  Artist/Venue repos.
- **DOC1** — `e2e.ps1` + the regress skill reworded: the flaky-stack retry runs the failures *together on a
  single freshly-booted stack* (separate from the full run's), not one-per-scenario.
- **Incidental build fix (not a review finding)** — a pre-existing uncommitted working-tree change had stripped
  the direct `Concertable.B2B.Artist.Infrastructure` ProjectReference from `Artist.Api`, `Workers`, and
  `E2ETests`, which only still compiled via a transitive crutch through the old `Tenant.Api` reference. MB1
  removed that crutch and exposed the breakage; the three references were restored to their committed (HEAD) state.

## Coverage
<!-- dependency order, foundation first. Each item lists exact `git diff -- <paths>` globs. -->
- [x] **Payment proxy (adapter service + gRPC client)** — ~15 files — `api/Concertable.Payment/` _(reviewed 2026-06-21)_
- [x] **Tenant module core (contracts, domain, infra, API)** — ~38 files — `api/Concertable.B2B/Modules/Tenant/Concertable.B2B.Tenant.Api/` `api/Concertable.B2B/Modules/Tenant/Concertable.B2B.Tenant.Application/` `api/Concertable.B2B/Modules/Tenant/Concertable.B2B.Tenant.Contracts/` `api/Concertable.B2B/Modules/Tenant/Concertable.B2B.Tenant.Domain/` `api/Concertable.B2B/Modules/Tenant/Concertable.B2B.Tenant.Infrastructure/`
- [x] **B2B identity wiring (User module + Web host)** — ~13 files — `api/Concertable.B2B/Modules/User/` `api/Concertable.B2B/Concertable.B2B.Web/` _(reviewed 2026-06-21)_
- [x] **Other B2B modules + Seed (consumers)** — ~45 files — `api/Concertable.B2B/Modules/Artist/` `api/Concertable.B2B/Modules/Concert/` `api/Concertable.B2B/Modules/Venue/` `api/Concertable.B2B/Modules/Contract/` `api/Concertable.B2B/Modules/Conversations/` `api/Concertable.B2B/Seed/` _(reviewed 2026-06-21)_
- [x] **Cross-service migrations & infra (Customer, Search, Messaging, Auth)** — ~25 files — `api/Concertable.Customer/` `api/Concertable.Search/` `api/Concertable.Messaging/` `api/Concertable.Auth/` _(reviewed 2026-06-21)_
- [x] **Tests (Tenant tests + B2B integration fixtures)** — ~13 files — `api/Concertable.B2B/Modules/Tenant/Tests/` `api/Concertable.B2B/Tests/` _(reviewed 2026-06-21)_
- [x] **Frontend, docs, plans, config** — ~18 files — `app/` `api/docs/` `plans/` `CLAUDE.md` `api/CLAUDE.md` `e2e.ps1` `.claude/` _(reviewed 2026-06-21)_

## Cross-area notes
<!-- one-liners added during a stage for things a LATER stage must verify. -->
- ~~**[Tenant stage]** Verify `Concertable.B2B.Tenant.Api/StripeAccountController` fronts Payment via `IPayoutAccountClient`, passes the active tenant id from `ITenantContext` as `ownerId`, and does **not** mint an `owner` claim.~~ **RESOLVED (Tenant stage):** controller uses `IPayoutAccountClient` (gRPC to the Payment adapter), passes `Tenant` = `tenantContext.TenantId` (fail-closed) to all four ops, gated `[HasPermission(Permissions.PayoutsManage)]`, mints no claim.
- ~~**[Tenant stage]** Verify the tenant's Stripe `PayoutAccount` is provisioned by `TenantCreatedEvent` → Payment `TenantCreatedHandler`, event-driven, never seeded.~~ **RESOLVED (Tenant stage):** `TenantProvisioningHandler` → `TenantEntity.Create` raises `TenantCreatedDomainEvent` (→ `TenantCreatedEvent`); seeders write only the founding tenant + Owner membership (documented exception), never a `PayoutAccount`. Payment-side provisioning was confirmed in Stage 1.
- ~~**[Cross-service/Customer stage]** Confirm Customer still calls the HTTP `StripeAccountController` directly with `owner` = the buyer's own id (the HTTP controller now serves only Customer). No Customer runtime change appeared in the diff — just confirm nothing relied on the old B2B path.~~ **RESOLVED (Cross-service stage):** every Customer file in the diff is a migration rename — **zero Customer runtime change**. Auth `Config.cs` keeps the `concertable.customer.api` resource's `UserClaims = { "role", "owner" }` unchanged, so Customer still mints `owner` = the buyer's own id via the scope it already requests. The B2B change (dropping `owner`, fronting Payment over gRPC) is entirely B2B-side; Customer never read a B2B-issued token, so nothing relied on the old path.
- ~~**[Frontend stage]** `app/web/shared/src/lib/paymentAxios.ts` is new — verify SPA payout calls route to the B2B proxy correctly, and that the wire contract matches (`PayoutAccountStatus` member names serialized as strings, `SavedCard` property names — both declared in `Concertable.Payment.Client`).~~ **RESOLVED (Frontend stage):** wire contract holds. (1) Routing: the B2B vite configs swap only `VITE_PAYMENT_API_URL` 7088→7086 (B2B host); the proxy `StripeAccountController` is `[Route("api/[controller]")]` mirroring Payment's endpoints, so the unchanged SPA `stripeAccountApi` paths resolve against the new base. Customer's vite stays 7088. (2) Enum: SPA matches `accountStatus === "Verified"` (string); `PayoutAccountStatus` returns from the proxy and **B2B.Web `Program.cs:66` registers `JsonStringEnumConverter` globally**, so it serializes as the member name — matches. (3) `SavedCard`: both hosts use default camelCase property naming → matches. The new boundary smudge this introduced (tenant store landing in universal shared) is **MB3** below.
- ~~**[Other B2B modules stage]** MB1 consumer half: `Concert.Api`, `Venue.Api`, `Artist.Api` each added a **new** `ProjectReference` to `Concertable.B2B.Tenant.Api` (Api→Api) purely to consume `[HasPermission]`. Verify the `[HasPermission(...)]` usages there and confirm whether that cross-module Api coupling is intended or should reference a Contracts-level attribute (see MB1).~~ **RESOLVED (Other B2B modules stage):** all `[HasPermission(Permissions.X, TenantType.Y)]` usages verified sensible (Artist controllers → `TenantType.Artist`, Venue/Concert/Opportunity/Application-decide → `TenantType.Venue`, etc.). `Artist.Api` and `Concert.Api` **replaced** their single existing `User.Api` ref with `Tenant.Api` (coupling-count-neutral, Api→Api retargeted). `Venue.Api` **added** `Tenant.Api` but kept a now-dead `User.Api` ref → **MB2**. The systemic Api→Api coupling itself is captured by **MB1**.
- ~~**[B2B identity wiring stage]** (a) `B2B.Web` also references `Tenant.Api`; verify the permission pipeline is registered in the host (`PermissionPolicyProvider` singleton + `PermissionAuthorizationHandler` scoped) and that `Admin`/bare `[Authorize]` still resolve via the delegated `DefaultAuthorizationPolicyProvider` — and that nothing registers a *competing* `IAuthorizationPolicyProvider`. (b) `ITenantModule.GetTenantIdByUserIdAsync` was renamed to `GetMembershipsAsync` (returns `IReadOnlyList<MembershipDto>`); confirm `/api/auth/me` (or whoever consumed the old method) now reads memberships and that **no** caller still references the removed method.~~ **RESOLVED (B2B identity wiring stage):** (a) host `Program.cs` calls `AddTenantApi` → `AddTenantModule`, which registers `PermissionPolicyProvider` as the **single** `IAuthorizationPolicyProvider` (only `AddSingleton<IAuthorizationPolicyProvider,…>` in the repo — wins over `AddAuthorization`'s `TryAdd` default regardless of order) + `PermissionAuthorizationHandler` scoped; `PermissionPolicyProvider` delegates non-`perm:` names (incl. `Admin`, bare `[Authorize]`) to a `DefaultAuthorizationPolicyProvider` fallback. (b) `GetTenantIdByUserIdAsync` has **zero** remaining `.cs` callers (only docs); `UserController.Me` now returns `user with { Memberships = await tenantModule.GetMembershipsAsync(...) }`. Also confirmed the `owner` claim is no longer minted (`UserClaimsController` returns only `role`) and **no** B2B code still reads an `owner` claim.
- ~~**[Other B2B modules + Seed stage]** Verify `seed.Memberships` (consumed by `TenantDevSeeder`/`TenantTestSeeder`) contains **only founding-Owner** memberships — the one documented direct-insert exception. Any non-founding (invitation-derived) membership must come from the accept flow / handler, never a seeder (`SEEDING_CONVENTIONS.md`).~~ **RESOLVED (Other B2B modules + Seed stage):** `SeedState.Memberships = SeedUsers.Managers.Select(m => MembershipFactory.FoundingOwner(m.TenantId, m.Id, now))` — `FoundingOwner` produces only `TenantRole.Owner`, `invitedBy: null`, one row per manager. No invitation-derived membership is seeded. Also confirmed `m.TenantId == TenantSeedIds.For(m.Id)` (the same id `TenantFactory.Create` stamps), so each founding membership points at its real seeded tenant, and the deterministic membership id makes the seeder re-runnable + dedup-safe against the provisioning handler.

## Findings
<!-- appended per area; finding IDs continue across areas: MS#, MB#, BUG#, SEED#, CV# -->

## Payment proxy (adapter service + gRPC client) — reviewed 2026-06-21

Architecturally clean. The Stripe-account logic was extracted from the HTTP `StripeAccountController`
into a shared `IPayoutAccountService`, now exposed two ways: HTTP (Customer, `owner` = buyer's own id
from the claim) and the new `PayoutAccount` gRPC service (B2B's proxy, `owner` = tenant id). Sync gRPC
to an **adapter** service is explicitly allowed (`api/ARCHITECTURE.md`), Payment stays tenancy-agnostic
(owner id is always passed in, never read from a claim), `IPayoutAccountClient` correctly lives in
`Concertable.Payment.Client`, and the gRPC endpoint is gated with `RequireAuthorization("ServiceToken")`
like its siblings. No correctness, microservice-isolation, module-boundary, or seeding issues.

- [x] **CV1 — LOW — C# conventions** — `api/Concertable.Payment/Concertable.Payment.Infrastructure/Grpc/PayoutAccountGrpcService.cs:50`
  The `PayoutAccountStatus → PayoutAccountStatusType` mapping is a **private static `ToProto` helper on the gRPC service**. `CODE_CONVENTIONS.md` ("Mappers — `XMappers` extension methods") states proto⇄domain mapping lives in a static `XMappers` class as `ToTarget()` extensions, "not as private `MapX` helpers on the consumer". Direct sibling precedent: `EscrowMappers.ToProtoStatus(this EscrowStatus s)` in the same folder. Move it to `PaymentMappers.cs` (infra) as `ToProtoStatus(this PayoutAccountStatus s)` and call `status.ToProtoStatus()`. (The *client* side did this right — `ToStatus` was added to `Concertable.Payment.Client`'s `PaymentMappers`; only the server side regressed.)

- [x] **CV2 — LOW — C# conventions / robustness** — `api/Concertable.Payment/Concertable.Payment.Infrastructure/Grpc/PayoutAccountGrpcService.cs:19,25,31,46`
  Parses the request id with raw `Guid.Parse(request.OwnerId)` (4×). Every other gRPC request mapper in this folder uses the established `request.X.ParseOrThrow<Guid>(nameof(request.X))` helper (`GrpcRequestParsers.cs`), which returns a clean `InvalidArgument` `RpcException`. Raw `Guid.Parse` throws `FormatException` on a malformed id, surfacing as an opaque gRPC `Unknown`/`Internal`. Input is a controlled tenant Guid in practice (defensive, not a live bug), but it's an unnecessary deviation from the one-line convention.

- [x] **CV3 — LOW (minor) — C# conventions** — `api/Concertable.Payment/Concertable.Payment.Api/Controllers/StripeAccountController.cs:10-12`
  The new WHY-comment is three stacked `//` lines; `CODE_CONVENTIONS.md` ("Comments — short, and multi-line uses `/* */`") says a genuinely multi-line comment uses one `/* */` block, never stacked `//`. Content is good — just the spelling. Borderline-pedantic; flagged only because the doc is explicit.

## Tenant module core (contracts, domain, infra, API) — reviewed 2026-06-21

The foundation stage: tenant **persona** (`TenantType`, fixed at provisioning from the registration
client-id), the **membership** table + entity, the role→permission **catalog** (`PermissionCatalog`,
a `FrozenDictionary` matrix), the on-demand **string-permission authorization** pipeline
(`[HasPermission]` → `PermissionPolicyProvider` → `PermissionRequirement` → `PermissionAuthorizationHandler`),
request-scoped **active-tenant resolution** (`TenantContext` now also implements `IMembershipContext`,
resolving the active membership from the DB per request via the `X-Tenant-Id` header / sole-membership
default / multi-membership fail-closed), and B2B's **payout proxy** controller.

Isolation, correctness, and seeding are clean — both Tenant-stage cross-area notes are confirmed
resolved above. Highlights checked and sound: the payout proxy resolves the owner from `ITenantContext`
and never mints a claim; the Stripe `PayoutAccount` is provisioned event-driven, never seeded, and the
founding Owner membership is re-ensured idempotently over the seeded row; `TenantContext`'s three
interfaces share one scoped instance so the memoized resolution (`resolved=true` set before the early
returns, `TenantContext.cs:54`) is shared and fails closed consistently; the migration was re-scaffolded
(`InitialCreate` renamed), not additive; the repository inherits the module `Repository<T>` base and
adds only the extra membership finders; no inline logging, no primary ctors on services/handlers, no
cross-module runtime query (the `Memberships`⋈`Tenants` join is within the module's own context).

- [x] **MB1 — MEDIUM — module boundaries** — `api/Concertable.B2B/Modules/Tenant/Concertable.B2B.Tenant.Api/Authorization/HasPermissionAttribute.cs`
  The public `[HasPermission]` attribute lives in the Tenant **Api** layer, so every other B2B module
  that gates an endpoint on a permission now takes a `ProjectReference` on
  `Concertable.B2B.Tenant.Api` — added **new in this branch** to `Concert.Api`, `Venue.Api`,
  `Artist.Api` (and `B2B.Web`). That is a module-Api → module-Api dependency, which the
  `MODULAR_MONOLITH_RULES.md` reference graph does not permit (`Api → Application, Contracts, Kernel,
  ASP.NET` only; cross-module surface belongs in `Contracts`). The permission *model* it needs is
  already correctly in `Tenant.Contracts` (`Permissions`, `PermissionPolicy`, `TenantType`) — only the
  attribute landed in Api, dragging the whole Tenant Api assembly (controllers, incl. the payout proxy)
  onto its consumers' compile graph. Move `HasPermissionAttribute` to `Tenant.Contracts` (it needs only
  `Microsoft.AspNetCore.Authorization`'s `AuthorizeAttribute` + `PermissionPolicy`, both reachable
  there) or a small shared B2B authz lib, so consumers reference the documented cross-module surface.
  May be a conscious tradeoff (Tenant as the authz-foundation module), but the coupling spreads as more
  modules adopt `[HasPermission]`. (Consumer halves flagged for the "Other B2B modules" / "B2B identity
  wiring" stages via Cross-area notes.)

- [x] **CV4 — LOW (borderline) — C# conventions** — `api/Concertable.B2B/Modules/Tenant/Concertable.B2B.Tenant.Infrastructure/Events/TenantProvisioningHandler.cs:75-78`
  The `else` branch is a single statement (`tenant.Announce();`) wrapped in braces; `CODE_CONVENTIONS.md`
  ("Single-statement branches — no braces") says drop them. Borderline because the sibling `if` branch
  is multi-statement and *needs* braces, so matched braces on the `else` is a defensible symmetry call
  many reviewers leave — flagged only because the doc states the rule absolutely (mirrors CV3). Dismiss
  freely if you prefer the symmetry.

## B2B identity wiring (User module + Web host) — reviewed 2026-06-21

The host + identity half of the persona/permission cutover. `B2B.Web` moves `TenantResolutionMiddleware`
to sit **between** `UseAuthentication` and `UseAuthorization` (so the `PermissionAuthorizationHandler` reads
an already-resolved `ITenantContext`) and converts it to a factory-based `IMiddleware` (now `AddScoped`-registered,
ctor-injecting `ITenantResolver`). The User module drops `owner`-claim minting (`UserClaimsController` returns
only `role`), deletes the `[VenueManager]`/`[ArtistManager]` attributes + their profile handlers (keeping only
the tenancy-orthogonal `Admin` policy), and `UserController.Me` now hydrates `UserDto.Memberships` from
`ITenantModule.GetMembershipsAsync`.

Both this stage's cross-area notes are confirmed **resolved** above. Correctness, isolation, and seeding are
clean. Highlights verified:

- **Middleware ordering + lifetime** — the move to between auth/authorization is correct and necessary (the
  permission handler needs the resolved tenant); the `IMiddleware` conversion is sound — registered scoped
  (`Program.cs:186`) so `UseMiddleware<T>` resolves it per request from the request scope via `IMiddlewareFactory`;
  `ITenantResolver` (scoped) injected by ctor with no captive-dependency hazard.
- **Permission pipeline is wired in the host** — `Program.cs` `AddTenantApi` → `AddTenantModule` registers the
  **single** `IAuthorizationPolicyProvider` (`PermissionPolicyProvider`, singleton) + `PermissionAuthorizationHandler`
  (scoped); the provider delegates `Admin`/bare `[Authorize]` to a `DefaultAuthorizationPolicyProvider` fallback;
  no competing provider exists.
- **`owner`-claim removal is safe** — `GetTenantIdByUserIdAsync` has zero `.cs` callers, and no B2B code reads an
  `owner` claim (`ICurrentPayoutOwner`/`GetOwnerId`/`"owner"` all absent in `Concertable.B2B`); the payout proxy
  passes the tenant id over gRPC instead (Stage 1/2).
- **Module boundaries hold** — `User.Contracts` → `Tenant.Contracts` (for `MembershipDto`) is the documented
  "Contracts → other Contracts when sharing base types" case (`MODULAR_MONOLITH_RULES.md`), and there is **no
  cycle** (`Tenant.Contracts` references only `Concertable.Contracts`/`Kernel`/`Messaging.Contracts`, never
  `User.Contracts`). `UserController` → `ITenantModule` is a cross-module call through the facade (Api → Contracts),
  allowed. No new MB finding — distinct from MB1 (which is Api → Api).
- **Migration re-scaffolded, not additive** — `InitialCreate` re-stamped `20260612…` → `20260620…`; the User
  model's `ArtistManagerProfileEntity`/`VenueManagerProfileEntity` *tables* correctly remain (only the authz
  attributes/handlers were deleted, not the profile entities).

- [x] **CV5 — LOW (borderline) — C# conventions** — `api/Concertable.B2B/Modules/User/Concertable.B2B.User.Api/Controllers/UserClaimsController.cs:14-17` and `api/Concertable.B2B/Modules/User/Concertable.B2B.User.Infrastructure/Extensions/ServiceCollectionExtensions.cs:56-58`
  Two new WHY-comments are written as 3–4 stacked `//` lines; `CODE_CONVENTIONS.md` ("Comments — short, and
  multi-line uses `/* */`") says a genuinely multi-line comment uses one `/* */` block, never stacked `//`. Both
  comments' *content* is good and earns its place (why `owner` is no longer minted; why only `Admin` survives the
  sweep) — only the spelling deviates. Same minor deviation as CV3; flagged for consistency, dismiss freely.

## Other B2B modules + Seed (consumers) — reviewed 2026-06-21

The consumer half of the persona/permission cutover across `Artist`, `Venue`, `Concert` (+ `Seed`);
`Contract` is a docs-only edit and `Conversations` is untouched. Three mechanical shifts, all sound:
(1) the `[VenueManager]`/`[ArtistManager]` attributes are swapped for `[HasPermission(Permissions.X,
TenantType.Y)]`; (2) ownership reads move off `UserId` onto the active tenant — repos drop their
`GetByUserIdAsync` finders for `GetIdForCurrentTenantAsync` querying the base `CurrentTenant` root, and
services/validators inject `ITenantContext` in place of `IUserModule`/`ICurrentUser`; (3) the `Seed`
catalog gains a founding-Owner `Memberships` set and a `TenantType` per tenant.

**Correctness verified — the dropped ownership checks are safe.** `ArtistService`/`VenueService.Update`
deleted their `if (entity.UserId != currentUser.GetId()) throw ForbiddenException` guard, now relying on
`repository.GetByIdAsync(id)`. That base finder (`Repository<T>.GetByIdAsync` → bare `context.Set<T>()`)
is automatically tenant-filtered because `ArtistDbContext`/`VenueDbContext.ApplyTenantFilters` register a
**global query filter** via `modelBuilder.ApplySingleOwner<T>(this)` (`TenantFilters.cs` →
`HasQueryFilter(IsHost || e.TenantId == current)`). So a cross-tenant id returns `null` → `NotFoundException`;
no IDOR. The `OwnsOpportunity*` rewrites (`repository.GetTenantIdByIdAsync(id) == tenant`, fail-closed on no
active tenant) and `ApplicationValidator` (`opportunity.TenantId != tenantContext.TenantId`) are sound and
have matching unit-test updates.

**Seeding clean.** `SeedState.Memberships` is founding-Owner-only (cross-area note resolved above);
`TenantFactory.Create` now stamps `TenantType` (Venue/Artist by `ManagerKind`) into the raised
`TenantCreatedDomainEvent`. Both cross-area notes for this stage are confirmed resolved.

**Module boundaries.** Cross-module reads go through the documented facades — `IArtistModule.GetIdForCurrentTenantAsync`,
`IVenueModule.GetVenueIdForCurrentTenantAsync` (Api/Infra → Contracts), allowed. The Api→Api `[HasPermission]`
coupling is the systemic **MB1**; one leftover instance is **MB2** below.

- [x] **MB2 — LOW — module boundaries** — `api/Concertable.B2B/Modules/Venue/Concertable.B2B.Venue.Api/Concertable.B2B.Venue.Api.csproj:19` and `api/Concertable.B2B/Modules/Venue/Concertable.B2B.Venue.Api/Controllers/VenueController.cs:5`
  **[FALSE POSITIVE — not actioned. `VenueController.Approve` uses `[Admin]` (`Concertable.B2B.User.Api.Authorization`),
  so the `User.Api` reference + `using` are live, not dead. Removing them breaks the build. See Resolution.]**
  `Venue.Api` **added** the `Tenant.Api` `ProjectReference` but **kept** the old
  `..\..\User\Concertable.B2B.User.Api\Concertable.B2B.User.Api.csproj` reference (and the
  `using Concertable.B2B.User.Api.Authorization;` in `VenueController`). With every `[VenueManager]`
  now `[HasPermission]`, that reference is **dead** — a grep of the whole `Venue.Api` project finds no
  remaining use of `User.Api`/`User.Contracts`/`VenueManager` beyond those two lines. The sibling modules
  did this right: `Artist.Api` and `Concert.Api` each **removed** their `User.Api` ref when migrating off
  `[ArtistManager]`/`[VenueManager]`. So this is a leftover that uniquely keeps `Venue.Api` coupled to
  `User.Api` (Api→Api, the exact coupling MB1 is about) for no reason. Drop the `ProjectReference` and the
  unused `using`. (Compiler tolerates both — it won't flag an unused `ProjectReference` — so CI won't catch it.)

- [x] **CV6 — LOW — C# conventions** — `api/Concertable.B2B/Modules/Artist/Concertable.B2B.Artist.Infrastructure/Repositories/ArtistRepository.cs:13,18` and `api/Concertable.B2B/Modules/Venue/Concertable.B2B.Venue.Infrastructure/Repositories/VenueRepository.cs:13,18`
  The new `GetIdForCurrentTenantAsync`/`GetDetailsForCurrentTenantAsync` call `CurrentTenant.AsNoTracking()`
  with no `base.` qualifier. `CODE_CONVENTIONS.md` ("Base-class members — call through `base.`") uses
  **this exact member** as its worked example: `base.CurrentTenant.Where(...)` is CORRECT, bare
  `CurrentTenant.Where(...)` is WRONG, because `CurrentTenant` is defined on `TenantScopedRepository`, not
  on these repos. These are the codebase's only `CurrentTenant` consumers (no contradicting precedent), so
  the new lines should read `base.CurrentTenant`. Minor — same low tier as CV4/CV5.

## Cross-service migrations & infra (Customer, Search, Messaging, Auth) — reviewed 2026-06-21

The cross-service blast radius of the user-model work. It is almost entirely a **migration re-scaffold**:
24 of the 25 files are `InitialCreate` renames `20260612…` → `20260620…` across Auth (Auth + Duende
contexts), all six Customer modules, Search, and Messaging (Inbox + Outbox). The `.cs` migration bodies
are byte-identical (`R100`, move-only — skipped); the `.Designer.cs` snapshots are 99% identical, the
**only** changed line in each being the `[Migration("…")]` timestamp attribute. No model drift, no
additive migration — exactly the `./initial-migrations.ps1` "nuke and re-scaffold every module's
`InitialCreate`" convention (`api/CLAUDE.md` → Migrations). These services' models were untouched by the
feature; they were re-stamped only because the re-scaffold runs solution-wide in one pass. Consistent
with the B2B/User/Tenant re-scaffolds already accepted in stages 1–4. Not a finding.

The single real content change is **`api/Concertable.Auth/Config.cs`** and it is correct:

- **B2B api resource `UserClaims` `{ "role", "owner" }` → `{ "role", "email" }`.** Drops the `owner`
  claim B2B no longer mints (its payout proxy passes the active tenant id to Payment over gRPC) and adds
  `email` so B2B can attribute created Venue/Artist profiles to the operator (the Phase-5 email-claim fix,
  commits `71abf85c`/`4fb5f605`). Matches `api/CLAUDE.md` ("B2B no longer mints `owner`… passing the
  active tenant id explicitly") word-for-word.
- **Customer api resource `UserClaims = { "role", "owner" }` unchanged** — Customer still mints `owner`
  (the buyer's own id) via the scope it already requests, confirming the Customer-stage cross-area note.
- **`payment:write` stays a service-only `ApiScope`** granted only to `ServiceClient` (ClientCredentials);
  no interactive (web/mobile) client lists it, so no user token can carry the money-surface gate — the
  invariant the `Config.cs` comment calls out, still holds.

Isolation, correctness, seeding, and module boundaries are all clean for this area. **No issues found in
this area.**

## Tests (Tenant tests + B2B integration fixtures) — reviewed 2026-06-21

The test half of the user-model work: new Tenant unit tests (`PermissionCatalog`, `TenantMembershipEntity`,
expanded `TenantContext` + `TenantEntity`), new Tenant integration tests (`ActiveTenantResolution`,
`StripeAccountProxy`, `TenantProvisioning`) driven through a new `TenantApiFixture`, and two fixture additions
(`MockPayoutAccountClient`, the `EmailHeader` on `ApiFixture.CreateClient`). All five lenses are otherwise
clean; two LOW items below.

**Correctness — sound, and the tests assert the right invariants.** Spot-checked the higher-risk ones:
- `StripeAccountProxyTests` proves the proxy keys Payment on the **active tenant, not the user**
  (`LastOwnerId == TenantOf(manager.Id)` *and* `!= manager.Id`), and that the permission gate holds across roles
  (Owner/Finance → 200, Manager → 403) using a header-named tenant — the exact Phase-5 invariant.
- `ActiveTenantResolutionTests` covers sole-membership default, header switch, multi-membership-no-header
  fail-closed, and unowned-tenant fail-closed against the real ASP.NET pipeline.
- `TenantProvisioningTests` drives the production trigger (`TenantProvisioningHandler` reacting to
  `CredentialRegisteredEvent`) directly rather than inserting rows — the sanctioned integration-test path — and
  asserts persona-from-client-id, founding-Owner, and idempotency over the seeded operator.
- `TenantContextTests` correctly verifies the header fast-path skips `GetMembershipsAsync` (`Times.Never`) and that
  a malformed header degrades to the sole-membership default.

**Fixture lifetime is safe.** `TenantApiFixture.OnReset(scope)` caches `TenantDbContext` from the **long-lived**
`scope` field `ApiFixture.ResetAsync` reassigns per test (disposed only on the next reset/dispose), so the
`Tenants`/`Memberships` read-backs and `AddMembershipAsync` stay valid for the whole test — no
`ObjectDisposedException`. Read-backs are `AsNoTracking`, so they see HTTP-committed rows despite the reused
context. **Isolation/boundaries/seeding clean:** test refs to `Concertable.Payment.Client` (adapter client lib) and
`Concertable.Auth.Contracts` (adapter contracts) are allowed crossings; the new `Tenant.Infrastructure` ProjectReference
is a test→own-module-infra ref; `AddMembershipAsync`'s direct insert is documented per-test arrangement of a state the
seed graph never holds (not an `IDevSeeder`/`ITestSeeder`), which the integration-test exception permits.

- [x] **BUG1 — LOW (latent) — correctness / test isolation** — `api/Concertable.B2B/Tests/Concertable.B2B.IntegrationTests.Fixtures/Mocks/MockPayoutAccountClient.cs` and `ApiFixture.cs:116,129`
  `MockPayoutAccountClient` holds per-test mutable state (`LastOwnerId`) but, unlike every sibling mock that does
  the same (`MockStripeApiClient`, `MockNotificationClient`, `MockManagerPaymentClient` all `: IResettable` and
  passed to `AddResettables(...)`), it neither implements `IResettable` nor is registered for reset. So `LastOwnerId`
  **leaks across tests** — `ResetAsync`'s `foreach (resettable) resettable.Reset()` never touches it. Harmless today
  because every assertion on `LastOwnerId` (`AccountStatus…`, `FinanceRole…`) is preceded by a request in the same
  test that sets it fresh; but it's a latent flake the moment a test asserts the owner id without a preceding call,
  and it breaks the fixture's own reset convention. Make it `IResettable` (`LastOwnerId = null`) and add it to the
  `AddResettables(...)` list.

- [x] **CV7 — LOW (borderline) — C# conventions** — `api/Concertable.B2B/Modules/Tenant/Tests/Concertable.B2B.Tenant.IntegrationTests/TenantProvisioningTests.cs:67-69`
  The idempotency note is a 3-line stacked `//` comment; `CODE_CONVENTIONS.md` ("Comments — short, and multi-line
  uses `/* */`") says a genuinely multi-line comment uses one `/* */` block, never stacked `//`. Same minor
  deviation as CV3/CV5; the content earns its place (explains why a clean re-run is itself the dedup assertion) —
  only the spelling. Dismiss freely.

## Frontend, docs, plans, config — reviewed 2026-06-21

The final stage: the SPA's tenant-header plumbing, the B2B payout base-URL swap, the e2e flaky-stack guard,
and a large, careful documentation/plan refactor. **Docs and plans are exemplary** and verified clean — not
findings, recorded so the next reader doesn't re-check them:

- **Reference hygiene is complete.** The prior tenant-scoping branch deleted `ORGANIZATION_REFACTOR_PLAN.md`,
  `TENANCY_DESIGN.md`, and `TENANT_SCOPING_PLAN.md`; this branch scrubs **every** dangling link to them across
  `LAUNCH_PLAN.md`, `MARKETPLACE_PLAN.md`, `MICROSERVICES_ARCHITECTURE.md`, and `MICROSERVICE_STEPS_CONT.md`,
  re-pointing each to `USER_MODEL_PLAN.md`. A live-tree grep finds **zero** broken links to the three deleted
  docs (the lone remaining mention is a prose changelog entry in `LAUNCH_PLAN.md` documenting the deletion, not
  a link).
- **`USER_MODEL_PLAN.md` is correctly retained, not deleted.** Phases 1–5 are struck ✅ in the same shipping
  commits; Phases 6–8 (invitations/member-mgmt UI, retire `role` claim, messaging inbox) remain outstanding —
  exactly the partially-done-plan rule (`plans/CLAUDE.md`). The new `plans/CLAUDE.md` (the whether-to-run-E2E
  workflow) and the `api/CLAUDE.md` / `SEEDING_CONVENTIONS.md` edits accurately describe the shipped design
  (B2B drops `owner`, fronts Payment over gRPC; only founding-Owner memberships are seeded).
- **`api/Concertable.Auth/Config.cs` and migrations** were the Cross-service stage's domain; nothing here
  contradicts it.

Correctness/boundary of the actual TS + PowerShell follows. The headline is **MB3** — a frontend module-boundary
smudge introduced by routing payouts through the shared axios client.

- [x] **MB3 — MEDIUM — module boundaries (frontend)** — `app/web/shared/src/features/tenant/` (`index.ts`, `store/useActiveTenantStore.ts`), consumed by `app/web/shared/src/lib/axios.tsx` + `paymentAxios.ts`
  The new `useActiveTenantStore` + `TENANT_HEADER` live in **universal** `app/web/shared`, whose `CLAUDE.md` is
  categorical: *"Everything here compiles into EVERY web app. Nothing app-specific goes here. Ever… Code shared
  by the two manager apps but not the customer app… belongs in `app/web/b2b/shared`, never here."* Tenancy is a
  manager-only concept (the whole `USER_MODEL_PLAN` is "B2B-internal"; customers have no tenants/memberships) —
  yet because the two shared interceptors (`axios.tsx`, `paymentAxios.ts`) now `import { TENANT_HEADER,
  useActiveTenantStore } from "@/features/tenant"`, the store **compiles into the customer bundle** (confirmed:
  `app/web/customer/src/main.tsx` imports `shared/lib/axios` + `shared/lib/paymentAxios`). It is behaviour-neutral
  today — nothing sets `activeTenantId` until the Phase-6 switcher, so customer never sends the header and the
  doc's tsc boundary gate can't catch it (it's genuinely type-safe everywhere). But it does plant a B2B concept
  in the all-sites tree, the exact "park B2B code in `app/web/shared` for now" the doc's closing paragraph warns
  against. The clean fix is the pattern the same doc prescribes — inject, don't import: give `configureApi`/
  `configurePaymentApi` an optional `getTenantId` provider that the B2B apps wire to the store (which then lives
  in `app/web/b2b/shared`), so universal shared never names "tenant". Defensible as a pragmatic tradeoff (the
  shared interceptor is the natural stamping point), so MEDIUM not HIGH — but it's a real boundary call worth
  making consciously now, before Phase 6 grows more tenant UI around it.

- [x] **BUG2 — LOW (latent) — correctness (frontend)** — `app/web/shared/src/features/tenant/store/useActiveTenantStore.ts:18-22`
  The store uses `persist(..., { name: "concertable.active-tenant" })`, so `activeTenantId` is written to
  `localStorage` and **survives logout**. localStorage is per-origin, and the four SPAs are separate origins, so
  there's no cross-app bleed — but within one manager app, if user A selects a tenant, logs out, and user B logs
  in on the same browser, the stale `activeTenantId` is replayed as `X-Tenant-Id`. The backend fail-closes on an
  unowned tenant (verified in the Tenant stage: "unowned-tenant fail-closed"), so it's a 403/no-active-tenant wall
  rather than a data leak — but a confusing one. Inert today (nothing sets the value pre-Phase-6); flagging so the
  Phase-6 switcher remembers to **clear `activeTenantId` on logout/login**. Same latent class as BUG1.

- [x] **DOC1 — LOW — docs/comment accuracy** — `e2e.ps1:103-111` and `.claude/skills/e2e-ui-regress/SKILL.md:51`
  Both say the flaky-stack guard re-runs the failures "**each** on a freshly-booted stack". The implementation
  (`e2e.ps1:111`) is a **single** `dotnet test --filter $retryFilter` invocation with all failed DisplayNames
  joined by `|` → the retried set shares **one** freshly-booted Aspire stack, not one per scenario. The mechanism
  still works (the small retried set means low load, which is what keeps the ASB emulator from dropping
  connections), so the guard is sound — but "each on a freshly-booted stack" overstates per-scenario isolation and
  could mislead a future debugger into thinking two still-interfering retries can't share a stack (they can).
  Reword to "the failures together, on a single freshly-booted stack (separate from the full run's)". Borderline;
  flagged because the codebase treats comment/doc honesty as a first-class rule.
