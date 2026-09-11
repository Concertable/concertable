# Auth identity model progress

- Plan: `plans/auth-identity-model/AUTH_IDENTITY_MODEL_PLAN.md`
- Roadmap: `plans/auth-identity-model/AUTH_IDENTITY_MODEL_ROADMAP.md`
- Roadmap item: `auth-identity-model/typed-identity-contract`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Refactor-AuthIdentityModel-Phase2`
- Branch: `Refactor/AuthIdentityModelPhase2`
- PR: #1008 (draft) — Phase 2 consumer migration. https://github.com/Concertable/concertable/pull/1008
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

## Current state — Phase 2, code-complete, not yet committed

All consumer migration done: `ConcertableAuthVersion` pin (all 6 consumers); Auth `Config.cs` +
`AuthHostExtensions` + new `ServiceClient`/`ServiceClientInfo`/`ServiceClients` in `Concertable.Auth`;
`AuthDevSeeder`; the three registration handlers; the four resource-server hosts (+ package refs, Search's
new `PackageVersion`); `Payment.Client` (`PrivateAssets="all"`); `Concertable.Testing.E2E`'s
`TestTokenMinter`; every `[InlineData(ClientIds.X)]` test site moved to `[InlineData(InteractiveClient.X)]`
+ `.Info().Id` inside the test body (the const string can't survive as a typed-enum inline-data value, so
each affected test's parameter type changed from `string` to `InteractiveClient`). `ClientIds.cs` /
`ApiScopeIds.cs` deleted; `api/Concertable.Auth/TECH_DEBT.md` → "No outstanding debt"; `AGENTS.md` updated.

`grep -rln "ClientIds\.\|ApiScopeIds\." api --include='*.cs'` → zero.

**Verified:** every touched project builds 0 warnings (`Concertable.Auth`, `Concertable.Auth.Contracts` +
its tests, B2B.Web, Customer.Web, Payment.Web, Payment.Client, Search.Web, Testing.E2E, the three handler
projects, all touched test projects). Non-Docker suites green: `Concertable.Auth.Contracts.UnitTests`
(50), `Concertable.Auth.UnitTests` (13), `Concertable.Auth.StartupTests` (11), `Concertable.Customer.User.UnitTests`
(15). Docker was down locally — B2B Tenant/User/Admin integration tests only build-verified; the merge
queue's `carve-*`/`integration-tests` jobs are the real gate.

**Environment note:** mid-session the workstation hit ~0 bytes free disk (unrelated background load from
this machine's other worktrees/NuGet cache, not this plan's own doing) — background jobs got killed by the
harness and one `Concertable.Auth.StartupTests` run failed on a Duende dev-signing-key file write race.
Freed ~27GB (`bin`/`obj` sweep + NuGet http-cache clear); re-run in isolation and full-suite both passed
clean afterward. Not a code defect — noted in case the same signature recurs.

**Superseded mid-Phase-2:** while this branch was in flight, the repo owner landed the proper fix for the
Phase-1-discovered pin freeze — `plans/platform/PLATFORM_RELEASE_TRAINS_PLAN.md` Phase 2/3
(`feat(packages): give service-owned packages their own train property`, merged as part of PR #1004,
~an hour after this branch forked). It introduces **exactly** the `ConcertableAuthVersion` name this branch
had already improvised, plus siblings for every other retained target
(`ConcertableB2BContractsVersion`/`ConcertableCustomerVersion`/`ConcertablePaymentVersion`/`ConcertableSearchVersion`),
advances `ConcertableDotNetPlatformVersion` to `0.2.0-alpha.0.4`, and adds `.github/scripts/pin-trains.test.mjs`
enforcing every package id pins to the train its `eng/repository-split/inventory.json` target names.
Merged `origin/main` in (`8e436a702`); resolved by taking main's property scheme everywhere and (a) bumping
`ConcertableAuthVersion` from main's `.1381` to `.1383` (the version this plan's Phase 1 actually published)
and (b) re-adding the `Concertable.Auth.Contracts` `PackageVersion` line to Search and Shared, which main's
version doesn't carry (they didn't consume it before this plan). `node --test
.github/scripts/pin-trains.test.mjs` — 3/3 pass. Rebuilt + retested everything post-merge — all still green.

## Next Steps

PR #1008 already exists (draft) — the "push, open the PR" step below is stale in that this branch is
already pushed; treat it as "confirm CI on the current head" instead. Two things surfaced 2026-09-11
reviewing this branch against a scratch spike of the same migration (see Decisions) — resolve both before
requesting review:

1. **Apply the extension-block fix** (`AUTH_IDENTITY_MODEL_PLAN.md`, "Design decisions" — the new
   `extension()`-block bullet). Mechanical, no design call: convert `InteractiveClients`/`AuthScopes`/
   `AuthResources`/`ServiceClients` from legacy `this`-parameter extension methods to `extension()` blocks
   with property members, and update every call site (`.Id()` → `.Id`, `.Info()` → `.Info`, `.Audience()` →
   `.Audience`, `.AcceptedScopes()` → `.AcceptedScopes`, `.IncludedClaims()` → `.IncludedClaims`). Rebuild +
   rerun the non-Docker suites listed under Verified above.
2. **Resolve the `AuthParty` open question** (`AUTH_IDENTITY_MODEL_PLAN.md`, "Open questions") — repository
   owner call: does client → business-party classification stay in `Concertable.Auth.Contracts` as
   `AuthParty`, or move to `Concertable.Contracts` as a neutral `Party` (the `Genre` precedent)? This is the
   one open item that is genuinely undecided, not something to guess at again. It only affects
   `TenantProvisioningHandler`/`CredentialRegisteredHandler`/`UserCreationHandler` and their tests — rework
   those three (and only those three) once decided.
3. Once both are resolved: push, confirm CI on the head that includes the fixes, review (`review` skill) +
   record in `## Reviews`, get exact-head CI green (Docker-backed integration tests run there —
   Auth/B2B Tenant/User/Admin were only build-verified locally). Apply the `merge` skill's tier table fresh
   for the final head.
4. Merge. No sync PR follows this one (see Phase 1 note above) — Phase 2 is delivery-terminal on its own
   merge.
5. Close the whole plan: delete `plans/auth-identity-model/` and `reviews/Refactor-AuthIdentityModel.md`,
   tick the roadmap item, in the Phase 2 PR's own merge commit (not a separate docs tail).

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
- **2026-09-11, same evening:** a separate session spiked the same Phase 2 migration from scratch in
  `.worktrees/Chore-TechDebt-Auth-20260911-122436` (branch `Chore/TechDebt-Auth-20260911-122436`,
  no PR), not knowing this branch/PR #1008 already existed. It independently found the two issues in
  "Open questions" above — the extension-block-syntax gap and the `AuthParty`-in-an-identity-package
  concern — reverted its own attempt at the three handlers rather than guess at the resolution, and wrote
  both up. That worktree's own copies of this plan/ledger are now superseded by this entry; its code
  changes (extension-block conversion, Auth `Config.cs`/`TestTokenMinter` wiring) are a verified-safe
  reference for step 1 above but were never intended to ship from there. Retire that worktree
  (`worktrees.ps1 retire`) once step 1 above is done here — nothing in it needs preserving beyond what
  this entry already captured.
