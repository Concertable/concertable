# Polyrepo-ready guidance corpus

> **Next steps live in @plans/docs/POLYREPO_READY_PROGRESS.md → `## Next Steps`.**

Finish the split the polyrepo cut requires, **before** the cut rather than after it. The restructure
divided the corpus by portability — generic rules out to `dotagents`/`react-agents`, this system's roster
to `agent-standards`, the floor in-repo. Right axis, applied as though this repo survives.
[`plans/platform/POLYREPO_ROADMAP.md`](../platform/POLYREPO_ROADMAP.md) records the ruling that it does
not: services become independently-developed repos, so `api/` and a shared `plans/` tree are destinations
with no future.

**Sequencing is the point.** Land this before the cut and eight repos inherit a correct corpus on day
one. Land it after and it is eight repos to fix — the copy-and-drift failure this epic exists to kill,
reintroduced at repo scale.

## What is actually wrong, measured

| Problem | Measure |
|---|---|
| ~~Generic plan process sits in a repo with no future~~ — **moved, Phase 1** | `plans/agents/PLAN.md` 233 lines · `PROMPTS.md` 57 · `plans/agents/ROADMAP.md` 34 — **324 lines**, of which 32 carried a Concertable-specific name or command (the 259 first recorded here undercounted; `wc -l` on the files at the time of the move is the number) |
| Route rows anchored on the monorepo layout | 3 of 37: `^api/…`, `^app/…`, `^plans/…` — no such prefix exists in a service repo |
| Hub docs open by describing a monorepo | root `AGENTS.md` line 1; `docs/INDEX.md` is a monorepo index |

Six sibling process docs — branching, committing, merging, remote validation, docs-and-debt,
failing-tests — already moved to `standards/process/`. Plans moved 78 lines and left 259. There is no
reason for the asymmetry beyond the restructure scoping `PLANS.md` narrowly and nobody re-deriving it,
which is the same failure that kept the `concertable-` prefix alive on an argument already dead when it
was read.

**The four layer routes survive the cut unchanged** (`.Application/`, `.Api/`, `.Domain/`,
`.Infrastructure/`) because they key on architecture rather than location. That is the shape the
re-anchor should follow, not a second set of prefixes.

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

**What stayed in-repo, deliberately:** `plans/AGENTS.md` (71 → 75 lines) keeps the `plans/<epic>/`
layout, `plan_graph.py` and `plan_handoff_stop.py`, `worktrees.ps1 close -PlanManaged`, the debug-skill
names by tier, `/resume-plan` and `/continue-roadmap`, `initial-migrations.ps1`, the merge-queue E2E
tier, and the carve's own instance of the breaking-contract rule. That is genuinely per-repo and is what
each carved service will carry.

**Gate — met.** `plan_graph.py` 0 errors, `docs_reachability.py` 0 errors (26 pre-existing warnings, all
in `plans/`); the `plans` route still fires on a `plans/**/*.md` write; hook tests 14/14 here and 161/161
in `agent-standards`; no guidance doc links a moved file (the only surviving mentions are historical
records in spent ledgers and review files).

### Phase 2 — re-anchor the three monorepo-shaped route rows

Make the two area floors match both layouts, keyed on the same principle as the layer rows — what a file
*is*, not where the monorepo happens to put it. A standalone service repo has no `api/` prefix, so an
anchored floor silently matches nothing there: precisely the failure the floors were added to remove.

**Gate:** replay every tracked path through the table and confirm 100% coverage **in both shapes** — the
monorepo tree, and a simulated carved tree with the prefix stripped.

### Phase 3 — re-premise the hub docs

Root `AGENTS.md` opens "It is a monorepo (a convenience, not the architecture)". A carved service repo
inheriting that inherits a false statement; same for `docs/INDEX.md`'s framing. Reword so the corpus
describes a platform of services, with the monorepo named as current packaging rather than premise.

### Phase 4 — prove it on one service, or it is not done

Take the smallest independently-carvable service and check its guidance standalone: every route fires,
every skill resolves, no doc links a path outside the service, no doc asserts a monorepo. Payment is the
candidate — an adapter service with the fewest inbound dependencies.

**This is the only phase that tests the claim.** The first three are edits; this is the evidence.

## Explicitly not in scope

- Moving plan *documents* (4c above).
- The polyrepo cut itself, its seam decision, or any repo creation.
- `docs/analyzer-pushdown`, which is independent of this.
