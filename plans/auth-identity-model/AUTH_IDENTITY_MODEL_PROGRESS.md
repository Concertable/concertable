# Auth identity model progress

- Plan: `plans/auth-identity-model/AUTH_IDENTITY_MODEL_PLAN.md`
- Roadmap: `plans/auth-identity-model/AUTH_IDENTITY_MODEL_ROADMAP.md`
- Roadmap item: `auth-identity-model/typed-identity-contract`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Refactor-AuthIdentityModel-Phase2`
- Branch: `Refactor/AuthIdentityModelPhase2`
- PR: not yet opened (Phase 2 — consumer migration)
- Dependency/package gates: none remaining — `ConcertableAuthVersion` now pins all 6 consumers to
  `0.1.0-alpha.0.1383`, the version `Concertable.Auth.Contracts` published at. `ConcertableDotNetPlatformVersion`
  stays untouched at `0.1.0-alpha.0.1370` (frozen — see Decisions).
- Last reconciled: `2026-09-11` against `origin/main` `48ce79347` (Phase 1's own merge commit).

## Phase 1 — terminal (producer)

PR #986 merged (`48ce79347`, 2026-09-11); published `Concertable.Auth.Contracts` `0.1.0-alpha.0.1383`.
Reviewed twice (4 findings, all fixed — see `reviews/Refactor-AuthIdentityModel.md`, retained until this
whole plan closes). Merge-queue ejection on the way in was a Docker/Testcontainers infra flake in
`Concertable.Payment.IntegrationTests` (PR touched zero Payment files) — re-enqueued, landed clean.

**Discovery that reshapes Phase 2:** commit `7adedd3a0` ("Cut over platform package publishing", merged
2026-09-11 ~03:27, ~7h before #986) **deleted the `platform-sync` bot** (`platform-sync.yml` /
`platform-sync-alert.yml` — the thing that used to open a `chore/platform-sync-<version>` PR bumping every
service's pin within minutes of a publish). Pin refresh is now **Renovate**, weekly
(`renovate.json`: `schedule: ["* 0-4 * * 1"]`), `automerge: false`, grouped as "shared .NET platform
train". **There is no sync PR to wait on or follow for this plan, or any future plan that publishes a
platform package** — the producer PR's own author bumps every consumer's `ConcertableDotNetPlatformVersion`
directly, in the same PR that migrates consumers (or a dedicated pin-bump PR, same as before this cutover
existed). This is a repo-wide change, not particular to this plan.

## Current state — Phase 2, not yet started

Fresh worktree created; no code changes yet. Plan's Phase 2 section and Package topology updated for the
no-sync-bot reality.

## Next Steps

1. ~~Add `ConcertableAuthVersion`~~ — **done.** User confirmed the `ConcertablePaymentVersion` pattern,
   named it `ConcertableAuthVersion` (not `ConcertableAuthContractsVersion`). Added to all 6 consumers
   (Auth, B2B, Customer, Payment, Search, Shared) at `0.1.0-alpha.0.1383`, `Concertable.Auth.Hosting` +
   `.Contracts` retargeted onto it. Verified: `Concertable.Auth`, `B2B.Web`, `Customer.Web`, `Payment.Web`,
   `Search.Web` all restore clean. (First attempt had an illegal `--` inside an XML comment, breaking CPM
   for every package in the file — fixed.)
2. Migrate every consumer per the Phase 2 consumption table in `AUTH_IDENTITY_MODEL_PLAN.md`: Auth
   `Config.cs`/`AuthHostExtensions` (+ new `ServiceClient` enum/catalog in `Concertable.Auth`), the three
   registration handlers (`TenantProvisioningHandler`, `CredentialRegisteredHandler`, `UserCreationHandler`),
   the four resource-server hosts (add `Concertable.Auth.Contracts` package refs where missing — Search
   needs a new `PackageVersion` too), `Payment.Client` (`PrivateAssets="all"`), `Concertable.Testing.E2E`
   (`TestTokenMinter`), and the Auth/B2B/Customer test `[InlineData]` sites using client-id string literals.
3. Delete `ClientIds` / `ApiScopeIds`; delete the resolved `api/Concertable.Auth/TECH_DEBT.md` entry; update
   its `AGENTS.md` "Duende config is in code" note.
4. `grep -rniE "ClientIds|ApiScopeIds"` → zero (allowlist: none needed, both are fully removed).
5. Build affected projects + run Auth/B2B Tenant/User/Customer User unit+integration suites; open the PR
   (breaking-but-safe republish — nothing outside the repo references the deleted classes); review; merge.
   No sync PR follows — Phase 2 is delivery-terminal on its own merge.
6. Close the whole plan: delete `plans/auth-identity-model/` and `reviews/Refactor-AuthIdentityModel.md`,
   tick the roadmap item, in the Phase 2 PR's own commit (not a separate docs tail).

## Reviews

Phase 1's two review passes are recorded in `reviews/Refactor-AuthIdentityModel.md` (kept live until this
plan closes, since it is still this branch's local merge-gate evidence trail for that PR). Phase 2 needs
its own fresh `review` pass before merge.

## Decisions, discoveries, blockers, and deviations

- **No platform-sync bot any more, and `ConcertableDotNetPlatformVersion` is frozen** (see above) — the
  load-bearing facts for Phase 2 and any future published-package change in this repo. Tried bumping
  `ConcertableDotNetPlatformVersion` to `0.1.0-alpha.0.1383` first (matching Phase 1's published version);
  restore broke (`NU1605` downgrade conflict) because the feed's platform-dotnet train (`Concertable.Kernel`
  etc.) tops out at `0.1.0-alpha.0.1371` / has moved to an independent `0.2.0-alpha.0.x` line — reverted.
  `plans/platform/PLATFORM_RELEASE_TRAINS_PROGRESS.md` confirms this is deliberate, not a gap: the pin stays
  at its "resolvable slow floor" until retained packages (Auth.Contracts, B2B.*, …) get their own train
  property, same as `ConcertablePaymentVersion` already gives Payment.
- `Concertable.Auth.Contracts` is consumed only as a feed `PackageReference`, never source-swapped
  (`PlatformSourcePackages.targets` does not list it) — this is why Phase 1 had to be additive-only and
  Phase 2 needs the pin bump before any consumer can see the new types.
- Service clients (`concertable-b2b` etc.) stay out of the published contract — they do not cross the wire
  and their secrets are Auth's. `ServiceClient` enum lands in `Concertable.Auth` this phase.
- Superseded `Chore/TechDebt-20260909-234925` / PR #981 (closed) — its worktree is still unretired
  (`worktrees.ps1 retire` needs a durable evidence commit; #981 was closed not merged, so there is none —
  low-priority manual cleanup, not blocking this plan).
