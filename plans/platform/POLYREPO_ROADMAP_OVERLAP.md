# Open question: does the polyrepo roadmap → plan split still earn its keep?

**If you were handed this file: do Prompt 1 below, now. It is read-only — answer the question, change
nothing.** Prompt 2 (the actual rewrite) waits until PR #862 and the rest of stage 3 have landed, and
is Tommy's call to trigger.

Raised 2026-08-30 mid-stage-3. Nothing here blocks stage 3. This document is disposable — delete it
once the question is answered and the outcome is recorded in whichever artifact survives.

Another Claude Code session is actively working stage 3 in this same worktree. **Do not edit any file
here.** Report your answer in the terminal.

## The observation

`REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md` reads as though it *is* the polyrepo epic, which makes
`POLYREPO_ROADMAP.md` look like a layer that earns nothing.

## What is actually true, as of 2026-08-30

The plan is **one item in §6** of the roadmap (`platform/polyrepo-cut` — "The cut itself"). The roadmap
has five other sections that are genuinely separate work:

| Roadmap section | State | Owns |
|---|---|---|
| §1 Backend decomposition & extraction | 🟠 mostly done | Phase 5 event-schema versioning outstanding; `A1`–`A7` IVT/legacy-coupling debt |
| §2 Backend carve | ✅ done | — |
| §3 Frontend full-stack carve | 🟡 in progress | `platform/polyrepo-fullstack`, `platform/b2b-package-topology` — their own plans |
| §4 Per-service doc locality | 🟠 4a+4b shipped | 4c deferred |
| §5 Mirror automation | ✅ retired 2026-08-27 | — |
| §6 End-state shape + **the cut** | 🟡 | **this plan** |

So the roadmap is strictly bigger than the plan, and the containment worry is not the real defect.

## The real defect: overlap, not containment

The plan's nine stages **re-absorb** territory the roadmap already tracks as separate items. The clearest
case: **stage 8 "extract `platform-web`" is §3's frontend carve.** The ledger has already had to reconcile
this once — it records the plan's checkpoint 5 as "~85% done **by the POLYREPO_FULLSTACK effort**" and its
stated constraint as "superseded — reality overtook the plan."

That is two artifacts owning the same work, with the plan repeatedly discovering that the roadmap's other
children already did some of it. That is the thing worth fixing.

## Prompt 1 — diagnose (read-only, run first)

```
Read plans/platform/POLYREPO_ROADMAP.md and plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md
plus its _PROGRESS ledger, and the sibling plans the roadmap spins off
(POLYREPO_FULLSTACK, B2B_PACKAGE_TOPOLOGY).

Question: does the roadmap -> plan split still earn its keep? Map every one of the
plan's nine stages onto the roadmap item that owns the same work, and list the ones
owned twice. I already know stage 8 / checkpoint 5 overlaps section 3's frontend
carve -- find the rest.

Answer first. Do not restructure anything. Tell me which of these three it is:
(a) the plan is genuinely one roadmap item and the overlaps are stale text to delete,
(b) the plan has outgrown its item and should be promoted to the roadmap itself,
(c) the roadmap has outlived its use now that its other items are done, and the plan
    should absorb what is left and the roadmap be deleted.
```

## Prompt 2 — act (only after picking a, b or c)

```
Execute option <a|b|c> from the polyrepo roadmap/plan overlap diagnosis in
plans/platform/POLYREPO_ROADMAP_OVERLAP.md.

One artifact owns each piece of work and no other restates it. Nothing still
outstanding may be lost in the rewrite -- reconcile against the ledger's nine-stage
list and the roadmap's unchecked items before deleting either, and carry the
still-open section 1 work (phase 5 event-schema versioning, the A1-A7 IVT debt)
into whatever survives.

Docs-only change. Delete plans/platform/POLYREPO_ROADMAP_OVERLAP.md as part of it.
```
