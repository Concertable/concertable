# Polyrepo-ready guidance corpus

> **Next steps live in @plans/docs/POLYREPO_READY_PROGRESS.md → `## Next Steps`.**

Finish the split the polyrepo cut requires, **before** the cut rather than after it. The restructure
divided the corpus by portability — generic rules out to `dotagents`/`react-agents`, this system's roster
to `agent-standards`, and a third bucket it called "the floor", left in-repo. Right axis, wrong number of
destinations: that third bucket is the one §6 deletes.
[`plans/platform/POLYREPO_ROADMAP.md`](../platform/POLYREPO_ROADMAP.md) records the ruling that it does
not: services become independently-developed repos, so `api/` and a shared `plans/` tree are destinations
with no future.

**Sequencing is the point.** Land this before the cut and eight repos inherit a correct corpus on day
one. Land it after and it is eight repos to fix — the copy-and-drift failure this epic exists to kill,
reintroduced at repo scale.

## What is actually wrong, measured

| Problem | Measure |
|---|---|
| ~~Generic plan process sits in a repo with no future~~ — **moved, Phase 1** | `plans/agents/PLAN.md` 233 lines · `PROMPTS.md` 57 · `plans/agents/ROADMAP.md` 34 — **324 lines**, of which 32 carried a Concertable-specific name or command (the 259 first recorded here counted non-blank lines; these are `wc -l` at the time of the move) |
| The route table has no home after the cut | 37 rows in root `.agents/skill-routes.json`. `agent-standards` **vendors the hook** (`vendor-hooks.ps1`, provenance-hashed) but ships **no** table — so the table is per-repo data, and 3 rows (`^api/…`, `^app/…`, `^plans/…`) name paths a service repo does not have. The *convention* those 37 rows follow has no owner anywhere |
| The hub docs are in the deleted root | root `AGENTS.md` (147 lines) and `docs/INDEX.md` — not "they open by describing a monorepo", which is the wording; the problem is every rule in them needs a destination |
| The 28 workflow skills are in the deleted root | 2,900 lines in `.agents/skills/`, every family platform-wide — Phase 5 |

Six sibling process docs — branching, committing, merging, remote validation, docs-and-debt,
failing-tests — already moved to `standards/process/`. Plans moved 78 lines and left 259. There is no
reason for the asymmetry beyond the restructure scoping `PLANS.md` narrowly and nobody re-deriving it,
which is the same failure that kept the `concertable-` prefix alive on an argument already dead when it
was read.

**Rows keyed on architecture port; rows keyed on location don't.** The four layer routes
(`.Application/`, `.Api/`, `.Domain/`, `.Infrastructure/`) mean the same thing in a service repo; the three
area floors name monorepo directories and mean nothing there. That is the distinction the convention has to
encode — not a reason to keep one table alive in two shapes.

## Decisions taken, so no phase waits on a question

1. **Generic process stays in `Concertable/agent-standards`, and its charter is reworded to admit it.**
   That repo opens with "everything that is true of **Concertable specifically**", then holds seven
   generic process docs — its own README concedes "branching and merging are neither stack". Every repo
   the cut produces is a Concertable service, so a Concertable-scoped plugin carrying generic process
   costs nothing. **Rejected: a fourth `process-agents` repo.** It is the only option that would let the
   DevOps there), and three-repo coordination already produced two stale-standards incidents in a single
   session. Four is worse. Revisit only if a work repo actually wants the plan method.
2. **Copying is not the answer, at any tier.** A developer works on two or three services — B2B and
   Payment, or Customer alone — so the standards must be identical across the repos they clone.
   Per-machine plugin install already gives that. Committing `plans/agents/` into eight repos means one
   copy is edited and seven rot.
3. **Out of scope: `POLYREPO_ROADMAP` item 4c.** Where a `launch` plan spanning four services physically
   lives is contentious and gated on that roadmap's §6. How to *write* a plan has no locality question,
   so it does not wait behind it.

## The only two destinations

[`POLYREPO_ROADMAP.md`](../platform/POLYREPO_ROADMAP.md) §6, decided 2026-08-18: *"there is no `api/` node
in a polyrepo, so `api/agents/` and `api/AGENTS.md` are destinations with no future. Everything in them
re-homes to `standards/` (platform-wide, inherited by every service repo) or to the owning service's repo."*

