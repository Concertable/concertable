---
name: code-review
description: Full code review of a branch diff against Concertable's conventions, module-boundary rules, and microservice-isolation rules. Reviews the diff for correctness bugs plus convention/boundary/microservice anti-patterns (B2B and Customer are separate services that must only communicate via *.Contracts integration events — never each other's runtime), filters to high-confidence findings, writes them to a per-branch review markdown, and stamps the reviewed-up-to commit SHA at the top. Use when the user wants to "code-review my changes", "review this branch", "review the PR", or "do a full review". For re-reviewing only commits added since a previous review, use the `incremental-review` skill (a thin wrapper around this one). This is Concertable's own architecture-aware review and intentionally replaces the stock built-in code-review; the plain built-in `/review` (GitHub PR review) is left untouched.
---

# code-review

Full code review of the current branch's diff, judged against Concertable's actual documented rules — not generic best practice. The output is a per-branch review markdown with a `Reviewed up to commit:` SHA marker at the top, so a later `incremental-review` run knows exactly where this review stopped.

`incremental-review` is this skill with one input changed: it starts the diff at a recorded SHA instead of the branch's merge-base. Everything else — the lenses, the confidence filter, the output file, the marker — is identical. Keep them in sync: a change to the review procedure here is inherited by `incremental-review`.

## When to use

- "review this branch", "code review my changes", "review the PR", "do a full review"
- First review of a branch (no prior review markdown exists yet)

## When NOT to use

- Re-reviewing only what changed since the last review → `incremental-review` (it reads the SHA marker and scopes to `SHA..HEAD`).
- A massive branch (100s/1000s of files) → `big-review` (stages this skill by area).
- An exhaustive multi-agent pass → run a `Workflow` (ultracode).

## Step 1 — Determine the review range

- **Start** = merge-base with master: `git merge-base master HEAD` (reviews the whole branch).
- **End** = `HEAD` (`git rev-parse HEAD`).

