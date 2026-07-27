# CI/CD pipeline re-architecture

Rip-and-replace of the Concertable delivery pipeline. The current setup — GitHub Actions + GitHub
merge queue + a pile of self-heal workflows — is symptom-patched, not designed. This plan audits it
completely (Step 1), decides and justifies a target state (Step 2), and lands it as an incremental,
reversible, phase-by-phase migration where **merges keep working throughout** (Step 3).

Supersedes `plans/PIPELINE_PROBLEMS.md` (the 8-problem seed), which this folds in and extends.

Non-negotiable invariants preserved end to end:
- **The carve / standalone-build guarantee** — every service builds from its own folder + the feed.
- **Platform-sync correctness** — the publish→pin-bump→consumer-migrate loop keeps working.

---

# Step 1 — Problem inventory (complete audit)

Audited: all 8 workflows (`test.yml`, `auto-merge.yml`, `platform-sync.yml`, `platform-sync-alert.yml`,
`publish-packages.yml`, `mirror.yml`, `mirror-parity.yml`, `claude-review.yml`), the ruleset
`17393335`, `bump-platform-version.sh`, and live run history.

## Corrections to the seed (`PIPELINE_PROBLEMS.md`)

- **#7 is misdiagnosed.** Ruleset `17393335` ("Main merge queue (e2e gate)") is **repo-level**, not
  org-level: `source_type: "Repository"`, `source: "Concertable/concertable"`, readable and writable at
  `repos/Concertable/concertable/rulesets/17393335`. The real reason admin merge is refused is
  `bypass_actors: []` + `current_user_can_bypass: "never"` — the ruleset declares **no bypass actor at
  all**. That is the bootstrap deadlock, and it is a one-field fix (add a break-glass actor), not an
  org-admin escalation. This reframes the whole fix for #7.
- **#1 is truly fixed** (confirmed live): queue branches flipped from `gh-readonly-queue/master/...`
  (≤ pr-220) to `gh-readonly-queue/main/...` (pr-222+). The `[skip-tests]` rename commits landed.

## The root cause under all of #2–#5

