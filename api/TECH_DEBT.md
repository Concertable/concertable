# Concertable — cross-cutting technical debt

Debt spanning multiple services or living in shared code (`Shared/`, `Concertable.Messaging`, host `Program.cs` files). Service-specific debt belongs in that service's own `TECH_DEBT.md`. When an item is fixed, update both this file and [`ARCHITECTURE.md`](./ARCHITECTURE.md).

---

## MED

### `IEntity.DisplayName` is a soft standard (throwing default member), not a hard `static abstract`

`Shared/Concertable.Kernel/IEntity.cs` carries `DisplayName` as a `static virtual` **default interface
member** whose default *throws* `NotSupportedException`, so entities that self-name via `OrNotFound()` must
override it; an un-overridden entity fails at runtime rather than the compiler forcing a name. The intended
design was `static abstract` (compiler-enforced, every entity named), but that is a binary-breaking change
that cannot land: the core libs (`DataAccess.Infrastructure`, `Messaging.Domain`) source-reference Kernel
so integration tests load the new Kernel, while service entities compile against the Kernel *package* — a
required static-abstract member's implementation mapping is fixed at compile time against the old interface,
so package-compiled entities throw `TypeLoadException` against the new Kernel (two red CI runs confirmed).
The default member is the additive workaround.

**Resolves when:** the core libs stop source-referencing Kernel (or the repo builds shared source lockstep
so entities compile against the same Kernel the tests load), at which point `DisplayName` can become
`static abstract` and the throwing default is deleted.

### Kernel `ClaimsPrincipal.GetId()` fails open with `string.Empty`

`Shared/Concertable.Kernel/Identity/ClaimsPrincipalExtensions.cs` returns `user?.FindFirst("sub")?.Value ?? string.Empty` — a principal with no `sub` claim becomes an empty-string user id instead of a failure. Its sibling `CurrentUserExtensions.GetId(ICurrentUser)` gets this right (throws `UnauthorizedAccessException`). The only consumer, `NotificationHub`, assigns the result to `string?` and null-checks it — a check that can never fire because the method never returns null, so an unauthenticated principal sails through as `""`.

**Resolves when:** the extension fails closed (returns `string?` with no empty-string coercion, or throws like its `ICurrentUser` sibling), and `NotificationHub`'s guard actually rejects principals without a `sub` claim.

---

### Required config bound with `?? ""` — services boot misconfigured and fail later

Eight hosts coalesce required auth/bus settings to empty string at bind time: `Auth:Authority` / `ServiceAuth:*` `ClientId`+`ClientSecret` in `Concertable.Auth/Program.cs`, `Concertable.B2B.Web/Program.cs`, `Concertable.B2B.Workers/ServiceCollectionExtensions.cs`, `Concertable.Customer.Web/Program.cs`; the ASB `ConnectionString` additionally in `Concertable.Payment.Web`, `Concertable.Payment.Workers`, `Concertable.Search.Workers`, and `Concertable.B2B.Seed.Simulator` `Program.cs`. A missing setting silently becomes `""`, the host starts cleanly, and the failure surfaces later as a confusing auth/bus error instead of at startup. `Concertable.Messaging.AzureServiceBus/Options/AzureServiceBusOptions.cs` compounds it with `= ""` property defaults where the convention (`docs/CODE_CONVENTIONS.md`) requires `null!` for binder-populated values. All of these also use the banned `""` literal.

**Resolves when:** required settings fail fast at startup — options validation with `ValidateOnStart` (or an explicit throw on missing key) replaces every `?? ""`, and `AzureServiceBusOptions` defaults become `null!`. Genuine optional-with-empty-default settings, if any, keep an explicit `string.Empty`.

---

### Auth builds against a pinned shared-platform package while the rest of the solution builds from source

`api/Concertable.Auth/Directory.Packages.props` pins the shared platform to `ConcertablePlatformVersion` (currently `0.1.0-alpha.0.526`), so in the full `Concertable.slnx` build Auth compiles against that *published* package while B2B/Customer/Search build the same shared projects from live source. Edit shared source without re-publishing + bumping the pin and Auth silently compiles against stale code; a breaking shared-API change turns only the Auth build red with a confusing "works in source, fails as package" error. Accepted build-separation tradeoff for now (Auth.Contracts has ~0 churn and the shared platform changes infrequently), but the divergence is real the moment shared code moves without a publish.

**Resolves when:** the SERVICE_BUILD_SEPARATION hybrid inner-loop toggle lands (`ProjectReference` for local multi-service dev, `PackageReference` in CI/standalone), or the platform-version pin is automated so it can't lag a shared-source change.

---

### Shared test libraries are ProjectReferenced across the service-folder boundary (carve leak)

