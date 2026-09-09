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
- **N10 — Queue admits a PR but never dispatches its `merge_group` run (→ ~1h timeout → eject).**
  (= seed #9, logged live.) PR #224 (`417d316f`) entered the queue `AWAITING_CHECKS` and sat **~53 min
  with no `merge_group` run ever dispatched**, then GitHub ejected it on `check_response_timeout` (60
  min). Earlier SHAs of the same PR *did* dispatch — so dispatch is **intermittent**, and the seed's
  own note fingers the likely aggravator: **dequeue/re-queue churn from `auto-merge.yml`'s poller while
  a prior run is in flight.** This is the strongest evidence yet that the external poller is
  net-negative: the machinery built to un-stick the queue is churning it into a *new* stuck state. It
  is a distinct symptom of the same root cause (unreliable admission compensated from outside), and it
  is **cured by the same fix** — deterministic single check (Phase 1) + delete the poller (Phase 3) —
  not by a fourth `if` branch. Immediate relief needs an escape hatch (break-glass, Phase 0) so a
  CLEAN PR is never held hostage by the queue for an hour.

## Inventory summary (what each workflow is, and its verdict)

| Workflow | Purpose | Verdict |
|---|---|---|
| `test.yml` (CI) | Classify diff → tiered build/carve/unit/integration/e2e | **Keep the engine, re-shape the gate.** Classifier logic is sound; the required-check surface and PR-vs-queue asymmetry are the problem. |
| `auto-merge.yml` | External poller nudging stuck PRs into the queue | **Retire.** Pure compensation for unreliable admission (N1). Deletable once the gate is deterministic. |
| `platform-sync.yml` | Publish→pin-bump PR→auto-merge | **Superseded** — deleted by `PLATFORM_RELEASE_TRAINS_PLAN.md` Phase 2. |
| `platform-sync-alert.yml` | Issue+label when a sync PR goes red | **Superseded** — deleted alongside it by that plan's Phase 2. |
| `publish-packages.yml` | Pack `IsPackable` + verify-restore closure | **Keep, gate smarter.** Well-designed; reduce needless republish churn (N3). |
| ~~`mirror.yml`~~ | ~~Subtree-split 6 services → standalone repos on every push~~ | **VOID 2026-08-27 — deleted.** The six mirror repos no longer exist; the polyrepo cut extracts with `git-filter-repo`. |
| ~~`mirror-parity.yml`~~ | ~~Nightly drift check of the mirrors~~ | **VOID 2026-08-27 — deleted** with `mirror.yml`. |
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
4. **Native GitHub auto-merge after explicit merge authorization, no poller.** With one
   always-reporting required check, the repository merge workflow enables GitHub's built-in
   auto-merge only after an explicit merge instruction. PR readiness remains review state and never
   authorizes delivery. `auto-merge.yml`'s toggle/dequeue heuristics and the later repo-wide
   `enable-auto-merge.yml` ready-event trigger are **deleted**. Ejection (#4) becomes unambiguous: a
   failed `ci-complete` is a plain failed required check, not a state indistinguishable from a stall.

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
- **`platform-sync` is superseded** by `PLATFORM_RELEASE_TRAINS_PLAN.md` Phase 2, which deletes it and
  `platform-sync-alert` rather than inheriting the queue simplification.
- **Mirror moved off the hot path** (N4, N7): tag/manual-triggered, `git archive`-based, not
  subtree-split-on-every-push. `mirror-parity` (nightly) already covers drift.

---

# Step 3 — Phased migration plan

## STATUS (live)
- ✅ **Immediate stabilization** — break-glass bypass applied to ruleset `17393335`
  (`current_user_can_bypass: always`); the `Auto-merge` poller workflow `disabled_manually`.
- ✅ **Phase 1** — `ci-complete` shipped to `main` (PR #226, merged `42e08574` via break-glass), and
  the ruleset repointed to require **only** `ci-complete`. The multi-check + skippable surface that
  stalled admission/dispatch is gone. The `auto-merge.yml` poller was retired; its later
  `enable-auto-merge.yml` replacement was also removed after PR #561 proved that coupling
  `ready_for_review` to queue admission bypassed explicit merge authorization. State applied
  imperatively (API), not yet captured in Terraform.
- ⬜ **Phase 0 (Terraform)** — remaining: author the `github` provider config that **imports the
  now-live ruleset** (bypass + `ci-complete`-only) so protection is code-managed. Doing it now imports
  the *final* desired state directly.
- ✅ **Phase 2** — PR-authoritative e2e + flake quarantine (merged: pr-227, pr-229).
  The classifier now runs the real e2e on `pull_request` (and the queue), skipping only the redundant
  post-merge push. The flaky Customer search-results scenario is tagged `@quarantine`; the blocking UI
  lane runs `Category!=quarantine` and a new **non-gating** `e2e-ui-quarantine` job (absent from
  `ci-complete`'s needs) runs `Category=quarantine` on the PR. To make the tag filterable past the
  trait-collapse tooling, `StripFeatureTraits` now preserves the `quarantine` Category. The flake fix:
  `FindPage.ApplyFiltersAsync` waits for the `/header?` search response instead of relying on the bare
  default 5s visibility timeout. The fragile `[skip-e2e]` substring tokens were removed (they misfired on
  pr-227: a commit message that merely *mentioned* `[skip-e2e]` silently disabled the PR e2e) — **but the
  E2E opt-out itself is kept, re-implemented robustly** (revising N9's "no opt-out at all" stance, which
  made every trivial refactor pay full ~27-min E2E): a git **trailer** `Skip-E2E: true` /
  `Skip-E2E-UI: true` (parsed structurally by `git --format=%(trailers:...)`, so prose can't trip it) or a
  same-named PR label. Unit/integration, build, and carve remain never-skippable for code/package changes.
  Unit and integration matrices discover every matching test project and run on both the PR and synthetic
  merge-group commit, so the required aggregate validates the candidate against live `main`.
  A further fix landed in pr-229: the quarantine lane no-ops to success — a single skipped check, even a non-required one, stalls
  GitHub's queue admission (the inert-PR N10 stall). **Correction to an earlier assumption:** Phase 1's
  non-skippable `ci-complete` did **not** fully cure N10 — native auto-merge still intermittently fails to
  admit a green PR (a GitHub *re-evaluation* glitch; pr-229 needed a one-time auto-merge re-assert to
  admit). That is handled by *detection*, not automation (see Phase 3). **Deferred (not blocking):** wiring
  `E2E_BASELINE.md` as the CI quarantine source of truth (D3) — the tag is the mechanism for now.
  **Un-quarantine** the scenario once it's green N consecutive runs.
- 🔶 **Phase 3** — de-padding in progress. The `auto-merge.yml` poller and the later repo-wide
  `enable-auto-merge.yml` ready-event trigger are retired; the dead `ci-gate` job is **deleted** (superseded by
  `ci-complete`, which the ruleset alone requires) and `mirror.yml` is **off the hot path** (manual-only;
  N7). The merge-confirm monitor (`AGENTS.md`) is now a **3-outcome classifier** — merged / CI-failed
  (name the check, STOP, never retry) / green-but-unadmitted (the re-eval glitch, surfaced as its own state
  for a one-time human nudge, never an automated poke). No retry machinery is added: a real failure is
  surfaced to debug, the glitch is surfaced to nudge, and the two are told apart by inspecting `merge_group`
  run results (not just PR state — seed #4: ejected-after-failure looks identical to never-admitted).
- ✅ **Phase 3b** — merge-queue wall clock (PR #973, merged `3095cb655`). The serial E2E chain is
  unbraided, both E2E lanes are per-service matrices, and an `api/` runtime diff can no longer opt out of
  E2E. **Measured: 35m55s → 17m37s.**
- ⬜ **Phases 3c–5** — outstanding (below).

Each phase is independently shippable, ends green, and is reversible. **A gate is never removed before
its replacement is proven.** CI-hardening (no Azure) comes first; cloud/ephemeral-env work is last,
behind the creds gate. Authoring artifacts (Terraform, new workflow jobs) is reversible working-tree
work; the **irreversible steps are the `terraform apply` / ruleset change / merge** — those get an
explicit go-ahead.

### Immediate stabilization (day 0 — stop the bleeding before the full arc)
The pain right now is that a CLEAN PR can be held hostage by the queue for ~an hour (N10) with manual
direct-merge the only escape, and the poller may be *causing* the non-dispatch. Two reversible live
actions give relief before Phase 0's Terraform is even written:
- **Add a break-glass bypass actor to the ruleset** (a repo admin), so a CLEAN PR can be admin-merged
  the instant the queue misbehaves — no more direct-merge-API scrambles, no hour-long hostage. This is
  Phase 0's payload done by hand first (`gh api PATCH repos/Concertable/concertable/rulesets/17393335`),
  to be replaced by the Terraform-managed version in Phase 0. Trivially reversible (remove the actor).
- **Disable `auto-merge.yml`** (`gh workflow disable`), removing the dequeue/re-queue churn that N10
  fingers as the non-dispatch aggravator. With the break-glass hatch in place we don't need the poller
  to un-stick anything. Reversible (`gh workflow enable`); it's deleted for good in Phase 3.

Both change live repo state, so they wait on an explicit go-ahead — but they're the fastest route to
"merges work again," ahead of the full migration.

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
- **What:** Delete `auto-merge.yml`'s poller/toggle/dequeue heuristics and every repo-wide
  ready-event auto-merge trigger. Explicit `/merge` authorization enables native auto-merge; only
  narrowly scoped generated-PR workflows such as platform-sync may enable it themselves. Delete the CLAUDE.md
  monitor/until-loop guidance that compensates for the old stalls. Move `mirror.yml` off the hot path
  (N4, N7).
- **Why:** the payoff — only safe once Phases 1–2 prove native auto-merge + PR-authoritative gating are
  reliable.
- **Gate:** a full week / N merges land with zero manual queue intervention; no stall, no jam loop.
- **Reversible:** the deleted workflows are in git history; restore if a regression appears.

### Phase 3b — Merge-queue wall clock: unbraid the serial E2E chain. Fixes N11.

**N11 (new finding) — the queue's wall clock is one long serial chain, not the job count.** Merge-queue
run `34367029757` (frontend-only diff): 14:57:21Z→15:33:16Z = **35m55s**. A green `api/` run
(`34360270574`) is the same order. The chain is `changes → local-platform-pack (2m45) → build (3m17) →
e2e-api-tests (8m50) → e2e-ui-tests (20m43)` ≈ 33m of the 36m. The carve tier is **not** the driver:
`carve-*`, `carve-fe` and `fe-boundaries` hang off `changes`, run in parallel and are done inside two
minutes — anything blaming carve duplication for the latency is measuring the wrong thing. Three of the
four edges in that chain are ordering conveniences, not data dependencies:

- **`e2e-ui-tests: needs: [build, e2e-api-tests, changes]`** — the only `needs:` in `test.yml` carrying no
  justifying comment. It dates from `e53681f1e` (2026-06-04, "E2E legs … serialized jobs"), when both
  suites shared **fixed** Stripe test customers and a concurrent run genuinely would have corrupted the
  other. That precondition is gone (settled below), so the edge buys nothing and costs 8m50 of dead time.
- **`needs: build` on both E2E lanes** — `build` compiles the slnx *as a gate*; it produces nothing either
  E2E job consumes. Both download the `local-platform` feed artifact (from `local-platform-pack`) and
  compile their own project closure from it. The edge is fail-fast ordering and costs 3m17 on the critical
  path — the same reasoning that already removed `architecture-tests` from `e2e-api-tests`' needs.
- **One job running two services' suites back to back** — 11m37 (B2B) + 4m17 (Customer) in `e2e-ui-tests`,
  5m03 + 2m40 in `e2e-api-tests`. A per-service matrix runs them concurrently, and
  `SPLIT_TIME_E2E_STRATEGY.md` wants these owned per service anyway, so per-service rows are the shape the
  polyrepo cut inherits rather than undoes.

#### The Stripe precondition, settled

The serialization edge would be load-bearing if the two suites shared mutable Stripe test-account state:
the B2B UI suite's `@ResetsStripe` hook calls `StripeHooks.DetachSeededCustomerCardsAsync`, which detaches
every card from the seeded customers. Against shared customers that corrupts a concurrently-running API
suite. It does not, and the isolation is deliberate and documented:

- `StripeCustomerResolver.CreateAsync` **creates fresh Stripe customers per fixture**, stamped with a
  per-run `runId`, and deletes them on dispose. Every `AppFixture` (B2B API, B2B UI, Customer API, Customer
  UI) constructs its own. `DetachSeededCustomerCardsAsync` resolves through that per-run map, so it can
  only ever touch its own run's customers.
- `Concertable.Payment.E2ETests.Stripe/AGENTS.md` states the rule directly: "Each E2E fixture creates
  distinct test-mode customers … never restore fixed shared customer IDs. Connect account IDs remain fixed
  because tests do not mutate them." `StripeAccountClient` bears that out — it links pre-provisioned
  Connect accounts to DB rows and never creates, updates or deletes one.
- Webhook cross-talk is already handled for the account-wide `stripe listen` stream:
  `StripeWebhookProcessor` drops any `PaymentIntent`/`SetupIntent` event whose customer is not in this
  run's map (`OwnsCustomer`), and the production `WebhookProcessor` handles no other object type.
- Provenance: per-run customers plus account-wide webhook isolation landed in `3f9d95497` (2026-08-09),
  **two months after** the serialization edge. The edge is a leftover from the shared-customer era, not a
  live guard.
- Everything else the two suites touch is per-runner: SQL is a Testcontainer, Service Bus and Blob storage
  are `RunAsEmulator()` containers, and the `local-platform` feed is a read-only artifact download.

Correction to the handoff's counter-evidence: `e2e-ui-quarantine` does **not** already prove concurrent
safety — it is `pull_request`-only while `e2e-api-tests` is `merge_group`-only, so those two never overlap.
The real proof is the per-run customer provisioning above, plus the fact that the queue already builds up
to five entries concurrently, which puts two full E2E runs on one Stripe test account today.

**Therefore: delete the edge; per-suite Stripe customer isolation is already the design.**

#### The change set

1. **Decouple `e2e-ui-tests` from `e2e-api-tests`** (−8m50). Both lanes gate independently through
   `ci-complete`; nothing is skipped, only unbraided.
2. **Drop `build` from both E2E lanes' `needs`, keeping `local-platform-pack`** (−3m17). `build` still
   gates the merge through `ci-complete`; it just stops standing in front of the longest job in the run.
   Trade-off taken deliberately: a broken compile now burns E2E runner minutes before failing. That is
   cost, and this phase optimises wall clock.
3. **Split each E2E lane into a per-service matrix** (`B2B`, `Customer`), `fail-fast: false` so one
   service's red never cancels the other's verdict (−4m17 of Customer UI and −2m40 of Customer API off
   their lanes' critical paths).
4. **Force the E2E tier on for an `api/` runtime diff** — below; a hardening, not a speed-up.

Projected critical path: `changes → local-platform-pack (2m45) → e2e-ui-tests (B2B) (≈4m setup + 11m37)`
≈ **18m30 against today's ≈36m**. `build`, the carves, unit/integration/startup, `e2e-api-tests` and the
Customer UI row all finish inside that window.

- **Gate: met.** Run `34387954665` (PR #973's own queue entry, labelled `full-e2e` so both suites ran in
  full rather than waiting for an unrelated `api/` diff to prove the restructure): 18:16:03Z→18:33:40Z =
  **17m37s green**, against the 35m55s baseline — a 51% cut. Every edge behaved as designed:
  `local-platform-pack` 3m01, then `build` (3m04), `container-images`, and all four E2E rows starting
  together at 18:19:21-22 — `e2e-api-tests` B2B 6m07 / Customer 5m03, `e2e-ui-tests` Customer 8m47,
  B2B 14m11. `ci-complete` remained the single required check over every lane.
- **What this hands Phase 3c:** the B2B UI row at 14m11 is now the entire critical path — 3m01 of pack
  plus that row is 17m12 of the 17m37 — so sharding it is the only remaining lever of size.
- **Reversible:** restore the two `needs:` entries and collapse the matrices; nothing outside `test.yml`
  and its policy test changes.

#### Phase 3c — Shard the B2B UI suite (the next ≈−5m, after 3b is measured)

32 scenarios / 11m37 in one row is then the whole critical path. Sharding it needs a partition that cannot
silently drop a scenario, and `StripFeatureTraits` strips the `FeatureTitle` trait, so the filter dimension
has to be `FullyQualifiedName` (one generated class per feature file, never stripped). Design: enumerate
classes with `dotnet test --no-build --list-tests`, greedily bin-pack **whole classes** by scenario count
into N shards, and fail the job if a shard's class list is empty or the union of shards is not the full
list. Whole classes, never scenarios split within a feature, so a feature-internal ordering assumption
cannot be broken by a shard boundary. **Precondition to prove first:** the suite's stated
scenario-independence convention actually holds — every scenario reaching its start state through a seeded
fast-forward `Given` rather than through the residue of the previous one. Deliberately sequenced after 3b,
which is an edge deletion with no way to change a suite's verdict; this one can.

#### Phase 3d — Stop re-running queue jobs whose inputs are byte-identical to the PR head (≈−6m)

The queue legitimately re-tests the merge commit. But when `main` has only moved by paths a job does not
consume, that job's inputs on the merge commit are byte-identical to the PR head's, which the PR run
already proved. `changes` computes both diffs already and could compare the two trees per job scope.
**E2E is explicitly excluded**: another PR's runtime change can break your surface without touching a file
you own, which is the entire reason the queue gate exists. Not free either — reusing `build`'s verdict
means reusing `local-platform-pack`'s artifact across runs — so it is sequenced last of the three.

#### Phase 3e — Delete the carve tier at the polyrepo cut, not before

`carve-*`, `carve-fe` and `split-inventory` simulate standalone repos. Once each service is its own repo
that is just its normal build. They are off the critical path (all inside two minutes, parallel to
`local-platform-pack`), so this is cleanup that follows the cut — never a latency lever, and never a reason
to weaken the standalone-build guarantee early.

#### Tightening what counts as a positive E2E trigger (same pass, no wall clock)

On 2026-09-09 the UI suite produced six failures and all six were genuine defects — a missing
`(Processing, Authorize, Authorized)` payment state-machine edge that left an authorized escrow
permanently uncaptured, and an invitation-acceptance deadlock that made joining an organization
impossible. Zero flakes. Both had sat undetected for days because `Skip-E2E` was being applied per-PR to
behaviour-changing diffs: tier selection was a judgment call, and the judgment was wrong.

So the classifier stops accepting the unsafe direction of that call: **a non-inert change under `api/`
that is not confined to a service's test tier forces both E2E lanes on, and the `Skip-E2E` /
`Skip-E2E-UI` trailer and label are ignored for that diff.** A change to an E2E suite's own sources counts
as a positive trigger too — that is the gate re-validating itself. The opt-out survives only where it was
always defensible: frontend-only, unit/integration/architecture test-tier-only, `eng/`, `scripts/` and
workflow diffs. `merge`'s Step 4 still selects the tier; it simply can no longer select *off* for a
runtime diff.

One case would have become unmergeable, so it gets a named exception instead of a silent one.
`PIPELINE_DEBT.md` records a published-package **expand** merge: the backend flips to the new wire shape
while its consumer surfaces are deferred to the sync merge, so `carve-fe` needs the old shape and UI E2E
needs the new one — full UI E2E cannot pass by construction, and the documented workaround was a generic
`skip-e2e-ui`. That is exactly the runtime diff the rule above now refuses to let opt out. The classifier
therefore honours one label, **`expand-merge`**, which drops the UI lane only and never the API lane that
proves the backend flip. Naming the situation rather than the effect is the point: claiming it is a
deliberate, greppable act pointing at a documented structural conflict, where `skip-e2e-ui` was
indistinguishable from ordinary "this seems fine". It retires with that debt entry.

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
