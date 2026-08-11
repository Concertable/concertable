---
name: update-roadmap
description: Reconcile a decision, requirement, dependency, package release, or upstream API change across an epic `*_ROADMAP.md` and every affected live plan and progress ledger. Use when Tommy asks to update a roadmap and its existing/current plans or propagate new upstream assumptions through an epic. Planning docs only; do not implement the change.
---

# Update Roadmap

Update the complete live ROADMAP -> PLAN -> PROGRESS graph from one authoritative change set.

## Workflow

1. Resolve `/update-roadmap [@plans/<epic>/<EPIC>_ROADMAP.md] <change set or source>`. Ask only when
   more than one roadmap plausibly owns it. Verify compatibility-sensitive claims against the named
   upstream source, release, package, or baseline when available.
2. Read `AGENTS.md`, `plans/AGENTS.md`, `plans/agents/ROADMAP.md`, `plans/agents/PLAN.md`,
   `PROMPTS.md`, and the roadmap. Read `api/ARCHITECTURE.md` for cross-service impact.
3. Run `python .agents/hooks/plan_graph.py --root <absolute-worktree>`. Find every live ledger whose
   `Roadmap:` header names the roadmap, then reconcile its declared branch/worktree/PR with real git
   and GitHub state. Read each plan and ledger from its owning worktree because it may be ahead of
   `main`; include reserved, blocked, delivery-gated, and in-flight plans.
4. Before editing, classify every change against every live plan as `roadmap`, `plan`, `ledger`, or
   `none`. Follow semantic dependency usage, not filename or keyword matches. Keep the reasons for
   every `none` classification for the final coverage report.
5. Preflight all affected worktrees before the first write. Verify branch ownership and inspect the
   target planning paths. Preserve unrelated dirty code; never stage it. A dirty target roadmap,
   plan, or ledger is a collision, so stop before producing a partial reconciliation.
6. Update only the owning level:
   - roadmap: shared scope, ordering, status, invariants, and dependency map;
   - plan: design, phases, gates, verification, and definition of done;
   - ledger: current evidence, blockers, handoffs, verification validity, and one resolved next step.

   Replace stale assumptions instead of appending release-note history. Do not add roadmap citations
   to plans. Do not change product code, packages, generated baselines, tests, or runtime configuration.
7. Re-read the changed graph as a set, search for obsolete versions/API terms, and use a current
   checkout's `plan_graph.py --root <owner-worktree>` against every changed owner. Make path-scoped docs
   commits per owning branch. Route any clean
   standalone roadmap/unowned-plan branch through `docs-review` and `merge-docs`; leave live-plan
   commits on their owner branches unless Tommy explicitly requested a push.
8. Report the source evidence, changed artifacts, justified `none` classifications, validation, and
   commits. Follow `PROMPTS.md` for each explicitly targeted actionable or blocked ledger.
