# Concertable — root technical debt

Debt that is genuinely repo-wide: `.github/workflows/**` gating logic, root-level docs/config, or
anything spanning both `api/` and `app/`. Backend-only cross-cutting debt (multiple services, host
`Program.cs` files) belongs in [`api/TECH_DEBT.md`](./api/TECH_DEBT.md); frontend-only cross-cutting
debt in [`app/web/TECH_DEBT.md`](./app/web/TECH_DEBT.md) / [`app/shared/TECH_DEBT.md`](./app/shared/TECH_DEBT.md).
Service- or tier-specific debt belongs in that area's own `TECH_DEBT.md`.

---

## HIGH

### A shared contract change scopes CI to its owning service, so no consumer's suites run

`test.yml`'s service scope narrows a PR's test matrix to the services its `api/` paths touch, and fails
safe to `ALL` when any path sits outside a single service folder. The regex deciding that is
`SERVICE_DIRS='^api/Concertable\.(Auth|B2B|Customer|Payment|Search)([./]|$)'`, and its character class
admits `.`, so `api/Concertable.Auth.Contracts/` matches as the `Auth` service folder and the per-file
`sed` reduces it to `Auth`.

`Concertable.Auth.Contracts` is not Auth's private tree. It is a published contract package that
`Concertable.B2B.Web`, `Concertable.Customer.Web`, `Concertable.Search.Web` and `Concertable.Payment.Web`
all consume, and it carries the scope and audience registry every resource server validates tokens
against. A PR changing only that directory therefore runs Auth's suites and none of its four consumers' —
the opposite of the fail-safe-to-`ALL` intent stated in the scoping block's own comment. The same holds
for any `api/Concertable.<Service>.Contracts/` directory.

Verified rather than reasoned: piping `api/Concertable.Auth.Contracts/AuthScopes.cs` through the shipped
regex matches, and through the shipped `sed` yields `Auth`.

The concrete loss today is that `PaymentScopeParityTests`, which holds Payment's `payment:write` literal
equal to `AuthScope.PaymentWrite.Id()`, cannot fire on an Auth-side rename — it is excluded from that PR's
matrix. `push` and `merge_group` use the same scoping code, so the merge queue does not recover it.

**Resolves when:** `SERVICE_DIRS` matches only a service's own directory (`(/|$)` rather than `([./]|$)`),
so every `api/Concertable.*.Contracts/` path falls through to `ALL` and a shared contract change runs every
consumer's suites. Worth confirming at the same time that no other sibling directory is being swallowed by
the same character class.

---

## MED

### Plan-managed worktree close rejects the prescribed repository-relative ledger path

`plans/AGENTS.md` prescribes repository-relative worktree paths in progress ledgers and
`scripts/worktrees.ps1 close -PlanManaged`, but the close command compares that ledger value with the resolved
absolute registered worktree path. A correctly recorded producer worktree was therefore rejected until the same
safe close ran without `-PlanManaged`.

**Resolves when:** `AssertLedger` resolves repository-relative ledger worktree values against the repository
root before comparing them with the registered path, with a regression test covering the documented ledger form.

### One style rule the standard requires enforced is missing from `.editorconfig`

`STYLE.md` opens by stating that style rules an analyzer can express belong in `.editorconfig` at
`severity = error`, and lists five. The single root `.editorconfig` carries four — the private-field
camelCase naming rule, `csharp_style_namespace_declarations`, `MA0053` and `CA1848` — and omits
`csharp_prefer_braces = when_multiline:error`, so brace style on single-statement bodies is carried by
reviewers noticing.

The `this.` half of this entry is **not** closed yet. The intended convention is that `this.` exists
only to disambiguate a member a parameter or local shadows, and the codebase sweep has followed it —
but every published `dotnet-standards` copy still carries
`dotnet_style_qualification_for_field = true:error` in `STYLE.md`'s table plus the "every constructor
assignment is `this.`-qualified" prose, so the standard and the codebase disagree and every review of
this repo re-flags the sweep as a violation. The remaining sweep itself is tracked in
[`api/TECH_DEBT.md`](./api/TECH_DEBT.md).

**Also resolves when:** `STYLE.md` in `dotagents` states the disambiguation-only rule and that version
is published, so a reviewer reading the standard reaches the same conclusion as the codebase.

**Resolves when:** `csharp_prefer_braces = when_multiline:error` is enforced with the codebase brought
to it, and `STYLE.md`'s table matches the `.editorconfig`.
