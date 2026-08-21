# Code review — Chore/TechDebt-adminrepo

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `980c4c97a1415579b26a288242f1e09c13197011`  _(2026-08-21)_
**Security-reviewed up to commit:** `980c4c97a1415579b26a288242f1e09c13197011`  _(2026-08-21)_

> Range reviewed: `c4c83ee1..119714ba` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No issues found. Checked correctness, microservice isolation, module boundaries, seeding, C#
conventions, and test coverage of changed paths — plus the routed skills (`dotnet-standards:persistence`,
`dotnet:persistence`, `dotnet-standards:multitenancy`, `dotnet:multitenancy`,
`dotnet-standards:dependency-injection`, `dotnet-standards:module-structure`, `dotnet:module-structure`,
`dotnet-standards:result-carriers`, `dotnet-standards:unit-testing`, `dotnet:unit-testing`,
`dotnet-standards:csharp-style`, `dotnet-standards:csharp-naming`, `csharp-style`, `csharp-naming`,
`docs-and-debt`).

Layer 1 (native review, `code-reviewer` subagent, medium effort): no findings. Confirmed the split is
mechanical and behaviour-preserving — entity mappings moved 1:1 to the matching interface/repository, DI
registers both new services in the owning composition root, no stray references to the deleted
`IAdminRepository` remain, and `AdminServiceTests` preserves every original assertion with the correct
mock reassigned per call site.

Layer 2 (architecture lenses):
- **Persistence** (`PERSISTENCE.md` "one repository per entity") — this diff is the textbook fix for
  that rule: `IAdminInvitationRepository` binds `Repository<AdminInvitationEntity>` via the module's
  `Repository<TEntity>` alias exactly like `IOrderRepository`'s example; `IAdminProfileRepository` is a
  narrow hand-rolled interface (no generic base) because `AdminProfileEntity` has no `Id`/`IEntity<TKey>`
  shape — it's keyed by `Sub` alone — so it can't sit on the generic capability hierarchy, correctly
  matching how the standard frames a repository's binding as one-per-entity rather than one-per-base.
  Both repositories inject `AdminDbContext` into a field named `context`, never `dbContext`.
- **Module structure** — both interfaces stay `internal` in `*.Application/Interfaces`, both
  implementations stay `internal sealed` in `*.Infrastructure/Repositories`; no visibility widened.
