# Platform package sync — automate the cross-service publish→consume loop

**Goal:** kill the manual tax that turns a one-line cross-service contract change (e.g. renaming a
`Payment.Client` type B2B/Customer consume) into a multi-merge, manually-bookkept, red-in-the-middle
dance. Make the propagation **automated and green**, not hand-driven.

## The friction, stated precisely

Cross-**service** code is consumed as **version-pinned NuGet packages** off the org feed, not source
(the carve — [`api/ARCHITECTURE.md`](../api/ARCHITECTURE.md)). B2B/Customer pin a **fixed**
`ConcertablePlatformVersion` (e.g. `0.1.0-alpha.0.526`). So when Payment changes a published contract:

1. The new `Payment.Client`/`Contracts` only exist on the feed **after merge to master**
   (`publish-packages.yml`, push→`master`, `api/**`). MinVer gives them a *new* version.
2. Consumers keep compiling against the **old** pinned version until someone **manually bumps the pin**
   and migrates the consumer code — a second merge that can't happen until step 1 lands.

Two things make it worse today:

- **No auto-sync.** The pin bump is hand-edited in each service's `Directory.Packages.props`; nothing
  reminds you, nothing does it. Easy to forget; silent drift.
- **The full-solution build straddles both worlds.** `test.yml`'s `build` job runs
  `dotnet build api/Concertable.slnx` — production B2B/Customer bind the **package** (old), but their
  **integration-test fixtures `ProjectReference` the source** (new). A breaking source change makes the
  fixtures see the new shape while production sees the old → the build/tests go red mid-migration.
  (Proven empirically on `Feature/PaymentDtoConsolidation`: Payment.slnx green; full slnx red only in the
  4 B2B/Customer fixture mocks + a `TicketApiTests` mixed-reference collision.)
- **`UseLocalCore` doesn't cover this.** The existing hybrid-loop escape hatch swaps only the *churny
  core* (`Kernel`, `Messaging.*`) for source; cross-**service** adapter packages
  (`Payment.Client`/`Contracts`, `*.Tenant.Contracts`, etc.) have no local-source swap.

## The reframe — the 2-step loop is *inherent*, not ceremony to delete

A breaking change to a package that other **independently-deployable services** consume is **always** a
two-step release: publish the new package, then bump+migrate consumers. That's not a monorepo wart — it
is exactly what real separate repos do, and service independence is the whole point of the carve. We
could "cheat" with source refs while it's one repo, but the cheat evaporates the day a service splits
out, so building on it is building on sand.

**So the target is not "make it one PR" (that fights the architecture) — it's "make the two steps
painless, automated, and green."** For *non-breaking* platform changes, propagation should be fully
hands-off. For *breaking* ones, the tool should hand you a single green PR whose only manual content is
the consumer migration itself.

## Proposed solution

### Phase 1 — automated platform-sync PR (the core fix)

Add a `platform-sync` GitHub Actions workflow that triggers after `publish-packages.yml` succeeds on
`master` (workflow_run), computes the just-published MinVer version, and for every service whose
`ConcertablePlatformVersion` is behind it:

- bumps `ConcertablePlatformVersion` in that service's `Directory.Packages.props`,
- opens (or force-updates) a single branch/PR `chore/platform-sync-<version>`,
- lets normal CI run on it.

Outcomes:
- **Non-breaking platform change** → sync PR is green → **auto-merge** (label + branch-protection
  auto-merge). Zero human touch; services stay lockstep automatically.
- **Breaking change** → sync PR is **red** at the consumers that must migrate. The author does the
  migration *in that PR* — now legal, because the new package exists on the feed. One green PR closes it.

This automates all the pin bookkeeping and makes drift loud and self-healing.

### Phase 2 (optional, dev-ergonomics only) — extend `UseLocalCore` to cross-service adapters

So a developer can build the whole solution locally green while mid-migration (fast inner loop), extend
the `ChurnyCorePackage` swap in each service's `Directory.Build.targets` to also cover cross-service
adapter packages under `-p:UseLocalCore=true`. **Local/inner-loop only — never committed, never CI**
(same rule as today). CI + the carve gates still build against packages and remain the source of truth
for the boundary. This does *not* change the merge story; it just removes the local red while iterating.

### Explicitly rejected

- **Building PR CI on source (`UseLocalCore`) as the default.** Hides package/carve drift until
  post-merge (the carve jobs restore old feed packages and wouldn't catch a source-only rename), just
  relocating the red to master. Weakens the per-PR boundary guarantee the carve exists to give.
- **A repo-root `Directory.Packages.props` to float all pins together.** Explicitly the trap the
  per-folder-closure design forbids ([`ARCHITECTURE.md`](../api/ARCHITECTURE.md) "Per-folder build
  closures") — it wouldn't survive a carve.

## Steps

- [x] **Phase 1a** — `.github/workflows/platform-sync.yml` + `.github/scripts/bump-platform-version.sh`.
  Triggers on `workflow_run` (Publish packages → success on master) + `workflow_dispatch`; recomputes
  the just-published version with MinVer on the published commit (`--tag-prefix v --minimum-major-minor
  0.1`, same inputs as pack); bumps every lagging `Directory.Packages.props` (bump script — tested
  locally: bumps all 5 services, idempotent); opens/updates `chore/platform-sync-<version>` and sets
  `gh pr merge --auto --squash`. Auto-merge = green→merges, red→waits for a human to migrate. **Chose
  auto-merge over manual-click** (the whole point is hands-off non-breaking propagation).
- [ ] **Phase 1b — repo settings (needs Tommy; can't be done from code):**
  - Add secret **`PLATFORM_SYNC_TOKEN`** — PAT/App token with `contents:write` + `pull-requests:write`
    + `workflow`. **Required**: the default `GITHUB_TOKEN` can't trigger `test.yml` on the sync PR
    (GitHub recursion guard), so without this the PR would have no checks to gate auto-merge on.
  - Enable repo **"Allow auto-merge"** + branch protection on master requiring the `test.yml` checks.
    (Without it the PR still opens fine; it just won't auto-merge — a human clicks merge when green.)
- [ ] **Phase 1c** — verify live: a no-op platform bump → sync PR opens green → auto-merges; a
  deliberately-breaking change → sync PR opens red at exactly the consumers to migrate.
- [x] **Phase 1d** — documented the loop in `api/ARCHITECTURE.md` ("Cross-service contract changes":
  publish → auto sync-PR → green auto-merge / red migrate-in-PR).
- [ ] **Phase 2 (optional)** — extend the `UseLocalCore` swap to cross-service adapter packages for the
  local inner loop; document; keep out of committed config + CI.

## Once this lands — resume the Payment DTO consolidation

With Phase 1 in place, `plans/PAYMENT_DTO_CONSOLIDATION.md` becomes: (1) merge the Payment-side rename
(publishes new packages) — the validated work is parked on branch `Feature/PaymentDtoConsolidation`,
commit `wip(payment): … (PAUSED)`; (2) the auto sync-PR opens red at the 4 fixture mocks + `TicketPayment`,
migrate them there, green, merge. Two painless steps instead of the manual dance.
