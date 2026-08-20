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
| The route table has no home after the cut | 36 rows in root `.agents/skill-routes.json`. `agent-standards` **vendors the hook** (`vendor-hooks.ps1`, provenance-hashed) but ships **no** table — so the table is per-repo data, and 3 rows (`^api/…`, `^app/…`, `^plans/…`) name paths a service repo does not have. The *convention* those 36 rows follow has no owner anywhere |
| The hub docs are in the deleted root | root `AGENTS.md` (147 lines) and `docs/INDEX.md` — not "they open by describing a monorepo", which is the wording; the problem is every rule in them needs a destination |
| The 28 workflow skills are in the deleted root | **3,285** lines in `.agents/skills/`, every family platform-wide — N1 |

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
ledger's "generated or hand-kept?" open question was really asking. N1 and N7 settle it with the rest.

**Gate — met.** `plan_graph.py` 0 errors, `docs_reachability.py` 0 errors (26 pre-existing warnings, all
in `plans/`); the `plans` route still fires on a `plans/**/*.md` write; hook tests 14/14 here and 161/161
in `agent-standards`; no guidance doc links a moved file (the only surviving mentions are historical
records in spent ledgers and review files).

## The remaining work is one node at a time

Phase 1 proved the shape: measure a node, move the platform-wide content to `agent-standards`, leave the
per-repo values behind, land producer then consumer. Everything left is that same operation applied to the
next node, and the nodes are known and measured (`origin/main` at `1d15a7920`):

| # | Node | Size | Destination | Leaves behind |
|---|---|---|---|---|
| N1 | `.agents/skills/` — 18 skills left of 28 | **1,838** of 3,285 lines | platform-wide, four families left of six | per-repo values: script paths, suite names |
| N2 | `.agents/skill-routes.json` — 36 rows | 36 rows | the *convention* + a generator | the table itself: per-repo data |
| N3 | `api/AGENTS.md` + `api/CLAUDE.md` | 78 | platform-wide | nothing — §6 deletes this node |
| N4 | `api/ARCHITECTURE.md` + `api/docs/MICROSERVICES_ARCHITECTURE.md` | 62 + 525 | platform-wide (cross-service by definition) | nothing |
| N5 | root `AGENTS.md` | 147 | platform-wide, minus the monorepo-only lines | nothing |
| N6 | `docs/` — `INDEX.md` 188 · `USP.md` 203 · `DEEP_RESEARCH_PROMPT_GUIDE.md` 81 · `OVERVIEW.md` 55 · `REMOTE_VALIDATION.md` 27 | 554 | mixed — see N6 | nothing |
| N7 | `plans/` tree + `plans/AGENTS.md` 75 | tree | platform-wide + per-repo values | gated on roadmap §4c |
| N8 | *(gate)* prove one carved service standalone | — | — | — |

**One node per slice, in this order.** N1 first: every hub below it points *at* skills, so re-homing a hub
before its targets have addresses writes pointers to nowhere. N2 is independent and can run in parallel.
N3–N6 follow N1. N7 waits on an external decision. N8 is last and is the only evidence.

### N1 — the 28 workflow skills (3,285 lines)

Under §6 each skill is platform-wide or single-service. Applying that test rather than "does it name
Concertable" (which is what produced a keep-bucket in an earlier draft of this plan):

| Family | Lines | Why it is common |
|---|---|---|
| ~~`review` · `docs-review` · `big-review` · `incremental-review` · `address-review` · `big-review-all`~~ **done** | 813 | Every repo reviews before merge. Landed as seven `standards/process/review/` docs — `reviews/AGENTS.md` joined the family, since the review-file lifecycle is the same rule in every repo |
| ~~`merge` · `merge-docs` · `pr-preflight` · `create-gh-pr`~~ **done** | 634 | Every repo has a queue, a docs bypass, a preflight, PRs. Landed as four `standards/process/merge/` docs. `create-gh-pr` became `open-pr`: a plugin installs per machine, and that name already belonged to a contradictory work-repo procedure |
| `e2e-ui-debug` · `e2e-api-debug` · `e2e-ui-regress` · `e2e-debug` · `integration-debug` · `reset-test-explorer` | 1,022 | Every repo debugs a red suite by tier. The *procedure* is common; the artifact it invokes (`scripts/e2e.ps1`, the suite names) is the per-repo value |
| `commit` · `commit-all` · `push` · `pull` · `sync` · `worktree` | 429 | Zero lines name this repo. Also needs reconciling with `dotagents`' `commit-push`/`sync`/`pull-main`, which already duplicate them |
| `resume-plan` · `continue-roadmap` · `update-roadmap` · `techdebt` · `auto-memory` | 203 | The executable counterparts of the `plans` skill Phase 1 already moved |
| `package-cutover` | 184 | Published-contract cut-over is the carve's own mechanic, identical in every repo consuming the feed |

**Single-service: none.** That is the finding. What a carved repo keeps is *values* — its `scripts/e2e.ps1`,
its suite names, its hook and migration paths — named in a thin `AGENTS.md` on the `Concertable.Payment`
model. The same applies to `plans/AGENTS.md` (N7).

One family per slice, ordered by cost of duplication: ~~review~~ → ~~merge/PR~~ → test-debug (largest; needs the
parameterisation decided first) → git (plus the `dotagents` overlap) → plan-workflow → `package-cutover`.

**Gate per family:** a simulated carved tree loses no rule, the router resolves the moved skill from a
fresh install, and what stays at root is only values this repo owns.