`Concertable.Testing`, `Concertable.Testing.Integration`, and the shared `Concertable.E2ETests` harness
live under `Concertable.Shared/tests/` — i.e. in the Shared "repo" — yet every consuming test project
reaches them by a `ProjectReference` that **escapes its own service folder**
(`api/Concertable.B2B/src/Modules/.../Tests/*.csproj → ..\..\..\..\..\..\Concertable.Shared\tests\Concertable.Testing\...`).
That is exactly the cross-folder escape the runtime carve forbids for service projects (the
`PackageReference, never a ProjectReference` guard in the service `.csproj`s). Runtime deps that live in
the Shared tree (Kernel, Messaging) publish + are pinned; the shared **test** libs alone leak straight
into every service's test projects. On a real repo split those references break. `Concertable.Testing`
even carries `IsPackable=true` with **zero** package consumers — a half-committed intent. First flagged
adding a shared `Money` test helper for the door-revenue UI E2E: it compiled same-PR *because* of this
leak, where a Kernel helper needs a publish-first PR.

**Resolves when:** the shared test libs are published as test-support packages consumed by pinned
`PackageReference` like the runtime shared libs (carrying the same publish-first + pin-bump boundary) —
OR test infra is explicitly documented as carve-exempt (dev-only, never shipped in a service runtime)
and the misleading `IsPackable=true` is dropped. Decision + execution steps:
[`plans/SHARED_TEST_LIBS_PACKAGING.md`](../plans/SHARED_TEST_LIBS_PACKAGING.md). Lean: publish, for
consistency with the Shared-repo model — the cost is that every shared-test-helper edit then takes the
publish-first cycle.

---

## LOW

### `initial-migrations.ps1` re-stamps every module, desyncing packaged libs from their published packages

`api/initial-migrations.ps1` nukes and re-scaffolds **every** module's `InitialCreate` with a fresh
timestamp — including libs consumed as *published packages* (`Messaging`, `Payment`, `Auth`) whose
model didn't change. The regenerated source then carries a newer migration id than the published
package the standalone/E2E stack actually loads, and `DevDbInitializer` blows up applying a migration
whose table already exists (first seen while re-scaffolding on a migration-touching branch: "There is already an object named
'Outbox'", every UI E2E scenario dead at fixture init). Workaround each time: after running the
script, `git checkout origin/master -- <migration dirs>` for every module whose migration content is
byte-identical to master (only the genuinely-changed module keeps its new migration). Bites every
migration-touching branch.

**Resolves when:** the script only re-scaffolds modules whose model actually changed (diff the
generated migration content, skip re-stamp if identical), or packaged-lib migrations are excluded
from the blanket nuke.

### Orphaned FlatFee accept-checkout holds release only by ~7-day Stripe expiry

When a venue runs FlatFee accept-checkout (a manual-capture PI ring-fencing the venue's own funds) and the application is then withdrawn/rejected/cancelled instead of accepted, nothing cancels the hold: Payment exposes no cancel RPC (`ManagerPayment` has `FindHeldIntent` but no cancel; `IStripeHoldClient.CancelAsync` is Payment-internal), so the funds stay ring-fenced until Stripe auto-expires the intent (~7 days). Money-safe, just slow to release. This was the deliberately-skipped optional Phase 5 of the delivered application-cancel plan — it needs a Payment-first two-PR cycle across the package boundary.

**Resolves when:** `ManagerPayment` gains a `CancelHeldIntent(payer_id, application_id)` RPC (+ `IManagerPaymentClient.CancelHeldIntentAsync` and fake/mock impls, published as `Payment.Client`), and B2B best-effort releases the hold on FlatFee withdraw/reject/cancel.

---

### No local-source swap for cross-service adapter packages during a breaking migration

`Directory.Build.targets`' `UseLocalCore` swaps only the churny *core* (`Kernel`, `Messaging.*`) from package to source; cross-**service** adapter packages (`Payment.Client`/`Contracts`, `*.Tenant.Contracts`, etc.) have no equivalent swap. So mid-way through a *breaking* cross-service contract change, the full `Concertable.slnx` won't build green locally — production consumers bind the old package while the integration-test fixtures `ProjectReference` the new source. You can still build/test per-service (`Payment.slnx` green; red confined to the 4 consumer fixtures + `TicketApiTests`), so it's a comfort gap, not a blocker. Deliberately deferred (was Phase 2 of the now-deleted `plans/PLATFORM_PACKAGE_SYNC.md`): the core friction — hands-off, green pin propagation — is already solved by the `platform-sync` workflow; this only removes local red while iterating, and adds a local-vs-CI divergence (the reason the swap is inner-loop-only, never committed/CI).

**Resolves when:** a real breaking migration makes the local red painful enough to justify extending the `UseLocalCore` swap to cross-service adapter packages (local/inner-loop only — CI + the carve gates always build against packages).

### CI feed restore assumes a same-repo `GITHUB_TOKEN` — fork / Dependabot PRs can't read the org feed

`.github/workflows/test.yml` authenticates the GitHub Packages feed with `secrets.GITHUB_TOKEN` in the `build`, `carve-auth`, and merge-queue E2E jobs. A PR opened from a **fork** (or a Dependabot PR) runs with a read-only token scoped to the fork, which cannot read the `Concertable` org's private packages, so those PRs would 401 at restore regardless of the change. Not a problem for the current same-repo branch + merge-queue workflow (no fork PRs), logged in case the repo is ever opened to external contributors.

**Resolves when:** the org packages are made internal-visible to the org's repos, or fork PRs are given a `read:packages` PAT (or simply aren't accepted).
