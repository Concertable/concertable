# Code review — Chore/TechDebt-adminrepo

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `119714ba74586f7542291f0d4b2fb824afb0a351`  _(2026-08-21)_

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