**The review and merge/PR families both answered "how do you parameterise a workflow?" — you don't.** Neither
leaves a values file behind. Every repo-specific input is resolved mechanically at run time: the review
family reads that repo's route table through `skill_router.py --skills-for`, every `AGENTS.md` in a touched
directory, and whichever architecture doc the root `AGENTS.md` names; the merge family reads the slug from
`gh repo view`, the check set from `gh pr checks`, the queue config from the rulesets API, and "does this
merge publish?" from the repo's own publish path filter — which is also where its one genuinely
monorepo-shaped value (`api/**`) got re-anchored. Where a value is identical in every repo — the tier
labels, the vendored hook paths, `scripts/worktrees.ps1` — **state it rather than invent a parameter with
one value.** The remaining families try that shape first and reach for a named value only where a *path a
script lives at* genuinely cannot be discovered — still the open question for test-debug, which names
`scripts/e2e.ps1` and `scripts/docker-health.ps1`. Settle it by deciding whether those scripts should be
**vendored** the way the hooks already are; if they are, the path is a constant and there is still no values
file.

**A skill's name is machine-scoped, not corpus-scoped.** The merge family found `create-gh-pr` already taken
on the same machine by a contradictory work-repo procedure reached through junctions, and a plugin installs
per machine. Check a new name against every repo on the machine, not just the three standards repos and the
harness built-ins — and expect it again for `worktree`, which family 4 shares with both `dotagents` and the
work repos.

### N2 — the route table's convention (36 rows)

The mechanism already splits the way §6 requires: `agent-standards` vendors `skill_router.py` through
`vendor-hooks.ps1` (provenance-hashed) and ships **no** table, so the hook is platform-wide procedure and
the table is per-repo data. The three area-floor rows (`^api/`, `^app/`, `^plans/`) are therefore values
that cease to exist with the root — nothing to re-anchor.

What has no owner is one tier up: **the convention those 36 rows follow.** That every source file is gated
by an area floor plus a layer route, that every matching row fires rather than the first, that a row keyed
on location cannot port while one keyed on architecture can, what a row's `note` is for — all of it lives
today inside the table's own notes and `docs/INDEX.md`, both in the deleted root. Eight repos would
hand-write eight tables from no stated rule.

- Publish the convention from `agent-standards` beside the vendored hook, plus a template or generator that
  emits a repo's table from its own layout, so a carved table is derived rather than copied.
- Keep the layer rows as the portable core; each repo's floors name its own top-level directories.

**Gate:** generate the table for a simulated carved tree from the published convention, replay every
tracked path in that tree, require 100% coverage and no row naming a path outside the repo.

### N3 — `api/AGENTS.md` + `api/CLAUDE.md` (78 lines)

§6 names this node explicitly: *"there is no `api/` node in a polyrepo."* It is already pointers-only with
no `@`-imports, so this is a re-home, not a rewrite: the pointers that are platform-wide (the service
roster, the boundary rule, which skill owns what) go to `agent-standards`; anything naming one service was
already pushed down in roadmap 4a/4b. Nothing stays.

### N4 — `api/ARCHITECTURE.md` (62) + `api/docs/MICROSERVICES_ARCHITECTURE.md` (525)

Cross-service by definition — the carve, the publish→sync loop, Contracts-only dependencies. Platform-wide
in full, and the largest single document left. It is also what every service repo needs on day one to know
what it may depend on, so it cannot be the last node moved.

### N5 — root `AGENTS.md` (147 lines)

Split by the same test. The monorepo-only lines (that this *is* a monorepo, where `api/` and `app/` sit) do
not port and die with the root; everything else — the scalable-fix rule, the autonomy rules, the merge and
platform-sync invariants, doc locality, the review gates — is platform-wide. Rewording its opening sentence
so the monorepo reads as "current packaging" is the cosmetic tier and is explicitly **not** the work.

### N6 — `docs/` (554 lines across five files)

Three different answers in one folder, so it is one node with three outcomes:

- `INDEX.md` (188) — the topic→owner map. Platform-wide, and its per-repo half is a short "what this repo
  owns" list. Blocked on N1: the map's rows point at skills, so it can only be written once they have homes.
- `REMOTE_VALIDATION.md` (27) — already a thin pointer at the `remote-validation` standard; folds into the
  per-repo floor with its commands.
- `OVERVIEW.md` (55), `USP.md` (203), `DEEP_RESEARCH_PROMPT_GUIDE.md` (81) — **not agent guidance and not
  service-specific: product narrative.** Neither destination fits, which makes this the one genuinely open
  question in the plan. **Do not invent a home** — surface it with N6 and let Tommy place them.

### N7 — the `plans/` tree and `plans/AGENTS.md` (75 lines)

Gated on roadmap §4c (where a cross-service plan physically lives), which is gated on §6's remaining
sub-decision. The file itself is not gated: every rule in it is platform-wide with per-repo values, which is
also the answer to the ledger's old "generate it eight times or hand-keep it?" question — neither, the
content leaves.

### N8 — prove one carved service standalone, or none of this is done

Take the smallest independently-carvable service and check its guidance alone: every route fires, every
skill resolves, no doc links a path outside the service, no doc asserts a monorepo. Payment is the
candidate — an adapter service with the fewest inbound dependencies.

**This is the only node that produces evidence rather than edits**, and it depends on N1–N6: a carved
Payment repo with no review, merge or debug skill cannot prove anything.

## Explicitly not in scope

- Moving plan *documents* (4c above).
- The polyrepo cut itself, its seam decision, or any repo creation.
- `docs/analyzer-pushdown`, which is independent of this.
- ~~The workflow skills in `.agents/skills/`~~ — silently out of scope until N1 named them, and first
  classified with a "genuinely local, keep" bucket that §6 does not allow. Phase 1 measured three documents
  and moved three documents; nothing in it examined the skill roster.