**Every phase below answers one question per artifact: platform-wide → `agent-standards`, or single-service
→ that service's repo. There is no third answer.** "It names this repo's scripts", "it is the in-repo
floor", "it is genuinely per-repo" are not destinations — the root is being deleted, so anything left there
is deleted with it or replicated eight times, which is the failure this epic exists to end. The test is
**common across services**, never *does it mention Concertable*: `/merge` names the queue and
platform-sync and is still platform-wide, because all eight repos merge and all eight own a sync.

## Phases

### Phase 1 — move the generic plan process out ✅ **done**

Target `Concertable/agent-standards` (PR [#5](https://github.com/Concertable/agent-standards/pull/5)):

- [x] `plans/agents/PLAN.md` (233) → merged into `standards/process/PLANS.md`, which went 78 → 248. It
  already owned the lifecycle; this added the *method* — phases, verification gates, the four-line
  blocker schema, the ledger format.
- [x] `plans/agents/ROADMAP.md` (34) → same file, as the roadmap tier.
- [x] `PROMPTS.md` (57) → `standards/process/HANDOFF.md` (57), a new node. It is the continuation
  pointer's exact shape and nothing else defines it, so it earns its own doc rather than a section.
- [x] `plans` skill description widened; `handoff` router added; the README charter reworded to say which
  domain is a roster and which is method, and why a fourth process repo was rejected.

**What stayed in-repo — and owes the two-destination test.** `plans/AGENTS.md` (71 → 75 lines) kept the
`plans/<epic>/` layout, `plan_graph.py` and `plan_handoff_stop.py`, `worktrees.ps1 close -PlanManaged`, the
debug-skill names by tier, `/resume-plan` and `/continue-roadmap`, `initial-migrations.ps1`, the
merge-queue E2E tier, and the carve's instance of the breaking-contract rule. Calling that "genuinely
per-repo" was the mistake above in miniature: **every one of those is platform-wide.** All eight repos run
a plan graph, close plan-managed worktrees, debug by tier, and own a merge queue; only the *values* differ
(this service's script path, this service's suite names). So the content belongs in `standards/` with the
values named by each repo's own thin `AGENTS.md` — not as 75 lines copied eight times, which is what the
ledger's "generated or hand-kept?" open question was really asking. Phase 5 settles it with the rest.

**Gate — met.** `plan_graph.py` 0 errors, `docs_reachability.py` 0 errors (26 pre-existing warnings, all
in `plans/`); the `plans` route still fires on a `plans/**/*.md` write; hook tests 14/14 here and 161/161
in `agent-standards`; no guidance doc links a moved file (the only surviving mentions are historical
records in spent ledgers and review files).

### Phase 2 — give the route table's convention a home; the monorepo rows die with the root

The original framing was "re-anchor the three monorepo-shaped rows so the table works in both shapes". That
keeps a root table alive in two worlds, which §6 does not allow. The mechanism already splits the way the
rule requires: `agent-standards` vendors `skill_router.py` into each repo and ships **no** table, so the
table is per-repo *data* and the hook is the platform-wide *procedure*.

So the three area-floor rows are not re-anchored — they are values that cease to exist when the root does.
What is missing is one tier up: **the convention 37 rows follow has no owner.** That every source file is
gated by an area floor plus a layer route, that every matching row fires rather than the first, that a row
keyed on location cannot port while one keyed on architecture can, and what a row's `note` is for — all of
that lives today as prose inside the table's own notes and `docs/INDEX.md`, both in the deleted root, so
eight repos would hand-write eight tables from no stated rule.

- Publish that convention from `agent-standards` beside the vendored hook, and a template or generator that
  emits a repo's table from its own layout, so a carved repo's table is derived rather than copied.
- Keep the layer rows as the portable core; let each repo's own floors name its own top-level directories.

**Gate:** generate the table for a simulated carved service tree from the published convention, replay
every tracked path in that tree, and require 100% coverage with no row naming a path outside the repo. The
monorepo's own table only has to keep working until the root is deleted.

### Phase 3 — re-home the hub docs, rather than re-premise them

The original framing here was "root `AGENTS.md` opens *It is a monorepo* — reword it so the monorepo is
named as current packaging rather than premise". Under §6 that is effort spent on a file with no future:
root `AGENTS.md` and `docs/INDEX.md` are as deleted as `api/AGENTS.md` was. **Split their content by the
same test instead** — every rule in them is platform-wide (→ `agent-standards`, where the eight repos
inherit it) or single-service (→ that service's repo) — and let what remains at root be whatever genuinely
describes the monorepo *while it exists*, since it dies with it. Rewording the premise of a doomed hub is
the cosmetic tier; re-homing its rules is the work.

### Phase 4 — prove it on one service, or it is not done

Take the smallest independently-carvable service and check its guidance standalone: every route fires,
every skill resolves, no doc links a path outside the service, no doc asserts a monorepo. Payment is the
candidate — an adapter service with the fewest inbound dependencies.

**This is the only phase that tests the claim.** The first three are edits; this is the evidence.

### Phase 5 — the 28 workflow skills, split by the two-destination test

Phase 1 moved the plan *standards* and never looked at `.agents/skills/`. All 28 skills — 2,900 lines — sit
in the root that is being deleted, and under §6 each one is platform-wide or single-service. Applying the
test rather than the "does it name Concertable" question that produced a keep-bucket:

**Platform-wide → `agent-standards` (every service repo needs them, so a copy per repo is eight copies):**

| Family | Lines | Why it is common |
|---|---|---|
| `review` · `docs-review` · `big-review` · `incremental-review` · `address-review` · `big-review-all` | 813 | Every repo reviews before merge. Only 25 lines name anything here, and those are doc paths |
| `merge` · `merge-docs` · `pr-preflight` · `create-gh-pr` | 634 | Every repo has a queue, a docs bypass, a preflight, PRs. The queue and `platform-sync` are *platform* facts, not one service's |
| `e2e-ui-debug` · `e2e-api-debug` · `e2e-ui-regress` · `e2e-debug` · `integration-debug` · `reset-test-explorer` | 1,022 | Every repo debugs a red suite by tier. The *procedure* is common; the artifact it invokes (`scripts/e2e.ps1`, the suite names) is the per-repo value |
| `commit` · `commit-all` · `push` · `pull` · `sync` · `worktree` | 429 | Zero lines name this repo. Also needs reconciling with `dotagents`' `commit-push`/`sync`/`pull-main`, which already duplicate them |
| `resume-plan` · `continue-roadmap` · `update-roadmap` · `techdebt` · `auto-memory` | 203 | The executable counterparts of the `plans` skill Phase 1 already moved |
| `package-cutover` | 184 | Published-contract cut-over is the carve's own mechanic, identical in every repo that consumes the feed |

**Single-service → that service's repo:** nothing in the current roster, which is the finding. What each
repo carries is *values*, not procedure: its own `scripts/e2e.ps1`, its suite names, its hook paths, its
`initial-migrations.ps1` — named in its own thin `AGENTS.md`, the way `Concertable.Payment` already models.

So the skill *bodies* leave, parameterised over those values, and each carved repo keeps a short floor that
supplies them. The same applies to `plans/AGENTS.md`, which Phase 1 left behind (see its note above).

**Order by cost of duplication:** review family → merge/PR family → the test-debug family (largest, and
needs the parameterisation decided first) → git family (plus the `dotagents` overlap) → plan-workflow.

**Gate:** for each moved skill, a simulated carved tree loses no rule, the router resolves from a fresh
install, and what remains at root is only values that repo genuinely owns. Phase 4 depends on all of it —
"prove it on one service" is meaningless while a carved Payment repo would have no review, merge, or debug
skill at all.

## Explicitly not in scope

- Moving plan *documents* (4c above).
- The polyrepo cut itself, its seam decision, or any repo creation.
- `docs/analyzer-pushdown`, which is independent of this.
- ~~The workflow skills in `.agents/skills/`~~ — silently out of scope until Phase 5 named them, and first
  classified with a "genuinely local, keep" bucket that §6 does not allow. Phase 1 measured three documents
  and moved three documents; nothing in it examined the skill roster.