(The `incremental-review` wrapper overrides **Start** with the SHA from the review markdown's marker; do not change anything else.)

Show the range to the user:

```powershell
git rev-parse HEAD
git merge-base master HEAD
git log --oneline "<start>..HEAD"
git diff "<start>..HEAD" --stat
```

If the range is empty, say so and stop.

## Step 2 — Load the rules (read before flagging anything)

These docs are the source of truth. Read the ones relevant to the diff — do not rely on memory, and only flag a convention issue a doc actually states:

- Root `CLAUDE.md` and `api/CLAUDE.md` — top-of-context rules + pointers.
- `api/ARCHITECTURE.md` and root `ARCHITECTURE.md` — **microservice premise** (the boundary rules below).
- `api/docs/CODE_CONVENTIONS.md` — C# conventions (source-generated logging, field naming, ctors, etc.).
- `api/docs/MODULAR_MONOLITH_RULES.md` — module boundaries within a service.
- `api/docs/SEEDING_CONVENTIONS.md` — what may and may not be seeded directly.
- Any `CLAUDE.md` in directories the diff touches (each service / module may add local rules).

## Step 3 — Review the diff through these lenses

Review **only** the changes in `<start>..HEAD`. Read beyond them only to confirm a finding.

### Lens A — Correctness bugs

Logic errors, broken control flow, missing `await`, race conditions, atomicity/transaction gaps (e.g. a cross-context write that isn't in one transaction), null/boundary mistakes, wrong EF queries, swallowed exceptions. Real bugs hit in practice — not theoretical.

### Lens B — Microservice isolation (the high-value lens — `api/ARCHITECTURE.md`)

Concertable is a multi-service system; **B2B, Customer, and Search are data services that must NEVER depend on each other's runtime.** Flag, citing `api/ARCHITECTURE.md`:

- A **data service referencing another data service's non-Contracts project** — Customer (or its modules/tests) referencing B2B's `.Domain` / `.Application` / `.Infrastructure` / `.Seed` (anything beyond `*.Contracts`). Only `*.Contracts` (integration-event records + DTOs) may cross a service boundary.
- A data service **`WaitFor`-ing another data service** in any AppHost (the bug to never introduce). `WaitFor` is for **adapter** services only (`Auth`, `Payment`, `Notification`). `WithReference` is fine.
- "Fixing" a broken standalone host by **adding another data service to its AppHost** instead of using a `*.Seed.Simulator`.
- Cross-service communication done by **synchronous call between two data services** instead of a `*.Contracts` integration event. (Sync gRPC to an *adapter* service is allowed.)
- A producer's `*.Seed.Contracts` **referencing a consumer's** (dependency must point downward only: consumer → producer).
- Customer entities reaching back into B2B via nav chains instead of holding **purchase-time snapshots** of B2B fields.

### Lens C — Module boundaries (`api/docs/MODULAR_MONOLITH_RULES.md`)

- Cross-module calls not going through `Contracts` / the module facade (`IXModule`).
- EF queries inlined in a module facade (facades delegate to Application abstractions).
- A module writing through `IUnitOfWork` (tied to `ApplicationDbContext`, silently no-ops) instead of `xRepository.SaveChangesAsync()`.
- Impl types left `public` when an interface was extracted to `internal`.

### Lens D — Seeding (`api/docs/SEEDING_CONVENTIONS.md`)

- A seeder directly writing data whose only production write path is a reaction (read-model projections, `UserEntity`, manager profiles, Stripe `PayoutAccount`, inbox/outbox rows). The fix is to drive the event, never `context.X.AddRange(...)`.
- `IDevSeeder` vs `ITestSeeder` misuse (`ITestSeeder` never runs in dev/E2E).
- Integration events published from a service layer instead of raised from a domain event.

### Lens E — C# conventions (`api/docs/CODE_CONVENTIONS.md`)

- Inline logging templates (`logger.LogInformation("...")`) instead of a source-generated `[LoggerMessage]` in the project's `Log.cs`.
- Primary constructors on services/repos/handlers/validators (use explicit ctor + `private readonly` fields, no `_` prefix).
- `is { }` capture instead of `is not null`; unnecessary braces on single-statement `if`/`else`.
- Additive EF migrations (model changes re-scaffold via `./initial-migrations.ps1`).

## Step 4 — Confidence filter

For each candidate finding, judge whether it's real and will be hit in practice. **Drop anything below ~80/100 confidence.** Discard these false positives:

- Pre-existing issues on lines not changed in this range.
- Things a compiler / linter / CI catches (type errors, imports, formatting).
- Pedantic nitpicks a senior engineer wouldn't raise.
- Intentional changes that are part of the broader refactor.
- Issues deliberately silenced in code (lint-ignore, documented exception).
- A convention "violation" the relevant doc doesn't actually state.

## Step 5 — Write the review markdown

Default location: `reviews/<branch-slug>.md` at repo root (branch `/` → `-`, e.g. `reviews/Refactor-Microservices.md`). Create the `reviews/` dir if missing. If the user named a file, or a review file for this branch already exists (including a legacy one like `plans/PR_FEEDBACK.md`), use that instead.

File shape:

```markdown
# Code review — <branch>

**Reviewed up to commit:** `<full-HEAD-sha>`  _(<today's ISO date>)_

> Range reviewed: `<short-start>..<short-head>` (N commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [ ] **<ID> — <SEVERITY> — <lens>** — `file_path:line`
  <one-line description + which doc/rule it violates, quoting the rule>
```

- Group by lens or severity, whichever reads better for the count.
- Give each finding a short stable ID (e.g. `MS1` microservice, `MB1` module-boundary, `BUG1`, `SEED1`, `CV1` convention) so `incremental-review` runs can append new IDs without renumbering.
- If a review file already exists, **append** a new dated `## Incremental review — <date>` section rather than overwriting prior findings; preserve existing status marks.
- No findings → write `No issues found. Checked correctness, microservice isolation, module boundaries, seeding, and C# conventions.`

## Step 6 — Stamp the marker (mandatory)

Set the top-of-file marker to current HEAD — exactly one such line in the file:

```
**Reviewed up to commit:** `<full-HEAD-sha>`  _(<today's ISO date>)_
```

Today's date comes from session context; get the SHA from `git rev-parse HEAD`. Do not commit unless asked.

## Step 7 — Report

Concise chat summary: range reviewed (`<short>..<short>`, N commits), finding counts by lens/severity (or "none"), the file written, and the stamped watermark. Point at the file; don't restate every finding in chat. No "Generated with Claude" trailers anywhere.
