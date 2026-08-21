# Polyrepo-ready guidance corpus — progress

- Plan: `plans/docs/POLYREPO_READY_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/polyrepo-ready`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/docs_polyrepo-ready-n3-api-floor`
  (consumer, this slice). Producer worktree:
  `C:/Users/TommySeery/source/repos/agent-standards.worktrees/Docs/polyrepo-ready-n3-shared-intersection`.
- Branch: `Docs/docs_polyrepo-ready-n3-api-floor` (consumer); `Docs/polyrepo-ready-n3-shared-intersection`
  (producer). Both close after their PRs merge.
- PRs: **N3 producer agent-standards [#15](https://github.com/Concertable/agent-standards/pull/15) OPEN**;
  N3 consumer this repo — opening after producer merges + reprovision. Prior: N2 producer **#12 MERGED**,
  N2 consumer **#695 MERGED**; cross-harness producers **agent-standards #13**, **dotagents #3**,
  **react-agents #1** MERGED; consumer **#696 MERGED**.
- Dependency/package gates: **this diff DOES touch `api/**`** (deletes `api/AGENTS.md` + `api/CLAUDE.md`,
  edits several `api/**/*.md`). `publish-packages` triggers on `api/**` regardless of file type, so the
  consumer merge **will** republish (MinVer bump) and open a `chore/platform-sync-*` PR. It is
  **non-breaking** — no published type shape changed — so the sync PR auto-merges green; whoever merges the
  consumer owns it to green. This is the first polyrepo-ready slice that trips platform-sync.
- Last reconciled: 2026-08-21, N3 implemented in both worktrees; producer #15 open, consumer pending.
  **N3 is delivery-ready; next slice after it lands is N4.**

## Current state

**N3 (`api/AGENTS.md` + `api/CLAUDE.md`, 78 lines) is implemented on both sides; producer #15 is open,
consumer PR pending its merge + machine reprovision.** The finding: four of the five floor sections were
already skill-owned — microservices/roster → `microservice-boundaries` (`SERVICE_BOUNDARIES.md`), seeding →
`seeding`, migrations → `migrations`, source-generated logging → the generic `dotagents:logging`. Only
**"Shared code is the intersection, never the union"** had no canonical home (referenced by `MULTITENANCY.md`
and `HTTP_CLIENTS.md`, stated only in `api/AGENTS.md`) and no route row on the shared tier.

- **Producer (agent-standards #15):** Concertable-specific statement of the rule added to
  `standards/dotnet/structure/SERVICE_BOUNDARIES.md` (the generic principle already lives in the paired
  `dotnet-standards` `SERVICE_BOUNDARIES.md`); CANONICAL route row fires `microservice-boundaries` on
  `Concertable\.(Kernel|Contracts)/.*\.cs$` (universal shared tier only, not a service's own `*.Contracts`;
  carve-clean). No new skill → 62/62 unchanged.
- **Consumer (this repo, branch above):** same route row added to `.agents/skill-routes.json` (37 → 38);
  `api/AGENTS.md` + `api/CLAUDE.md` deleted; every inbound guidance-doc link repointed (root `AGENTS.md`
  backend bullet, `docs/INDEX.md`, `api/ARCHITECTURE.md`, the four service `AGENTS.md` inheritance lines,
  Deal/Search/Payment `ARCHITECTURE.md`, roadmap north star). Backend floor is thereafter the route table
  over the `dotnet` plugin.

**Everything from Phase 1, N1 (six families / 28 skills), and N2 is merged on both sides.** Both harnesses
are provisioned from GitHub with all five standards plugins at user scope. `auto-memory` still needs a
durable home before close-out (Codex-only utility).

## Next Steps

1. **Merge N3 producer #15** (agent-standards) via its normal green path, then **reprovision this machine**
   (`scripts/provision-agent-standards.ps1`) and start fresh sessions so the updated `SERVICE_BOUNDARIES.md`
   is installed.
2. **Open + merge the N3 consumer PR** (this branch) via `/merge-docs` (admin-merge bypasses the queue's
   E2E). **Then own the `chore/platform-sync-*` PR** the consumer merge triggers to green/merged — it is
   non-breaking (markdown-only under `api/**`), so it should auto-merge, but confirm it.
3. **Then N4** — `api/ARCHITECTURE.md` (62) + `api/docs/MICROSERVICES_ARCHITECTURE.md` (525): cross-service
   by definition, platform-wide in full, the largest single doc left; every service repo needs it day one.
   N5 (root `AGENTS.md`), N6 (`docs/` — carries the open product-narrative question for Tommy: `OVERVIEW.md`,
   `USP.md`, `DEEP_RESEARCH_PROMPT_GUIDE.md` are neither platform standard nor service-specific — surface,
   don't invent a home), then N7a follow. N7b waits on roadmap §4c; the **frontend carve seam** (§6/§4c)
   also gates the generator's `react-app` kind. N8 last, the only carved-repo evidence.

## Completed work

- **N3 — `api/AGENTS.md` re-home (this slice).** Producer #15 + consumer (this branch). Route table 37 → 38;
  the shared-is-the-intersection rule homed in `SERVICE_BOUNDARIES.md`; both floor files deleted; all inbound
  links repointed. See Current state.
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

**N3 — self-checked, no review file yet.** Producer is a roster-doc addition (the rule's generic half already
existed in `dotagents`) plus one route row proven by the committed gate test + monorepo/carve replay; consumer
is a route-row addition, two deletions, and mechanical link repointing verified by `docs_reachability.py` =
0 errors. A `/docs-review` over both halves can run before merge as prior slices did.

## Decisions, discoveries, blockers, and deviations

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
Read @plans/AGENTS.md, @plans/docs/POLYREPO_READY_PLAN.md, and @plans/docs/POLYREPO_READY_PROGRESS.md. Merge N3 producer agent-standards #15, reprovision, open + merge the N3 consumer PR via /merge-docs and own its platform-sync PR to green, then begin N4 in a fresh plan-managed worktree from origin/main.
```