**One required-check surface that behaves differently on `pull_request` vs `merge_group`, and can
resolve to `skipped`.** The queue's required checks (`build`, `carve-*×5`, `e2e-api-tests`,
`e2e-ui-tests`, `ci-gate`) are wired so that:
- On the PR, e2e **no-ops to green in ~3s**; in the queue it **runs for real (~27 min)**. So a PR is
  "all green" and admitted, and only *then* can e2e fail. (seed #2)
- Monitors reading `gh pr checks`/`mergeStateStatus` see the **PR rollup** (green no-ops) + queue
  *state*, never the `merge_group` run — so "queued/green" is reported while the queue run is failing.
  (seed #5)
- A required check that ends **`skipped`** stalls queue admission until `check_response_timeout` (60
  min). The classifier goes to great lengths (no-op-to-green instead of skip) to avoid this — the
  complexity is a workaround for GitHub's admission model, not a feature. (seed #3)
- Post-failure ejection leaves the PR `OPEN`/`CLEAN`/not-queued — **identical** to the #3 stall — so a
  blind self-heal re-queues a genuinely-failing PR and re-burns 27 min in a loop. (seed #4)

Every one of these is downstream of: *required checks are not authoritative on the PR, and can be
`skipped`.* Fix that one thing and #2–#5 collapse together.

## New findings (beyond the seed)

- **N1 — The band-aid pile, quantified.** **114 commits** touch `.github/`. Recent history is a
  self-heal graveyard: `kill the queue-admission stall`, `stop auto-merge re-adding queue-failed PRs
  (the jam loop)`, `detect and recover merge-queue entries that never dispatch`, `make the auto-merge
  re-assert actually fire`. `auto-merge.yml`'s own header concedes it is "two bolted-on heuristics
  deep… real tech debt, not a design," and predicts a *third* failure mode. This is the artefact of
  patching-from-outside a system whose core signal (required checks) is unreliable.
- **N2 — The flake is live right now.** PR #224 (`feat(dataaccess)… ExecuteAsync`) is `OPEN`/`CLEAN`
  but **failed the `merge_group` run twice today** (16:15, 17:30 UTC 2026-07-27) on the exact SHA
  `740226bd` that `main` currently sits at — the flaky Customer UI search-results scenario (seed #6),
  gating a real PR as this plan is written. It carries no `[skip-e2e]` label. The flake is not
  hypothetical; it is the current blocker.
- **N3 — Every merge pays a heavy, partly-wasted tax.** An `api/**` push to main triggers: (a)
  `publish-packages` republishing the whole `IsPackable` closure with a fresh MinVer version *on every
  commit* (lockstep), (b) `platform-sync` opening a pin-bump PR that is itself an `api/**` change
  (cascade-guarded, but still a full queue cycle with its own build+carve), and (c) `mirror.yml` doing
  a **`git subtree split` (full-history rewrite) of six subtrees** every push. Most of this fires even
  when no published *contract* changed.
- **N4 — Inconsistent "extract a service folder" technique.** Carve gates use fast `git archive`
  (tree at HEAD, no history); mirror uses slow `git subtree split` (rewrites full history) for the same
  conceptual operation. The carve technique is strictly better for anything that doesn't need history.
- **N5 — Merge method is `MERGE`** (`merge_method: "MERGE"`), i.e. main accrues merge commits. This
  contradicts the squash-merge-ghost assumptions baked into the repo's own branch-cleanup skills
  (`unmerged`, `prune-worktrees`) and inflates history. A decision to make deliberately, not by default.
- **N6 — unit/integration are not required checks** and are **skipped entirely in `merge_group`**
  (they run on the PR and on the post-merge push, aggregated by `ci-gate`). So the queue's *only*
  behavioural signal is e2e — and e2e is the flaky one. There is no fast deterministic behavioural gate
  in the queue; it is all-or-nothing on the slowest, least-stable suite.
- **N7 — Mirroring is speculative work run on the hot path.** The split-repo future is not active
  (no service has split out). Mirroring every `main` push (6× full-history rewrite + force-push +
  verify) is per-merge cost for a capability nobody consumes yet, and `mirror-parity.yml` already
  re-checks nightly. This belongs off the hot path.
- **N8 — Two disjoint IaC/config surfaces.** GitHub protection is clicked-together (the ruleset lives
  only in GitHub's UI/state); a *separate sibling `config` repo* already does azurerm Terraform (App
  Config + Key Vault, remote state, `validate` green, apply-blocked on creds). Nothing ties repo
  protection, CI infra, and cloud infra into one reviewable IaC surface.
- **N9 — `[skip-e2e]` is human-intuition gating.** The `create-pr` skill decides the tier "by
  intuition"; #224 needed the token retroactively. Tier selection is a guess made per-PR by a human,
  not derived deterministically — the exact opposite of what a gate should be.

## Inventory summary (what each workflow is, and its verdict)

| Workflow | Purpose | Verdict |
|---|---|---|
| `test.yml` (CI) | Classify diff → tiered build/carve/unit/integration/e2e | **Keep the engine, re-shape the gate.** Classifier logic is sound; the required-check surface and PR-vs-queue asymmetry are the problem. |
| `auto-merge.yml` | External poller nudging stuck PRs into the queue | **Retire.** Pure compensation for unreliable admission (N1). Deletable once the gate is deterministic. |
| `platform-sync.yml` | Publish→pin-bump PR→auto-merge | **Keep, simplify.** Inherits the queue fix; its stalls disappear. |
| `platform-sync-alert.yml` | Issue+label when a sync PR goes red | **Keep** as a cheap path-independent backstop; re-assess post-migration. |
| `publish-packages.yml` | Pack `IsPackable` + verify-restore closure | **Keep, gate smarter.** Well-designed; reduce needless republish churn (N3). |
| `mirror.yml` | Subtree-split 6 services → standalone repos on every push | **Move off the hot path** (N4, N7): tag/manual-triggered, `git archive`-based. |
| `mirror-parity.yml` | Nightly drift check of the mirrors | **Keep** (already off the hot path). |
| `claude-review.yml` | Opt-in AI PR review on a label | **Keep** (independent, harmless). |

---

# Step 2 — Target-state design

## Guiding principle

**Make the signal authoritative, then delete the machinery that exists to compensate for it not being
authoritative.** Almost every band-aid is downstream of "required checks aren't trustworthy on the PR
and can be `skipped`." So the design is: one deterministic, always-reporting required check; the PR
runs the real gate; the merge queue re-verifies against live main; native GitHub auto-merge does the
rest — no external poller.

## D1 — CI plane: **stay on GitHub Actions.** (Not Azure DevOps — yet.)

Source, PRs, the NuGet feed (GitHub Packages), the planned image registry (GHCR), and branch
protection all live in GitHub. "Toward Azure" is about the **runtime** (ACA), not the **CI plane**.
Moving CI to Azure DevOps would split identity and artifacts across two control planes and buy nothing
the trajectory needs. **Decision: GitHub Actions.**

Migrate to ADO **only** if org policy later mandates a single Azure-DevOps control plane. To keep that
door cheap, this redesign pushes logic **out of inline YAML into scripts + composite actions**
(`.github/actions/*`, `scripts/ci/*`), so a future ADO port is a thin re-wrapping of the same scripts,
not a rewrite. Portability is a design constraint, not a migration.

## D2 — Merge queue: **keep it, but collapse the gate to one deterministic check.**

The queue's *job* (serialize merges, re-verify each candidate against the main it will actually land
on) is correct and worth keeping. The pain is the gate wired to it. Redesign:

1. **Exactly one required status check: `ci-complete`.** It `needs:` every real job (build, carve×5,
   unit, integration, e2e) and evaluates to `success`/`failure` **deterministically — never
   `skipped`** — on both `pull_request` and `merge_group`. A job that legitimately didn't need to run
   contributes `success` (a no-op pass), never a skip. This is the single change that kills the
   admission stall (#3): GitHub only stalls on a `skipped` *required* check, and there is now exactly
   one required check that never skips.
2. **The PR is authoritative.** The real gate — including e2e — runs on `pull_request`, so a green PR
   genuinely means mergeable. Kills #2 and #5: there is no longer a "green no-op on the PR, real run
   only in the queue" asymmetry, and monitors read a check that reflects reality.
3. **The queue re-runs the same aggregate against live main** — its actual purpose (catch semantic
   conflicts between concurrently-green PRs). Same `ci-complete`, same determinism.
4. **Native GitHub auto-merge, no poller.** With one always-reporting required check, GitHub's built-in
   auto-merge admits and merges without external nudging. `auto-merge.yml`'s toggle/dequeue heuristics
   (#3, #4 self-heal, N1) are **deleted**. Ejection (#4) becomes unambiguous: a failed `ci-complete` is
   a plain failed required check, not a state indistinguishable from a stall.

`grouping_strategy: ALLGREEN`, `max_entries_to_build: 5` stay. `check_response_timeout` can drop from
60 min once no required check can be `skipped`.

## D3 — Deterministic gating + flake quarantine (retire `[skip-e2e]` guesswork). Fixes #6, N6, N9.

- **Gating is by diff, not by human token.** The classifier already derives build/carve/test/e2e
  relevance from the changed-file set — that stays and becomes the *sole* source of tier selection.
  The `[skip-e2e]`/`[skip-tests]`/label overrides (N9) are removed: tier is deterministic, not guessed.
- **A `@quarantine` lane.** Known-flaky scenarios (the Customer search-results e2e) are tagged and run
  in a **separate, non-blocking** job that reports but never gates. The blocking e2e lane runs only
  scenarios on the stable baseline, so a red there is a *real* regression — actionable, not noise.
  `api/Concertable.Shared/tests/.../E2E_BASELINE.md` (today only a local-skill artifact) becomes the
  CI-enforced quarantine source of truth.
- **Fix the actual flake** (#6): stabilise the wait in the search-results scenario (wait for the search
  request / network-idle before asserting visibility; drop the bare 5s timeout). A quarantined test is
  tracked to be fixed, never carried forever.

## D4 — Infrastructure as code (GitHub protection + CI infra + Azure envs). Fixes #7, N8.

One reviewable Terraform surface, aligned with the org standard already set (`DEPLOYMENT.md`) and the
sibling `config` repo's azurerm remote-state pattern:

- **`github` provider — rulesets-as-code.** `github_repository_ruleset` imports `17393335` verbatim
  (zero behavioural change on import), then edits become PRs: required check → `ci-complete`, merge
  queue params, and a **break-glass `bypass_actors`** entry (a repo-admin / break-glass team). That
  bypass actor is the durable fix for #7 — a future CI-config deadlock is escaped by a code-declared
  bypass, not a scramble.
- **`azurerm` provider — Azure envs** (ACA environment/app/job, SQL, Service Bus, App Config, Key
  Vault, Static Web Apps) matching `DEPLOYMENT.md`'s spec'd `modules/` + `envs/{pr,test,prod}` layout,
  on the same remote azurerm state backend as `config`.
- **Placement decision (flag for the user):** a single `infra/` surface owning both GitHub + Azure.
  Options: `infra/` in this monorepo, a dedicated `Concertable/infra` repo, or folding into the
  existing sibling `config` repo. Recommendation below; final call is the user's (it touches `config`).

## D5 — Ephemeral per-PR environments (Aspire → ACA). Gated on Azure creds.

**Target:** on PR open/update, `terraform apply` a per-PR ACA environment (`pr-<n>` suffix), deploy the
services, run e2e against the **real** stack (real SQL/Service Bus, not emulators), tear down on PR
close + a nightly GC sweep for leaks. ACA Consumption scales to zero and supports the ephemeral
apply/destroy mode (`DEPLOYMENT.md` "Mode B", ~£1–3/mo). This maps cleanly onto the existing
per-service AppHost topology (E2E already boots per-service AppHosts with real upstreams).

**External prerequisite:** Azure subscription + service-principal creds + domain — currently the
blocker on the whole cloud story. **Therefore ephemeral-env work is sequenced last**, behind the
CI-hardening phases that need no cloud. Interim (Phases 1–2), e2e keeps running the in-runner Aspire
stack, but *on the PR* with the quarantine lane — PR-authoritative immediately, no cloud dependency.

## D6 — Monorepo publish + platform-sync (preserve exactly, trim the fat). Preserves both invariants.

- **Carve gates unchanged** — the standalone-build guarantee is the point of the whole package model;
  the `carve-*` jobs stay, folded under `ci-complete`.
- **`publish-packages` + verify-restore kept**; reduce needless churn (N3) by gating the *republish*
  on "did any `IsPackable` project's inputs actually change" rather than every `api/**` commit. Keep
  MinVer + `--skip-duplicate` semantics.
- **`platform-sync` kept**; it inherits the queue simplification (single required check, native
  auto-merge) so its own admission stalls vanish. `platform-sync-alert` stays as the backstop.
- **Mirror moved off the hot path** (N4, N7): tag/manual-triggered, `git archive`-based, not
  subtree-split-on-every-push. `mirror-parity` (nightly) already covers drift.

---

# Step 3 — Phased migration plan

Each phase is independently shippable, ends green, and is reversible. **A gate is never removed before
its replacement is proven.** CI-hardening (no Azure) comes first; cloud/ephemeral-env work is last,
behind the creds gate. Authoring artifacts (Terraform, new workflow jobs) is reversible working-tree
work; the **irreversible steps are the `terraform apply` / ruleset change / merge** — those get an
explicit go-ahead.

### Phase 0 — Rulesets-as-code + break-glass (unblocks everything). Fixes #7.
- **What:** Stand up the Terraform `github` provider; `import` ruleset `17393335` so the code is a
  byte-faithful mirror of today (zero behavioural change), then add a break-glass `bypass_actors` entry.
- **Why first:** every later ruleset edit (Phase 1's required-check change) must be a reviewable,
  revertible code change, and the deadlock in #7 must be gone before we touch protection at all.
- **Gate:** `terraform plan` shows no diff on import; after adding the bypass, `plan` shows only the
  bypass addition. Merges still work (nothing about the gate changed).
- **Reversible:** it *is* the current config; the bypass is removable.

### Phase 1 — Collapse required checks to `ci-complete` (the keystone). Fixes #3.
- **What:** Add one aggregate job that `needs:` all real jobs and reports deterministic success/failure
  (never skipped) on both events. Change the ruleset (via Phase 0's Terraform) to require **only**
  `ci-complete`.
- **Why:** kills the skipped-required-check admission stall and makes native auto-merge viable.
- **Gate:** a docs-only PR, a package-only PR, and a full-code PR each merge hands-off with no toggle
  intervention; no PR sits `CLEAN`-but-unadmitted.
- **Reversible:** revert the ruleset's required-check list.

### Phase 2 — Make the PR authoritative + quarantine lane. Fixes #2, #5, #6.
- **What:** Run the real gate (incl. e2e) on `pull_request`. Split e2e into a blocking stable lane and
  a non-blocking `@quarantine` lane; move the flaky Customer search-results scenario to quarantine and
  **fix the flake**. Remove the `[skip-e2e]`/label tier overrides (N9).
- **Why:** PR-green now equals mergeable; the queue merely re-verifies against live main.
- **Gate:** PR-green PRs merge without ever failing in the queue for a reason invisible on the PR; the
  quarantined scenario, once fixed, returns to the stable lane green N consecutive runs.
- **Reversible:** re-add the merge_group-only gating; un-quarantine.

### Phase 3 — Retire the band-aid pile. Fixes #4, N1.
- **What:** Delete `auto-merge.yml`'s poller/toggle/dequeue heuristics (replace with a minimal
  "enable native auto-merge on ready" step, or fold into the ruleset). Delete the CLAUDE.md
  monitor/until-loop guidance that compensates for the old stalls. Move `mirror.yml` off the hot path
  (N4, N7).
- **Why:** the payoff — only safe once Phases 1–2 prove native auto-merge + PR-authoritative gating are
  reliable.
- **Gate:** a full week / N merges land with zero manual queue intervention; no stall, no jam loop.
- **Reversible:** the deleted workflows are in git history; restore if a regression appears.

### Phase 4 — Modularize workflows into composite actions + scripts (portability + DRY).
- **What:** Extract the repeated `setup-dotnet` / NuGet cache / feed-auth / carve blocks into
  `.github/actions/*` and `scripts/ci/*`. Behaviour-preserving.
- **Why:** DRYs the 5 near-identical carve jobs and the e2e setup; makes a future ADO port a re-wrap
  (D1). Also trims per-merge churn (N3) via smarter publish gating.
- **Gate:** identical CI behaviour before/after (same checks, same results) on a representative PR.
- **Reversible:** pure refactor; revert the extraction.

### Phase 5 — Terraform-provisioned ephemeral per-PR ACA environments + CD. **Gated on Azure creds.** Fixes D5.
- **What:** azurerm modules for `envs/pr` (+ `test`/`prod`); apply on PR open, real e2e against real
  ACA, destroy on close + nightly GC; then the CD path (build image via `dotnet publish
  -t:PublishContainer` → GHCR → `terraform apply` → EF-migration ACA Jobs → roll revisions) per
  `DEPLOYMENT.md`. Replaces in-runner Aspire e2e for the PR gate.
- **Why last:** externally blocked (Azure subscription/creds/domain), and it depends on Phases 1–2's
  authoritative-PR model to slot the ephemeral e2e in as the blocking lane.
- **Gate:** a PR spins up its env, runs e2e green against real ACA, and tears down; no leaked
  resources after a week (GC verified).
- **Reversible:** feature-flag the ephemeral lane; fall back to in-runner Aspire e2e (Phase 2) if ACA
  provisioning is unavailable.

---

## Open decisions for the user (do not block Phase 0–1 authoring)

1. **Infra repo placement (D4):** `infra/` in this monorepo *(recommended — one PR reviews protection
   + CI + cloud together, and it carves out cleanly later)* vs a dedicated `Concertable/infra` repo vs
   folding into the existing sibling `config` repo. Touches `config`, so it's the user's call.
2. **Merge method (N5):** keep `MERGE` (merge commits) or switch the queue to `SQUASH` (aligns with the
   repo's own branch-cleanup skills, linear history). Deliberate choice, not a default.
3. **Execution go-ahead:** Phases 0–1 change *live branch protection* — outward-facing and gating every
   contributor. Confirm before the first `terraform apply` / ruleset change (authoring the Terraform +
   the `ci-complete` job is reversible and can proceed now).
