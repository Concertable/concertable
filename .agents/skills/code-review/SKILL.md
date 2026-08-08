---
name: code-review
description: Full code review of a branch diff against Concertable's conventions, module-boundary rules, and microservice-isolation rules. Runs the host tool's NATIVE general review FIRST (Claude's built-in catalog via the `code-reviewer` subagent, or Codex's native code review), then layers Concertable's architecture-aware lenses on top — correctness bugs plus convention/boundary/microservice anti-patterns (B2B and Customer are separate services that must only communicate via *.Contracts integration events — never each other's runtime) plus missing test coverage on changed paths — filters to high-confidence findings, merges both layers into one per-branch review markdown, and stamps the reviewed-up-to commit SHA at the top. It is a SUPERSET of the built-in review, never a replacement. Use when the user wants to "code-review my changes", "review this branch", "review the PR", or "do a full review". For re-reviewing only commits added since a previous review, use the `incremental-review` skill (a thin wrapper around this one). The GitHub PR `/review` is unrelated and untouched.
---

# code-review

Full code review of the current branch's diff in **two layers, both mandatory**: **Layer 1** is the host tool's *native* general review (correctness, reuse, simplification, efficiency, error handling — Step 1c), run first and captured; **Layer 2** is Concertable's architecture-aware lenses (Steps 2–4), the checks no native review can know. Both land in one per-branch review markdown with a `Reviewed up to commit:` SHA marker at the top, so a later `incremental-review` run knows exactly where this review stopped.

Layer 1 exists because a project skill named `code-review` shadows the built-in `/code-review` — so this skill reproduces the native pass (Step 1c) instead of losing it, which is exactly what "replacing" the built-in previously did.

`incremental-review` is this skill with one input changed: it starts the diff at a recorded SHA instead of the branch's merge-base. Everything else — the lenses, the confidence filter, the output file, the marker — is identical. Keep them in sync: a change to the review procedure here is inherited by `incremental-review`.

## When to use

- "review this branch", "code review my changes", "review the PR", "do a full review"
- First review of a branch (no prior review markdown exists yet)

## When NOT to use

- Re-reviewing only what changed since the last review → `incremental-review` (it reads the SHA marker and scopes to `SHA..HEAD`).
- A massive branch (100s/1000s of files) → `big-review` (stages this skill by area).
- An exhaustive multi-agent pass → run a `Workflow` (ultracode).

## Step 1 — Confirm the checkout, then determine the review range

This skill reviews the git repo **in the current working directory** — it takes no path argument and
infers everything from CWD. When the branch lives in a git *worktree* (a sibling checkout like
`…/<repo>.worktrees/<Branch>`), the session must already be running **inside that worktree**, or the
diff is against the wrong repo/branch. So identify the checkout first, then the range:

- **Checkout** = `git rev-parse --show-toplevel` + `git branch --show-current`. Echo both so a
  wrong-checkout run is caught immediately, not silently reviewed.
- **Start** = merge-base with main: `git merge-base main HEAD` (reviews the whole branch).
- **End** = `HEAD` (`git rev-parse HEAD`).

