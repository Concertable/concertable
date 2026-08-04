# Working in `plans/`

`plans/` holds two kinds of doc, each with its own playbook in [`agents/`](agents/):

- **Plans** — working docs for unfinished, multi-step work, each with a `_PROGRESS.md` ledger, deleted
  when the work is done. How they work → [`agents/PLAN.md`](agents/PLAN.md).
- **Roadmaps** (`*_ROADMAP.md`) — an epic's living progress tracker: no ledger, never deleted, lives
  until the epic ships. Each item spins off its own plan. How they work → [`agents/ROADMAP.md`](agents/ROADMAP.md).

Git history is the archive; a finished plan kept "for reference" is rot that misleads the next reader.
This file carries the cross-cutting rules for *doing* the work; the root [`AGENTS.md`](../AGENTS.md)
carries the short version.

## Commit finished work the instant its gate goes green — this is unconditional

**The default is to commit, not to "leave it for review."** The moment a discrete chunk of work is
complete and its gate has passed (build green + the affected tests), **commit it, without asking.**
Committing is a local, reversible checkpoint; only `push` waits for Tommy's explicit go-ahead
(**commit ≠ push**).

This rule is **not** scoped to phase boundaries or handoffs; it applies to *every* finished, verified
chunk: a phase, a bug fix, a refactor, an investigation's output, a doc close-out. Reading the commit rule as conditional on
handing off / clearing is exactly the misread that produces the failure below.

**"I've left it uncommitted so you can look at it first" is the anti-pattern, not the courtesy:**

- **Review runs off commits.** `/code-review` diffs `main..HEAD`; work sitting in the working tree
  is invisible to it. Leaving it uncommitted is precisely what stops the reviewer seeing it.
- **Uncommitted is the fragile state.** It survives no `git checkout`, no stray `git restore`, no
  context clear. A commit is the cheapest insurance that exists.
- **It silently wrecks PR scoping.** Finished work left loose gets swept into an unrelated PR by a
  later `git add -A`, or stranded when the branch moves on — and a reviewer opening the PR finds the
  feature the branch is *named after* missing from it.

If finished work belongs in a **different PR** than the branch you're on, that is a reason to
**branch and commit it there** — never a reason to leave it sitting in the working tree.

**Mechanical trigger:** the gate went green → commit. If your next sentence would be *"should I
commit this?"* or *"I've left it uncommitted for your review"*, **that sentence is the trigger** —
don't send it, commit. The only thing you ever stop and ask about is `push`.

## Use the fewest safe merges

Complete a plan in the fewest PRs its real dependencies allow. Numbered steps, commits and phases do
not each need their own PR; keep coherent work together. Split only where a merge, package publication,
platform sync or runtime deployment must finish before the next work can build or run, and group all
work possible on each side of that gate.

### Rename definition-of-done: the grep gate (mechanical, not judgement)

A rename is done **only when `grep -rniE "<oldterm>"` over the entire repo returns zero** — every
tier, no exceptions: type names, identifiers **of every case** (PascalCase, camelCase, snake, kebab),
local vars, fields, params, comments, string literals, `data-testid`s, HTTP routes, JSON keys, blob
paths, file/folder names, and docs. There is **no "cosmetic tier."** A field typed `IDealAccessor`
named `contractAccessor`, or a comment still saying "the agreement", is *not done* — it's exactly the
dishonest naming the rename exists to remove.

Run the grep before claiming done. The only acceptable non-zero is an **explicit, written allowlist**
of deliberate survivors, each justified in the plan — a term that legitimately means something else
now (a word reused for a different concept), a shared `.Contracts` event package, a wire value kept on
purpose. Anything not on that allowlist is outstanding work. "I renamed the types, the build's green,
tests pass" is **not** the bar — the grep is. Don't decide by hand which occurrences "count"; the whole
failure mode is discretion. Remove the discretion — grep, allowlist, zero.

