# Polyrepo-ready guidance corpus — progress

- Plan: `plans/docs/POLYREPO_READY_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/polyrepo-ready`
- Worktree: `Concertable.worktrees/Docs/docs_polyrepo-ready-n7a-plans-agents` — the N7a delivery worktree,
  off `origin/main` (`1452b5b8b`, incl. N5+N6). N5/N6 worktrees closed.
- Branch: `Docs/docs_polyrepo-ready-n7a-plans-agents` (this branch); `Docs/*` (durable guidance, meta-only).
- PRs: **N5 MERGED** — consumer [#745](https://github.com/Concertable/concertable/pull/745) (root `AGENTS.md`
  149 → 23 lines), producer agent-standards [#25](https://github.com/Concertable/agent-standards/pull/25) +
  [#27](https://github.com/Concertable/agent-standards/pull/27). **N6 MERGED** —
  [#750](https://github.com/Concertable/concertable/pull/750) (product narrative → new
  **[`Concertable/docs`](https://github.com/Concertable/docs)** repo). **N7a IN PROGRESS = this branch** —
  thin `plans/AGENTS.md` to per-repo values + pointers. Prior: N4 (#18/#19 + #715/#713), N3 (#15 + #698 +
  #700), N2 (#12 + #695), cross-harness all MERGED.
- Dependency/package gates: none open. N7a is **meta-only** — edits `plans/AGENTS.md` + `plans/docs/*`; no
  `api/**`, so no publish / `chore/platform-sync-*`.
- Last reconciled: 2026-08-22 — **N5+N6 merged.** **N7a in progress:** `plans/AGENTS.md` thinned 68 → 31
  lines (per-repo layout/hook/script/suite values + pointers to the owning skills); an independent mapping
  confirmed every platform rule in it was already homed by Phase 1 + N1, so N7a is consumer-only. Two small
  agent-standards `PLANS.md` follow-ups remain (see Next Steps), non-blocking. Reachability/plan-graph re-run
  clean on-branch.

## Current state

**N7a (`plans/AGENTS.md`) is in progress on this branch — a consumer-only thin.** An independent rule-by-rule
mapping of `plans/AGENTS.md` against the installed agent-standards process corpus found **every platform-wide
rule already homed** by Phase 1 + N1 (`PLANS.md`, `HANDOFF.md`, `plan/CHECKPOINT.md`, `git/WORKTREE.md`,
`FAILING_TESTS.md`, `REMOTE_VALIDATION.md`, and the `migrations`/`packages` skills). So there is no rule to
move out; the file thins to this repo's own **values** (the `plans/<epic>/` layout + `LAUNCH_ROADMAP.md`, the
hook paths `.agents/hooks/plan_graph.py`/`plan_handoff_stop.py`, `scripts/worktrees.ps1`,
`./initial-migrations.ps1`, the E2E suite names) plus pointers to the owning skills — the N5 root-`AGENTS.md`
pattern. **68 → 31 lines.**

- **The bare-stem naming convention stays** as part of what the plan explicitly designates this repo's own
  "folder layout" (the corpus has it only as an assumed category, not a stated rule — a `PLANS.md` follow-up,
  below).
- **No producer PR this slice.** agent-standards already has an **open** PLANS.md PR (#20, consumption
  contract); opening a third concurrent PLANS.md PR for two tiny additions would only churn. Both follow-ups
  are non-blocking and tracked in Next Steps for the epic close-out.

**Everything from Phase 1, N1–N6 is merged on both sides.** Both harnesses provisioned from GitHub, all five
standards plugins at user scope (agent-standards `main` now at `501332a`, #28 — ahead of the last reprovision
`13fcef1c0`). `auto-memory` still needs a durable home before close-out (Codex-only utility).

## Next Steps

1. **Finish N7a on this branch** — `/docs-review`, then `/merge-docs` (meta-only: `plans/AGENTS.md` +
   `plans/docs/*`, no `api/**`). Ledger is checkpointed on the delivery branch so `worktrees.ps1 close
   -PlanManaged` anchors normally.
2. **Two non-blocking agent-standards `PLANS.md` follow-ups, before epic close-out** — best folded into the
   in-flight PLANS.md work (#20) or one consolidated PR after it lands, never a third concurrent one:
   (a) the N5-logged "opening/naming a `plans/*.md` obliges reading it before acting" obligation (missing as a
   general rule — only `RESUME.md`/`CHECKPOINT.md` state it workflow-scoped); (b) promote the
   standing-reference **bare-stem** naming from an assumed category to a stated one-line convention. Also sweep
   the spent `Docs-docs_polyrepo-ready-*` review files (all from merged PRs) at close-out.
3. **N7b** — relocating the plan *documents* — waits on roadmap §4c; the **frontend carve seam** (§6/§4c)
   also gates the generator's `react-app` kind.
4. **N8 last** — the only carved-repo evidence. Endpoint: **root `AGENTS.md`/`CLAUDE.md` are deleted at the
   cut, not within this plan** — this plan makes root hold zero shared canon; the cut (separate roadmap)
   dissolves the monorepo root.

## Completed work

- **N7a (in progress on this branch) — `plans/AGENTS.md` thinned to per-repo values.** 68 → 31 lines: kept
  this repo's plan layout, hook/script paths (`plan_graph.py`, `plan_handoff_stop.py`, `worktrees.ps1`,
  `initial-migrations.ps1`) and E2E suite names as values; replaced every platform rule with a pointer to its
  owning skill (`plans`, `handoff`, `plan-checkpoint`, `open-worktree`, `migrations`, `merge`,
  `failing-tests`, `packages`). Consumer-only — the mapping found no unhomed rule. Two `PLANS.md` follow-ups
  deferred (Next Steps 2).
- **N6 — `docs/` product narrative relocated. MERGED (#750).** Created private `Concertable/docs`, moved
  `OVERVIEW`/`USP`/`DEEP_RESEARCH_PROMPT_GUIDE` there (README index, links absolutised), pushed. Deleted them
  from this repo; repointed root `AGENTS.md` + `INDEX.md` Product rows. `INDEX` kept as this-repo index
  (process rows fixed in N5, product rows → docs repo); `REMOTE_VALIDATION.md` kept as a per-repo doc
  (deviation from "fold into root"). Root stays the minimal anchor — full deletion is cut-time. Docs-reviewed
  (one LOW, fixed on-branch); admin-merged meta-only, no publish.
- **N5 — root `AGENTS.md` thinned 149 → 23 lines. MERGED (#745).** Every rule re-homed to the plugins;
  root holds only per-repo values/pointers. Docs-review found one HIGH (CON1: `INDEX.md` rows pointing at
  deleted root sections), fixed on-branch. Admin-merged meta-only; no publish fired. Producer:
- **N5 PRODUCER — root `AGENTS.md`'s platform-wide rules, floor + gate. MERGED (agent-standards, both into
  `main` at `13fcef1c0`).** #25 re-homed the three behavioral sections to `FLOOR.md`, injected by a new
  `session_floor.py` SessionStart hook and owned by a `floor` skill, and homed §7's ready-for-review invariant
  in `MERGING.md`; #27 made `skill_router` fail CLOSED (un-routed write blocked when the plugin didn't load).
  The remaining destinations were already present from earlier slices (`MERGING.md`, `BRANCHING.md`,
  `git/WORKTREE.md`, `REMOTE_VALIDATION.md`, `PLANS.md`, `DOCS_AND_DEBT.md`). Machine reprovisioned to
  `13fcef1c0`, 5/5 plugins, smoke-verified. **Consumer (thin root `AGENTS.md`) on this branch.**
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

## Verification — N7a

Consumer (this repo, `Docs/docs_polyrepo-ready-n7a-plans-agents`):
- `plans/AGENTS.md` thinned **68 → 31 lines**. An independent rule-by-rule mapping against the installed
  agent-standards corpus confirmed every platform rule is already homed: opening/method → `PLANS.md` +
  `HANDOFF.md`; folder/naming/worktree/template → `PLANS.md` + `plan/CHECKPOINT.md` + `git/WORKTREE.md`;
  plan-graph + handoff hooks → `PLANS.md`/`HANDOFF.md`; close/retire/close-out → `plan/CHECKPOINT.md` +
  `git/WORKTREE.md`; red suite → `FAILING_TESTS.md`; model-change migrations → the `migrations` skill (which
  already carries `initial-migrations.ps1`); merge-queue E2E → `REMOTE_VALIDATION.md` + the `merge` skill;
  breaking published contract → `PLANS.md` + `packages`. What stays is only this repo's values + pointers.
- `docs_reachability.py` (scoped): **0 errors**, 29 warnings — all pre-existing `plans/` dead-link warnings
  (warn-only), unchanged by this diff; the thinned file's one relative link (`../docs/REMOTE_VALIDATION.md`)
  resolves. `plan_graph.py`: **0 errors, 0 warnings**. `plans/CLAUDE.md` sibling intact (`@AGENTS.md`).
- **Meta-only**: touches `plans/AGENTS.md` + `plans/docs/*` — no `api/**`, so no publish / platform-sync.
  Lands via `/merge-docs` admin-merge (never `--auto`).

## Verification — N6

Docs repo (`Concertable/docs`, private):
- Seeded with `OVERVIEW.md`, `USP.md`, `DEEP_RESEARCH_PROMPT_GUIDE.md` + a `README.md` index; pushed to
  `main`. The three moved docs' repo-relative links (`../app/README.md`, `../app/AGENTS.md`, `../README.md`,
  the Deal `ARCHITECTURE.md`) rewritten as absolute `github.com/Concertable/concertable` URLs.

Consumer (this repo, `Docs/docs_polyrepo-ready-n6-docs`):
- `docs/OVERVIEW.md`, `docs/USP.md`, `docs/DEEP_RESEARCH_PROMPT_GUIDE.md` deleted; the only two in-repo
  referrers (root `AGENTS.md` line 1, `INDEX.md` Product rows) repointed at `Concertable/docs`. Broad
  non-md reference sweep: none.
- `REMOTE_VALIDATION.md` unchanged (kept as per-repo doc). Root `AGENTS.md` stays the minimal anchor.
- `docs_reachability.py` (scoped): **0 errors**, 29 warnings — all `plans/` dead-link warnings (warn-only,
  never gating), from the plan/ledger's own `@`-import and relative `AGENTS.md` / `*_PROGRESS.md` references
  resolved against repo root; none references a deleted doc. `plan_graph.py`: **0 errors**.
- **Meta-only**: `AGENTS.md`, `docs/INDEX.md`, `plans/docs/*`, three `docs/*.md` deletions — no `api/**`, no
  publish / platform-sync.

## Verification — N5

Consumer (this repo, `Docs/docs_polyrepo-ready-n5-root-agents`):
- Root `AGENTS.md` thinned **149 → 23 lines**. Every deleted section's rule confirmed present at its
  destination by an independent no-rule-lost audit of the installed plugin (`13fcef1c0`): behavioral trio →
  `FLOOR.md` (injected by `session_floor.py`); ready-for-review + merge invariants + platform-sync →
  `MERGING.md`; branch-first + durable-guidance → `BRANCHING.md`; worktree cleanup → `git/WORKTREE.md`
  (+ `plans/AGENTS.md` for plan-managed close/retire); E2E-through-script → `REMOTE_VALIDATION.md`; plans
  method → `PLANS.md`; one-rule-one-home / doc-locality / reachability → `DOCS_AND_DEBT.md`.
- KEPT in root (per `DOCS_AND_DEBT.md`'s cost-of-missing table): the worktree-identity gate's **service
  ownership** clause — a monorepo concern costly to miss silently, so it stays in the always-loaded floor
  rather than moving to a skill.
- `docs_reachability.py` (scoped to the worktree): **0 errors**, 28 warnings (all pre-existing `plans/`
  dead-link warnings, which only warn). `plan_graph.py`: **0 errors**. CLAUDE.md sibling intact
  (`@AGENTS.md`). All 10 relative links + both script paths resolve.
- **Meta-only**: touches only `AGENTS.md`, `docs/INDEX.md`, `plans/docs/*` — no `api/**`, so no publish and
  no `chore/platform-sync-*` fires. Lands via `/merge-docs` admin-merge (never `--auto`).

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

**N6 — docs-reviewed by an independent agent; one LOW finding, fixed on-branch.**
`reviews/Docs-docs_polyrepo-ready-n6-docs.md`.
- **ACC1 (LOW, Lens A)** — the N6 verification parenthetical mis-attributed the reachability warning delta
  (claimed the +1 was a historical `docs/OVERVIEW.md` mention; the hook flags no such warning). Fixed: the
  29 warnings are all `plans/` dead-link warnings from the ledger's own `@`-import/relative references.
- Lenses A (durable dead-links / reachability / external-URL shape), B, C, D, E, F all verified clean: no
  surviving dead link to the three deleted docs, no orphan, the new `Concertable/docs` URLs are well-formed,
  and no sibling still claims product docs live in `docs/`.

**N5 — docs-reviewed by an independent agent; one finding, fixed on-branch.**
`reviews/Docs-docs_polyrepo-ready-n5-root-agents.md`.
- **CON1 (HIGH, Lens B)** — `docs/INDEX.md` still routed seven topic rows to now-deleted root `AGENTS.md`
  sections, and the thinned root delegates topic lookup to `INDEX.md`, so a reader was sent to sections that
  no longer exist. Fixed in `595136d8`: six rows repointed to their new owning skills (`floor`, `merging`,
  `open-worktree`/`git-branching`); the seventh (doc-locality + CLAUDE.md-siblings) dropped as redundant
  with the existing `docs-and-debt` row + the reachability-hook row.
- Lenses A / C / D / E / F clean; rule-loss check clean (every deleted section survives at its destination);
  the retained "service ownership" keep judged correctly placed.

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

- **N7a is consumer-only; no producer PR this slice (the scalable call).** A rule-by-rule mapping proved every
  platform rule in `plans/AGENTS.md` is already homed (Phase 1 + N1), so the thin loses nothing. The two small
  `PLANS.md` gaps it surfaced (read-before-acting; bare-stem promoted to a stated rule) are non-blocking and
  overlap the in-flight PLANS.md PR (#20); homing them there or in one consolidated PR after it lands beats a
  third concurrent edit of the same file.
- **N6 product-narrative question RESOLVED — dedicated central docs repo (Tommy).** The plan's one genuinely
  open question. Product narrative fits neither the standards plugins nor a service repo, so it goes to a
  standalone `Concertable/docs` (private) — the standard polyrepo pattern for cross-cutting product/system
  narrative (Backstage is the heavyweight version; a docs repo is the pragmatic one). Private repo = free
  (markdown renders on GitHub; only a *published site* via Pages would cost on a private repo, and we don't
  need one), which is why it beats Confluence.
- **N6 deviation — `REMOTE_VALIDATION.md` kept, not folded into root.** The plan said "folds into the per-repo
  floor with its commands," but folding into root `AGENTS.md` grows the root we are emptying. A standalone
  per-repo validation doc is the right home and is what a carved service repo keeps.
- **Endpoint clarified (root `AGENTS.md` deletion).** This plan makes root hold **zero shared canon**; the
  root *file* is deleted at the **cut** (the separate `POLYREPO_ROADMAP`), not within this plan — because
  `INDEX.md` and `REMOTE_VALIDATION.md` (this-repo nav/validation) still need a root reachability anchor until
  the monorepo dissolves and each service repo takes its own. The plan table's old "N5 leaves behind: nothing"
  was corrected to say so. Creating `Concertable/docs` front-runs the cut roadmap's "repo creation" slightly;
  low-risk and logged here rather than done silently.
- **N5 producer is a floor+gate, not just a re-home (the criterion-2 upgrade).** The three behavioral sections
  are *always-loaded* rules with no route path to fire on — deleting them from a per-repo file would drop them
  entirely in a carved repo. So the producer delivers them as an injected `FLOOR.md` (SessionStart hook) rather
  than relying on a route, and #27 hardens the router to fail CLOSED when no routed skill resolves. This is the
  N3 "add a route rather than trust the always-loaded floor" lesson generalised: the *behavioral* floor gets a
  hook, the *architectural* floor gets a route.
- **N5 consumer is genuinely meta-only.** It edits only root `AGENTS.md` — no `api/**`, so unlike N3/N4 it does
  **not** trip `publish-packages`/`platform-sync`. Lands via `/merge-docs` admin-merge (never `--auto`); the
  queue would still run E2E on a meta diff inside `merge_group`, hence admin-merge.
- **Ledger-on-the-delivery-branch, applied (the N4 deviation fixed).** This checkpoint was committed on
  `Docs/docs_polyrepo-ready-n5-root-agents` *before* the `AGENTS.md` thin, so `worktrees.ps1 close -PlanManaged`
  will anchor normally at merge.
- **No-rule-lost audit — one minor producer follow-up logged, two non-losses.** An independent audit of every
  deletion against its destination found: (8) the worktree gate's "service ownership" nuance is **not lost** —
  it is a monorepo value kept in root per the cost table; (10) `-PlanManaged`-on-close + retirement-evidence-on-
  main is **not lost** — both are in `plans/AGENTS.md` (stays) and `git/WORKTREE.md`; (12) **`PLANS.md` omits**
  root's "opening a `plans/*.md` obliges reading it in the same breath". (12) is a load-on-demand plan-work rule
  the task summons (safe in the `plans` skill per the cost table), so not a consumer blocker — **producer
  follow-up: add it to `PLANS.md`** in a later agent-standards PR for completeness.
- **Review marker vs the merge gate.** `merge_review_gate.py`'s `review_only` treats the review current only
  when every commit after the marker touches `reviews/` alone. So the ledger verification was committed first,
  the review marker stamped at that commit, and the review file committed last by itself — otherwise the
  ledger commit would stale the marker.
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
Read @plans/AGENTS.md, @plans/docs/POLYREPO_READY_PLAN.md, and @plans/docs/POLYREPO_READY_PROGRESS.md. N5+N6 are merged. N7a (thin plans/AGENTS.md to per-repo values + pointers) is on branch Docs/docs_polyrepo-ready-n7a-plans-agents — consumer-only, gates clean. Do what the ledger's `## Next Steps` says (finish N7a via /docs-review + /merge-docs; the two PLANS.md follow-ups and the spent-review-file sweep are deferred to epic close-out; then N7b gated on §4c, N8 last). Endpoint: root AGENTS.md/CLAUDE.md are deleted at the cut, not in this plan.
```