(The `incremental-review` wrapper overrides **Start** with the SHA from the review markdown's marker; do not change anything else.)

Show the checkout + range to the user:

```powershell
git rev-parse --show-toplevel
git branch --show-current
git rev-parse HEAD
git merge-base main HEAD
git log --oneline "<start>..HEAD"
git diff "<start>..HEAD" --stat
```

If the range is empty **or** the current branch is `main`, that is the wrong-checkout symptom (the
session was started in the main checkout, not the feature's worktree) — say so and stop rather than
reviewing nothing.

## Step 1b — Create the review file NOW, before reviewing (mandatory)

**Create the review markdown immediately** — the moment the range is known, before loading rules or
reviewing anything. Do not defer file creation to Step 5; a review that's interrupted mid-flight must
still leave a file on disk.

- Resolve the target path exactly as Step 5 does (`reviews/<branch-slug>.md`, or an existing/named file).
- **If the file does not exist:** create `reviews/` if needed and write the Step-5 skeleton now — the
  `# Code review — <branch>` header, the work-order blurb, the `**Reviewed up to commit:**` marker set
  to current HEAD, the range line, and a `## Findings` section containing a single placeholder line
  `- _(review in progress — findings appended as they're confirmed)_`.
- **If the file already exists** (a prior review, an `incremental-review` run, or a legacy
  `plans/PR_FEEDBACK.md`): leave its contents intact — you'll append per Step 5. Do not overwrite.

Then review (Steps 2–4) and **append each confirmed finding to this file as you go** (replacing the
placeholder line on the first real finding), rather than buffering them all for a single write at the
end. Step 5 then just reconciles the final list; Step 6 finalizes the marker.

## Step 1c — Native review layer (Layer 1 — run FIRST, capture findings)

Before loading Concertable's rules, run the host tool's native general review over the same `<start>..HEAD` range and fold its findings into the work-order as `NAT#`:

- **Claude Code:** spawn the `code-reviewer` subagent (Agent tool) with the range; it returns the built-in catalog's findings (correctness, reuse, simplification, efficiency, error handling) as markdown. Direct `/code-review` is unavailable here — this skill shadows that name and the built-in is non-sub-invocable — so the subagent is the supported capture path.
- **Codex:** run Codex's native code review over the range and capture its findings.

Append the returned findings under `## Findings` immediately (as `- [ ] **NAT# — <SEVERITY> — native** — file:line`), so an interrupted run still records them. They pass through Step 4's confidence bar during Step 5's reconcile.

## Step 1d — Security layer (only when the range touches security-sensitive paths)

Run `git diff --name-only <start>..HEAD`. If any path hits Auth, Payment, `*.Contracts`, a `*Controller*.cs`, auth/authz middleware, a secret/credential/config file, or `.github/workflows/**`, also run the host's security review (Claude `/security-review`; Codex's equivalent) over the range, fold any findings in as `SEC#`, and stamp a second marker at the top of the work-order:

```
**Security-reviewed up to commit:** `<full-HEAD-sha>`  _(<today's ISO date>)_
```

The merge gate's `_SECURITY_PATTERNS` (in `merge-review-gate.py`) is the source of truth for which paths count; it refuses to merge a security-sensitive branch without this marker current at HEAD. No sensitive paths → skip this step, no marker.

## Step 2 — Load the rules (read before flagging anything)

These docs are the source of truth. Read the ones relevant to the diff — do not rely on memory, and only flag a convention issue a doc actually states:

- Root `AGENTS.md` and `api/AGENTS.md` — top-of-context rules + pointers.
- `api/ARCHITECTURE.md` and root `ARCHITECTURE.md` — **microservice premise** (the boundary rules below).
- `api/agents/CODE_CONVENTIONS.md` — C# conventions (source-generated logging, field naming, ctors, etc.).
- `api/agents/MODULAR_MONOLITH_RULES.md` — module boundaries within a service.
- `api/agents/SEEDING_CONVENTIONS.md` — what may and may not be seeded directly.
- Any `AGENTS.md` in directories the diff touches (each service / module may add local rules).

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

### Lens C — Module boundaries (`api/agents/MODULAR_MONOLITH_RULES.md`)

- Cross-module calls not going through `Contracts` / the module facade (`IXModule`).
- EF queries inlined in a module facade (facades delegate to Application abstractions).
- A module writing through `IUnitOfWork` (tied to `ApplicationDbContext`, silently no-ops) instead of `xRepository.SaveChangesAsync()`.
- Impl types left `public` when an interface was extracted to `internal`.

### Lens D — Seeding (`api/agents/SEEDING_CONVENTIONS.md`)

- A seeder directly writing data whose only production write path is a reaction (read-model projections, `UserEntity`, manager profiles, Stripe `PayoutAccount`, inbox/outbox rows). The fix is to drive the event, never `context.X.AddRange(...)`.
- `IDevSeeder` vs `ITestSeeder` misuse (`ITestSeeder` never runs in dev/E2E).
- Integration events published from a service layer instead of raised from a domain event.

### Lens E — C# conventions (`api/agents/CODE_CONVENTIONS.md`)

- Inline logging templates (`logger.LogInformation("...")`) instead of a source-generated `[LoggerMessage]` in the project's `Log.cs`.
- Primary constructors on services/repos/handlers/validators (use explicit ctor + `private readonly` fields, no `_` prefix).
- `is { }` capture instead of `is not null`; unnecessary braces on single-statement `if`/`else`.
- Additive EF migrations (model changes re-scaffold via `./initial-migrations.ps1`).

### Lens F — Test coverage of changed behaviour

A behaviour the diff **adds or alters** that nothing asserts. The fix is concrete — name the test to write — so it obeys Step 4's no-hedge rule exactly like any other finding (the fix is "add test X", not "consider more tests"). `/review` catches these; this lens is why code-review now does too.

- A new or rewritten service method / handler / endpoint whose success **and** failure branches have no covering test.
- A refactor that re-routes a path through a new collaborator (e.g. reading from a repository instead of a service) with no test exercising the new wiring — even when behaviour is *preserved*: the wiring is new and unpinned.
- A deleted test that removed the only coverage of a path that still exists.

Do **not** flag: pure renames, DI-registration-only changes, generated code, or a path an existing test still exercises unchanged. This is not "add more tests" — it is one concrete missing assertion on a path this diff touched.

## Step 4 — Confidence filter

For each candidate finding, judge whether it's real and will be hit in practice. **Drop anything below ~80/100 confidence.** Discard these false positives:

- Pre-existing issues on lines not changed in this range.
- Things a compiler / linter / CI catches (type errors, imports, formatting).
- Pedantic nitpicks a senior engineer wouldn't raise.
- Intentional changes that are part of the broader refactor.
- Issues deliberately silenced in code (lint-ignore, documented exception).
- A convention "violation" the relevant doc doesn't actually state.

**No hedged findings — a kept finding is one you'd fix.** Every finding that survives this filter must
name a *concrete fix you're prepared to apply*. Do not emit conditional/hedged findings — "sub-threshold
but noting it", "only worth it if this is ever refactored", "non-blocking, your call". That middle
ground is a trap: it's above the bar → state the fix (and it gets applied), or below the bar → drop it
silently. Never the hedge. A hedged finding reads downstream as "human decision needed" — `address-review`
defers it, and a reversible, clearly-correct, repo-rule-backed fix wrongly turns into a permission
question instead of just being made. Severity `LOW` still means "fix it", not "maybe fix it".

## Step 5 — Finalize the review markdown

The file already exists (created up front in Step 1b) and findings were appended as they were
confirmed. Here you just reconcile the final list: ensure the placeholder line is gone, findings are
grouped and ID'd, and the shape below is honoured.

Path (same resolution as Step 1b): `reviews/<branch-slug>.md` at repo root (branch `/` → `-`, e.g. `reviews/Refactor-Microservices.md`). If the user named a file, or a review file for this branch already exists (including a legacy one like `plans/PR_FEEDBACK.md`), that file is the one you've been writing to.

File shape:

```markdown
# Code review — <branch>

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `<full-HEAD-sha>`  _(<today's ISO date>)_

> Range reviewed: `<short-start>..<short-head>` (N commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [ ] **<ID> — <SEVERITY> — <lens>** — `file_path:line`
  <one-line description + which doc/rule it violates, quoting the rule>
```

- Group by lens or severity, whichever reads better for the count.
- **Merge both layers into the one list.** Native findings (Step 1c) keep `NAT#` IDs; security findings (Step 1d) keep `SEC#`; the architecture lenses use `MS1` microservice, `MB1` module-boundary, `BUG1`, `SEED1`, `CV1` convention. Stable IDs so `incremental-review` runs append without renumbering.
- **Dedup across layers:** when a native correctness finding (`NAT#`) is the same defect as a Lens-A finding, keep one entry and note both lenses — don't list it twice. Native findings still pass Step 4's confidence bar and the no-hedge rule; drop any that don't clear it.
- If a review file already exists, **append** a new dated `## Incremental review — <date>` section rather than overwriting prior findings; preserve existing status marks.
- No findings → write `No issues found. Checked correctness, microservice isolation, module boundaries, seeding, C# conventions, and test coverage of changed paths.`

## Step 6 — Stamp the marker (mandatory)

Set the top-of-file marker to current HEAD — exactly one such line in the file:

```
**Reviewed up to commit:** `<full-HEAD-sha>`  _(<today's ISO date>)_
```

Today's date comes from session context; get the SHA from `git rev-parse HEAD`. If Step 1d ran, its
`Security-reviewed up to commit:` marker is stamped at HEAD too. Do not commit unless asked, except for
a plan-managed checkpoint required by Step 7.

## Step 7 — Checkpoint and report

Before any report or stop, if this workflow is plan-managed, read and apply
[the shared plan-progress checkpoint](../resume-plan/references/plan-progress-checkpoint.md).

Concise chat summary: range reviewed (`<short>..<short>`, N commits), finding counts by lens/severity (or "none"), the file written, and the stamped watermark. Point at the file; don't restate every finding in chat. No "Generated with Codex" trailers anywhere.
