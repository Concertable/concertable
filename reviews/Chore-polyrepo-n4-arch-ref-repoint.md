# Code review — Chore/polyrepo-n4-arch-ref-repoint

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `fe38af163d0bc6791c35a727e5b0f8b54b831167`  _(2026-08-21)_

**Security-reviewed up to commit:** `fe38af163d0bc6791c35a727e5b0f8b54b831167`  _(2026-08-21)_

> Range reviewed: `a364bebb..fe38af16` (2 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

**None.** All three layers are clear.

The entire range is a documentation-pointer repoint: every citation of the deleted-in-the-sibling-N4-docs-branch `api/ARCHITECTURE.md` is redirected to "the `packages` skill" inside MSBuild XML comments and service-boundary `<Error Text="...">` strings across the five service `Directory.Build.props`/`.targets` and `*.Hosting.csproj` files, plus one line in `.github/workflows/claude-review.yml` dropping `api/ARCHITECTURE.md` from the review bot's read-list. No build logic, MSBuild `Condition`, `EnforceServiceBoundary` value, workflow trigger/permission/checkout, or executable behaviour changed.

Verified during review:

- **Repoint target is accurate.** `PACKAGES.md` (the `packages` skill) carries the exact cited content — its `## Per-folder build closures — never repo-root config` section and the NuGet-`PackageReference`-vs-`ProjectReference` boundary rule, including the AppHost/`*.Hosting`/Tests/`UseLocalCore` exemptions the `.Hosting.csproj` comments reference. The new pointers land on the right owner.
- **Repoint is complete for this branch's scope.** The only other non-doc `ARCHITECTURE.md` references are `.agents/skill-routes.json:162` (repointed by the sibling `Docs/docs_polyrepo-ready-n4-architecture` branch, which also deletes the doc) and `scripts/worktrees.ps1:178` (a root-level `ARCHITECTURE.md` existence probe in a resilient `-or` marker chain — not a citation of `api/ARCHITECTURE.md`, and already satisfied by the `AGENTS.md` fallback). Nothing dangles across the coordinated pair, and the two branches share no files.

Lenses checked:

- **Layer 1 — native general review** (`NAT`): no findings — correctness, reuse, simplification, efficiency, error handling. Comment/string-only; no logic touched.
- **Security layer** (`SEC`): no findings. Security-sensitive paths (`.github/workflows/claude-review.yml`, `api/Concertable.Auth/*`, `api/Concertable.Payment/*`) are touched only by comment/string repoints — no trigger/permission/checkout/token change, no auth/authorization logic, no secret, no injection vector. Narrowing the bot's read-list does not weaken it (checkout unchanged; retained `AGENTS.md` still routes the corpus).
- **Lens A — correctness:** no logic, `Condition`, or behaviour changed.
- **Lens B — service isolation:** the boundary-enforcement mechanism (`EnforceServiceBoundary`, the escaping-reference `Error`) is untouched; only its human-readable message text changed.
- **Lens C — module boundaries:** N/A — no code.
- **Lens D — data seeding:** N/A — no code.
- **Lens E — language/framework conventions:** edits are repoints of existing comments/strings, not new commentary; `packages` skill is the correct rule owner.
- **Lens F — test coverage:** N/A — no behaviour added or altered.