## Branch first

Before any plan work, create a `Feature/<Name>` branch relevant to the plan if you're not already on one — never commit plan work to `main` or an unrelated branch.

## A failing test is never just reported — enter the matching debug skill and drive it to green

Whenever **any** test run comes back red — unit, integration, API E2E, or UI E2E — the next action is
**always** the matching debug skill, not a status report back to the user. A failure *is* the trigger;
don't make the user ask for it.

- unit / integration failures → **`integration-debug`**
- API E2E failures → **`e2e-api-debug`**
- UI E2E failures → **`e2e-ui-debug`** (or **`e2e-debug`** to sweep both layers)

The debug skill owns the whole **run → diagnose → fix → re-run** loop: find the root cause, fix it in
code (service / handler / page-object / step-def / fixture / config — wherever the real bug is), and
re-run until green. Never report a red suite and wait. Never disable, skip, `--no-build`, or
inflate-a-timeout to get past a failure — that's bypassing, not fixing. For E2E, the debug skill also
does **flaky-vs-real triage**: re-run the failed scenario alone on a fresh stack — passes clean = a
host-load blip (proven, not assumed); fails again = a real bug, so fix it.

## Merge-queue E2E tier — full by default, skip only for demonstrably low blast radius

The full E2E suites (API `Concertable.B2B.E2ETests` + the UI regress) are **expensive and
Docker-gated**. The merge queue runs them by default; the strict criteria below decide whether a PR
qualifies to opt out. Local verification still stops at build + unit + integration before a PR.

**The PR merge queue IS the E2E gate — never run E2E locally ahead of a merge.** When the change is
going out as a PR, the merge-queue pipeline runs the full suite (E2E included) as the gate. Running it
locally first just burns ~25-30 min duplicating exactly what CI will do on the way in. So for anything
headed to a PR, the local gate stops at build + unit + integration — **push it and let the queue run
E2E.** The **only** reason to run E2E locally is when **the merge fails on failing E2E tests** — then
run the failing scenarios via the **`e2e-ui-debug`** / **`e2e-debug`** skill to diagnose and fix, and
push the fix (the queue re-runs E2E on the way back in). **This overrides any plan phase line or
kickoff prompt that says "run the E2E regress"** — if a PR will run it, let the PR run it; a written
"run E2E" step is not a reason to duplicate the queue.

The merge skill's Step 4 is the single source of truth for this decision. **Full E2E is the default.**
Add `skip-e2e` only when the PR is both small and demonstrably low-blast-radius, with every one of
these true:

- The diff and affected area are small and isolated.
- It touches no package/service boundary, shared infrastructure, build/publish/deployment pipeline,
  CI workflow, or multiple application surfaces.
- It changes no user-facing/runtime flow covered by E2E.
- Unit/integration tests fully cover the affected behaviour.

**Zero intended behaviour change is not sufficient.** Package renames, lockfile/workspace changes,
shared-library moves, broad refactors, and build/publish separation still have broad blast radius and
must run full E2E. When in doubt, do not skip.

Encode a qualifying skip with the `skip-e2e` PR label (`skip-e2e-ui` for UI-only); labels are the
reliable lever and are read fresh in the merge group. Remove stale skip labels when the PR does not
qualify. If historical `Skip-E2E` / `Skip-E2E-UI` trailers would opt out a PR that now requires the
full tier, add `full-e2e`; it is the authoritative positive override. Unit and integration tests never
skip for code/package changes, and build + carve never skip. A matching git trailer also works but is
fragile because it must be in the final contiguous trailer block, so prefer the label.

When E2E must run for a PR, let the merge queue run it; this tier decision does **not** authorize a
duplicate local run. **How** to run E2E safely after a queue failure (the mandatory Docker health
pre-flight, only via the `e2e-*` skills) is unchanged — see the "E2E suites — Docker health first"
section in the root [`AGENTS.md`](../AGENTS.md).