- **Dependency injection** — both registered as `AddScoped<TInterface, TImplementation>` in
  `AddAdminModule` (the module's own composition root), constructor-injected into `AdminService`,
  no `IServiceProvider`/factory-lambda service location introduced.
- **C# style** — no primary constructors; `AdminInvitationRepository`/`AdminProfileRepository` use
  explicit constructors with `this.context = context;`; no `_`-prefixed fields.
- **Unit testing** (`UNIT.md`) — `AdminServiceTests` now builds `service` (the SUT) and every mock in the
  constructor as `this.`-qualified `private readonly` fields, matching the sibling
  `TenantServiceTests.cs` exactly, replacing the prior per-test `CreateService()` factory. Tests grouped
  into `#region`s per method under test (`RevokeAdminAsync`, `InviteAsync`, `RevokeInvitationAsync`,
  `IsCurrentUserAdminAsync`, `GetOverviewAsync`). Arrange/Act/Assert stays blank-line-separated, no
  `// Arrange` comments, `Method_Scenario_ExpectedBehaviour` naming preserved throughout.
- **Test coverage** — behaviour-preserving refactor; the 25 existing unit tests and 7 existing
  integration tests already exercise every path through the real DI-wired repositories (confirmed green
  in both worktree runs, pre- and post-rebase onto current `main`). No new behaviour, no coverage gap.
- **docs-and-debt** — the resolved `TECH_DEBT.md` entry is deleted outright (not archived), per
  "Once the debt is addressed, delete the entire entry."

## Incremental review — 2026-08-21

> Range reviewed: `119714ba..a5e7bf4c` (9 commits touching the Admin module; 188 commits total, the rest
> unrelated `origin/main` history pulled in by two base-currency merges — scoped with
> `-- api/Concertable.B2B/src/Modules/Admin`).

`origin/main` independently landed an admin-grant race-safety fix (`ff8f25243`) while this branch was
open: `AdminService.GrantIfEligibleAsync` (registration-time, `Task`) became
`EnsureCurrentUserAdminGrantedIfEligibleAsync` (post-login `/api/auth/me`, `Task<bool>`, with a
`TrySaveGrantAsync` helper catching a concurrent-grant duplicate-key race). Merging `origin/main` produced
real conflicts in `AdminService.cs` and `AdminServiceTests.cs` — both branches touched the same methods —
hand-resolved by reapplying the repository split onto main's new shape: every `repository.X` call became
`invitationRepository.X` or `profileRepository.X` by domain ownership, and `SaveChangesAsync` (including
inside `TrySaveGrantAsync`) keeps routing through `invitationRepository`, per the convention this file
already covers above. The rest of the diff (`IAdminService.cs`, `IAdminModule.cs`, `AdminModule.cs`,
`AdminInvitationEntity.cs`, `AdminProvisioningTests.cs`, the `.csproj`) merged clean from `origin/main`
with zero conflict — already-reviewed upstream content, sanity-checked here rather than re-reviewed.

Layer 1 (native review, `code-reviewer` subagent, medium effort): no findings. Verified line-by-line that
every `repository.X` call in main's pre-merge `AdminService.cs` maps 1:1 onto the correct split repository
(`ListAdminSubsAsync`/`GrantAdmin`/`RemoveAdmin`/`IsAdminAsync`/`CountAdminsAsync` → profile;
`GetPendingInvitationByEmailAsync`/`ListPendingInvitationsAsync`/`InsertAsync`/`GetByIdAsync` →
invitation), confirmed both repositories share one scoped `AdminDbContext` per request (so
"save via one designated sibling repo" is structurally correct, not just asserted), confirmed no stray
reference to the deleted `IAdminRepository` or the renamed `GrantIfEligibleAsync` remains anywhere under
the module, and confirmed the new `EnsureCurrentUserAdminGrantedIfEligibleAsync` unit tests mock and
verify against the correct split repository throughout.

Layer 2 (architecture lenses): Lens A (correctness/atomicity/races) — the duplicate-key race handling is
unchanged in effect by the split, since `SaveChangesAsync` still saves the one shared context regardless
of which repository interface issues the call. Lens B (service isolation) — not applicable, no
cross-service call added. Lens C (module boundaries) — not applicable, no new cross-module reach. Lens D
(seeding) — not applicable, no seeder touched. Lens E (conventions) — same routed skills as the prior
review, all still satisfied post-merge. Lens F (test coverage) — the 7 new
`EnsureCurrentUserAdminGrantedIfEligibleAsync` unit tests and the rewritten `AdminProvisioningTests`
(registration vs. login-time grant, split into `RegisterAsync`/`LogInAsync`) already ship with this diff
from upstream; no gap introduced by the merge resolution itself.

Security layer (`.Contracts` path touched — `IAdminModule.cs` — so this range needed a current
`Security-reviewed up to commit:` marker): no findings. `EnsureCurrentUserAdminGrantedIfEligibleAsync`
grants off `ICurrentUser.Id`/`.Email` (server-derived from the authenticated token, not attacker-supplied
input) and a server-configured bootstrap email — the same trust boundary as the pre-existing
`GrantIfEligibleAsync`, unchanged by the repository split. No raw SQL, string-built queries, secrets,
crypto or deserialization anywhere in the diff. The `IAdminModule.cs`/`IAdminService.cs` changes are a
method rename plus XML-doc updates carrying no new behaviour of their own — the actual security posture
change (moving the grant check from pre-verification registration time to post-login, closing an
unverified-email gap) is `ff8f25243`'s, already merged and live on `main` independently of this PR.

## Incremental review — 2026-08-21 (base sync, re-stamp only)

> Range reviewed: `a5e7bf4c..980c4c97` (base-currency merge picking up platform-sync PR #716 and the N4
> architecture-doc re-homing). Confirmed `git diff a5e7bf4c..HEAD --stat -- api/Concertable.B2B/src/Modules/Admin`
> is empty — nothing in this PR's own module changed. Re-stamped without a fresh review pass.
