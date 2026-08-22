# Polyrepo-ready guidance corpus — progress

- Plan: `plans/docs/POLYREPO_READY_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/polyrepo-ready`
- Worktree: none after this close-out merges. (This checkpoint runs from
  `Concertable/.worktrees/Docs-n4-closeout`; the N4 delivery worktrees are closed.)
- Branch: `Docs/docs_polyrepo-ready-n4-closeout`; none after this close-out merges — create a fresh
  plan-managed branch from `origin/main` for N5.
- PRs: **N4 all MERGED** — producer agent-standards [#18](https://github.com/Concertable/agent-standards/pull/18)
  (microservices architecture record + host-composition validation) + [#19](https://github.com/Concertable/agent-standards/pull/19)
  (repoint AppHost route note); consumer [#715](https://github.com/Concertable/concertable/pull/715) (docs
  delete + link repoint, merge-queue `skip-e2e`) + [#713](https://github.com/Concertable/concertable/pull/713)
  (non-doc citation repoint → `packages` skill). Publishes succeeded; final platform-sync
  `chore/platform-sync-0.1.0-alpha.0.1128` [#724](https://github.com/Concertable/concertable/pull/724) MERGED
  green, non-breaking. Prior: N3 **#15 + #698 + #700**, N2 **#12 + #695**, cross-harness all MERGED.
- Dependency/package gates: none open. N4 tripped platform-sync (deleting/editing `api/**/*.md` and editing
  `api/**` csproj/targets comments republished via the coarse `api/**` filter); the sync cascade
  (`.1125`→…→`.1128`) was **non-breaking** throughout (no published type shape changed) — intermediate syncs
  superseded/auto-merged, the terminal #724 merged green. No red sync, no `platform-sync-broken` issue.
- Last reconciled: 2026-08-22 — **N4 terminal.** All four N4 PRs merged, final sync #724 green, delivery
  worktrees closed. **N5 is the next slice.**

## Current state

**N4 (`api/ARCHITECTURE.md` + `api/docs/MICROSERVICES_ARCHITECTURE.md`) is complete and merged — this
checkpoint closes it out.** Both docs are cross-service by definition (the carve, the publish→sync loop,
Contracts-only deps), so both re-homed platform-wide; nothing stayed behind.

- **Producer (agent-standards #18 + #19):** the microservices architecture record homed platform-wide (its
  design rationale/decision history now the `microservices-architecture` skill; the boundary rule the
  `microservice-boundaries` skill), plus host-composition validation. #19 repointed the AppHost route note off
  the deleted `api/ARCHITECTURE.md`. Both MERGED.
- **Consumer (this repo) — split by branch type, the two-lane pattern:**
  - `Docs/docs_polyrepo-ready-n4-architecture` (#715) deleted `api/ARCHITECTURE.md` + `api/docs/MICROSERVICES_ARCHITECTURE.md`
    and repointed every guidance-doc link (root `AGENTS.md` north star, `docs/INDEX.md`, `docs/OVERVIEW.md`,
    the service `ARCHITECTURE.md`/`AGENTS.md`, `plans/AGENTS.md`, `.agents/skill-routes.json`). Landed through
    the merge queue with `skip-e2e`.
  - `Chore/polyrepo-n4-arch-ref-repoint` (#713) repointed the non-doc citations — five service
    `Directory.Build.props`/`.targets`, five `*.Hosting.csproj`, `.github/workflows/claude-review.yml` — to the
    `packages` skill (the correct owner of the per-folder-build-closure rule those comments cite). Comment/text
    only; landed via the queue with `skip-e2e`.

**Everything from Phase 1, N1 (six families / 28 skills), N2, and N3 is merged on both sides.** Both harnesses
are provisioned from GitHub with all five standards plugins at user scope. `auto-memory` still needs a
durable home before close-out (Codex-only utility).

## Next Steps

1. **Begin N5 in a fresh plan-managed worktree from `origin/main`** — root `AGENTS.md` (147): split by the
   same test. Monorepo-only lines (that this *is* a monorepo, where `api/`/`app/` sit) die with the root;
   everything else (scalable-fix rule, autonomy rules, merge/platform-sync invariants, doc locality, review
   gates) is platform-wide. Then N6 (`docs/` — carries the open product-narrative question for Tommy:
   `OVERVIEW.md`, `USP.md`, `DEEP_RESEARCH_PROMPT_GUIDE.md` are neither platform standard nor service-specific
   — surface, don't invent a home), then N7a. N7b waits on roadmap §4c; the **frontend carve seam** (§6/§4c)
   also gates the generator's `react-app` kind. N8 last, the only carved-repo evidence.
2. **Ledger deviation to carry forward:** the N4 consumer PRs (#715/#713) were merged in a prior session
   **without a progress-ledger update in them**, so `worktrees.ps1 close -PlanManaged` refused (no ledger to
   anchor). This close-out re-establishes the anchor. For N5, keep the ledger checkpoint *on the delivery
   branch* so the plan-managed close works normally.

## Completed work

- **N4 — `api/ARCHITECTURE.md` + `api/docs/MICROSERVICES_ARCHITECTURE.md` re-home. Terminal.** Producer
  agent-standards #18 (architecture record + host-composition validation) + #19 (AppHost route-note repoint);
  consumer #715 (both docs deleted, all guidance-doc links repointed) + #713 (non-doc csproj/targets/yml
  citations → `packages` skill). Both consumer PRs landed via the merge queue `skip-e2e`; #713's currency merge
  from `main` restamped its review (incremental, no new findings). Publish cascade `.1125`→`.1128` all
  non-breaking; terminal sync #724 (`.1128`) MERGED green. Both spent review files deleted in this close-out.
- **N3 — `api/AGENTS.md` re-home. Terminal.** Producer #15 + consumer #698 + code/CI follow-up #700 (ACC1
  `.yml` bot-prompt read-list, ACC3 `.cs` comment citation). Route table 37 → 38; the shared-is-the-intersection
  rule homed in `SERVICE_BOUNDARIES.md`; both floor files deleted; all inbound links (docs, `.yml`, `.cs`)
  repointed. Platform-sync #699/#705 merged, #706 auto-merging (all non-breaking). Machine reprovisioned.
- **N2 — route-table convention.** Producer #12: `standards/process/SKILL_ROUTES.md` (skill `skill-routes`)
  + carve-time generator `.agents/gen_skill_routes.py` + gate test; `--kind monorepo` reproduced the live
  37-row table exactly, `dotnet-service` carve clean, `react-app` refused. Consumer #695: `_comment`
  repointed, routes unchanged.
- **Cross-harness prerequisite.** agent-standards #13 (Codex manifests for all three plugins, one-command
  Claude/Codex provisioner, active-harness fail-closed router + all-route verifier), dotagents #3,
  react-agents #1, consumer #696. Both harnesses resolve all live routed skills independently.
- **N1 — six families / 28 skills, merged both repos.** review (#675 + #6, seven `review/` docs),
  merge/PR (#676 + #7, four `merge/` docs, `create-gh-pr` → `open-pr`), test-debug (#677 + #8, six `testing/`
  docs, `docker-health.ps1` vendored), git (#679 + #9, six `git/` docs, `sync`→`sync-checkout`,
  `worktree`→`open-worktree`), plan-workflow (#687 + #10, four docs + new `plan-checkpoint`), package-cutover
  (#693 + #11, `dotnet:package-cutover`).
- **Phase 1** (#669 + #5): plan method into `PLANS.md`; `HANDOFF.md` new.

## Verification — N4

Consumer (this repo, close-out):
- Both delivery PRs MERGED; `main` at `1c63e4f6b` (incl. #724). `git grep` on `main` shows no remaining
  guidance/code citation of the deleted `api/ARCHITECTURE.md` or `api/docs/MICROSERVICES_ARCHITECTURE.md`
  outside historical records in spent ledgers/reviews.
- #715 landed `skip-e2e` (64 pass / 5 skipping); #713 landed `skip-e2e` (64 pass / 5 skipping) after a clean
  `origin/main` currency merge and an incremental review re-stamp (`fe38af16..231dbd41`, no new findings — the
  three-dot diff confirmed HEAD carried only the already-reviewed repoints).
- Platform-sync owned to green: publishes for both merges succeeded; the cascade collapsed to a single terminal
  sync #724 (`.1128`), MERGED non-breaking. No open sync PR, no `platform-sync-broken` issue.
- `plan_graph.py` + `docs_reachability.py` run clean in this close-out worktree (see below).

## Verification — N3

Producer (agent-standards #15):
- `sync-generated.ps1 -Check`: **196 files current (62 skills / 62 docs)** — no new skill, `SERVICE_BOUNDARIES.md`
  plugin copy regenerated.
- Hook suite **177/177** (incl. `test_gen_skill_routes.py`: new row valid regex, carries skills, carve-clean).
- `build_routes("monorepo")` → **38 rows**; `Concertable.Kernel/ICurrentUser.cs` and universal
  `Concertable.Contracts/Genre.cs` resolve `microservice-boundaries`; per-service `Concertable.Auth.Contracts`
  correctly **excluded**; `dotnet-service` carve shows **no `^api/`/`app/` leak**.

Consumer (this repo):
- `.agents/skill-routes.json` parses, 38 routes; `skill_router.py --skills-for` resolves
  `microservice-boundaries` on a `Concertable.Kernel/**` path (the moved rule's representative path) and the
  prior families still resolve (seeding/migrations/logging/persistence).
- `docs_reachability.py`: 0 errors (api/AGENTS.md's inbound guidance-doc links all repointed);
  `plan_graph.py`: 0 errors. Vendored-hook check clean.
- **Not meta-only w.r.t. publish** — the diff deletes/edits `api/**/*.md`, so the merge republishes and opens
  a non-breaking `chore/platform-sync-*` PR (see gates). The queue path is still `/merge-docs` (admin-merge
  skips E2E); platform-sync fires post-merge and is owned to green.

## Reviews

**N4 — all findings addressed and merged; both review files deleted in this close-out.** #715 was docs-reviewed
(1 finding, ACC1: dead non-doc citations of the deleted doc — fixed by splitting them onto #713, the sanctioned
branch-ownership move since they were code-adjacent, not docs). #713 reviewed clean (comment/text-only,
Layer-1/security/all-lenses clear); its post-merge-currency re-stamp was an incremental review over the clean
`origin/main` merge with no new findings. `reviews/Docs-docs_polyrepo-ready-n4-architecture.md` and
`reviews/Chore-polyrepo-n4-arch-ref-repoint.md` are `git rm`'d here per the review lifecycle.

**N3 — all findings addressed and merged; both review files deleted in this close-out.** The consumer branch
was docs-reviewed by an independent agent (3 Lens-A dead-reference findings, all created by the deletion);
#700 had a self-review (2 comment/prompt-text lines, no findings) with a security marker (touched a workflow).
- **ACC2 (MED, docs)** — `api/Concertable.Search/ARCHITECTURE.md:60` still cited `(api/AGENTS.md)` — fixed in
  #698 (repointed to `microservice-boundaries`).
- **ACC1 (HIGH, `.yml`)** + **ACC3 (LOW, `.cs`)** — `claude-review.yml` bot read-list and a
  `StripeAccountController.cs` comment cited the deleted floor docs; out of #698's meta scope, fixed in #700.
- Reviewer verified clean: reachability 0 errors, route JSON 38 rows, the shared-tier regex matches the real
  universal tier and over-matches no per-service `*.Contracts`, the north-star quote matches root
  `ARCHITECTURE.md` verbatim, no sibling still asserts an `api/`-level floor.

Producer #15 self-checked: roster-doc addition (generic half already in `dotagents`) + one route row proven by
the committed gate test and monorepo/carve replay.

## Decisions, discoveries, blockers, and deviations

- **N4 split by branch type, not by node (two lanes).** The doc deletions + link repoints rode a `Docs/*`
  branch (#715); the code-adjacent citations (csproj/targets/yml comments) rode a `Chore/*` branch (#713),
  because a `.csproj`/`.cs`/`.yml` edit is not meta-only and must not admin-merge. Both landed through the
  merge queue `skip-e2e` (comment/text-only → no E2E trigger).
- **N4 deviation — ledger not in the delivery PRs.** #715/#713 were merged (prior session) without a
  progress-ledger checkpoint, so plan-managed worktree close refused; they were closed as ordinary merged
  worktrees and this `Docs/*` close-out re-establishes the `main` anchor. Carry forward: checkpoint the ledger
  on the delivery branch for N5.
- **Currency merge invalidates the review stamp.** Merging `origin/main` into #713 for merge-queue currency
  moved HEAD past its review marker; `merge_review_gate.py` blocked the merge until an incremental review
  re-stamped it. The three-dot diff proved the merge added no branch-authored content, so it was a clean
  re-stamp — but the gate is real: update-for-currency then re-run `/incremental-review`.
- **N3 added a route row rather than relying on the always-loaded floor (the scalable call).** The
  shared-is-the-intersection rule's violation (an audience-specific member on `Kernel`/`Contracts`) is silent
  and expensive, and no route fired on the shared tier — only `AppHost/Program.cs` fired
  `microservice-boundaries`. Deleting the prose floor without a route row would make the rule stop resolving
  at its violation site (acceptance criterion 2 failure). The new row (universal shared tier only, carve-safe)
  preserves the trigger; the live table and the generator's CANONICAL were updated together to stay faithful.
- **N3 trips platform-sync; earlier slices did not.** `publish-packages` keys on `api/**` for any file type,
  so deleting/editing `api/**/*.md` republishes. Non-breaking → the sync PR auto-merges; still must be owned
  to green by whoever merges the consumer.
- **Generator, not template (Tommy confirmed).** The table is generated once at carve time and committed; the
  generator (`.agents/gen_skill_routes.py`) carries the canonical rows once and parameterises only the floor
  anchor by `--kind`. No per-repo values file — resolve per-repo values at run time.
- **`react-app` generation still blocked** on the frontend carve seam (roadmap §6/§4c): the react rows carry
  `app/` mid-pattern. Folded into N8's dependencies.
- **`auto-memory`** stays in-repo; criterion 1 still requires a durable home for this Codex-only utility
  before close-out.
- **Durable cross-slice rules that still bind N4–N8:**
  - **Collision-check a new skill name across *every* repo on the machine** (the family-2/3/4 lesson). N3
    added no new skill, so no collision check was needed.
  - **No values file — resolve per-repo values at run time.**
  - **Commit+push the irreversible core of a slice before the longer ledger prose** — concurrent sessions
    prune worktrees here.
  - **A meta-only consumer ADMIN-MERGES via `/merge-docs`, never `--auto`** — the queue runs E2E even on a
    meta diff inside `merge_group`.
  - **Cross-harness completeness is a per-slice gate** — run the provisioner's repository verification for
    Claude and Codex whenever route ownership changes. (N3 added a row for an existing skill, so the unique
    skill set is unchanged; re-verify after reprovision anyway.)

## Resume prompt

```
cd C:/Users/TommySeery/source/repos/Concertable
Read @plans/AGENTS.md, @plans/docs/POLYREPO_READY_PLAN.md, and @plans/docs/POLYREPO_READY_PROGRESS.md. N4 is terminal. Create a fresh plan-managed worktree from origin/main for N5 (root AGENTS.md → agent-standards, platform-wide minus the monorepo-only lines), then do what the ledger's `## Next Steps` says.
```
