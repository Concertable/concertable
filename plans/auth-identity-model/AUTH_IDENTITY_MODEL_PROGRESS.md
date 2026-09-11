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

## Current state — Phase 2, `AuthParty` resolved and decentralized, uncommitted; one new question open

All original consumer migration done (as before): `ConcertableAuthVersion` pin (all 6 consumers); Auth
`Config.cs` + `AuthHostExtensions` + new `ServiceClient`/`ServiceClientInfo`/`ServiceClients` in
`Concertable.Auth`; `AuthDevSeeder`; the four resource-server hosts (+ package refs, Search's new
`PackageVersion`); `Payment.Client` (`PrivateAssets="all"`); `Concertable.Testing.E2E`'s `TestTokenMinter`;
every `[InlineData(ClientIds.X)]` test site moved to `[InlineData(InteractiveClient.X)]` +
`.Info().Id` inside the test body. `ClientIds.cs` / `ApiScopeIds.cs` deleted; `api/Concertable.Auth/TECH_DEBT.md`
→ "No outstanding debt"; `AGENTS.md` updated.

`grep -rln "ClientIds\.\|ApiScopeIds\." api --include='*.cs'` → zero.

**New this session (2026-09-11, same evening):** the `AuthParty` open question resolved (repository owner,
mid-session) — see Plan's Open Questions "RESOLVED 2026-09-11" entry for the full rationale. Mechanically:

- `api/Concertable.Auth.Contracts/AuthParty.cs` **deleted**. `InteractiveClientInfo` now carries only
  `Client`, `Id`, `MobileScheme` — no `Party`, no `IsB2b`. `InteractiveClientsTests.cs` trimmed to match
  (50 → 37 tests; removed `Find_APartyClient_CarriesThatParty`, `E2ETest_HasNoParty`,
  `IsB2b_IsTrueForVenueArtistAndAdminOnly`).
- New `Concertable.B2B.Infrastructure/Authorization/ManagerClients.cs` — B2B's own registration-time role
  classification: one `FrozenDictionary<InteractiveClient, TenantType?>`, exposed via C# 14 `extension()`
  block members (`client.IsManagerClient`, `client.ManagerTenantType`), matching the
  `InteractiveClients`/`AuthScopes`/`AuthResources`/`ServiceClients` catalog idiom already used 4× in this
  codebase. **One table, not two** — the first pass split it into two parallel per-handler maps
  (`TenantTypeByClient` in `TenantProvisioningHandler`, `ManagerClients` HashSet in
  `CredentialRegisteredHandler`); consolidated before landing, since two hand-written maps over the same key
  set is exactly the drift risk `csharp-naming`'s "never a parallel frozen map per consumer" line exists to
  prevent. `extension()` blocks are safe here (unlike in Auth.Contracts — see below) because
  `Concertable.B2B.Infrastructure` is `ProjectReference`-only within the same service, never published.
- `TenantProvisioningHandler` (B2B/Tenant), `CredentialRegisteredHandler` (B2B/User) now read
  `client.Client.ManagerTenantType` / `client.Client.IsManagerClient` from `ManagerClients`.
  `Concertable.B2B.Infrastructure.csproj` gained a `PackageReference` to `Concertable.Auth.Contracts`;
  `Concertable.B2B.User.Infrastructure.csproj` gained a `ProjectReference` to `Concertable.B2B.Infrastructure`
  (it had none before — User and Tenant modules now share this one classification via the existing
  cross-module shared-infra project, the same project Tenant.Infrastructure already referenced).
- `UserCreationHandler` (Customer) now matches `client.Client is InteractiveClient.CustomerBrowser or
  InteractiveClient.CustomerMobile` inline — no table, single call site.
- **Reverted this session, do not redo without reading the note below:** an attempt to also convert
  `InteractiveClients`/`AuthScopes`/`AuthResources`/`ServiceClients` from legacy `this`-parameter extension
  methods to C# 14 `extension()` blocks (the old Next Steps step 1) was fully reverted — see Decisions.

`grep -rniE "AuthParty|IsB2b|\.Party\b" api --include='*.cs'` → zero.

**Verified this session:** `Concertable.Auth.Contracts` + its tests (37 pass, 0 warnings); full rebuild of
`Concertable.B2B.Web`, `Concertable.Customer.Web`, `Concertable.Payment.Web`, `Concertable.Search.Web`,
`Concertable.Auth` — all 0 warnings, 0 errors, against the still-published `0.1.0-alpha.0.1383` package
(these changes needed no new publish — see Decisions). `Concertable.Auth.UnitTests` (13),
`Concertable.Auth.StartupTests` (11), `Concertable.Customer.User.UnitTests` (15) — all green, all re-run
against the current head. Docker came up later the same session — B2B Tenant (83), User (14), Admin (8)
integration tests all subsequently run and green too. **Every locally-runnable suite touched by this branch
is now verified green** on the current head; only the merge-queue's Docker-backed CI (exact-head, full
matrix) and a review pass remain before this branch can be considered merge-ready.

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

**The old step 1 and step 2 below (extension-block fix, `AuthParty` question) were both wrong or incomplete
in ways that cost real time this session — read the corrections, don't repeat either mistake.**

1. **Decide the newly-surfaced B2B Authorization module question first** — see
   `AUTH_IDENTITY_MODEL_PLAN.md`, Open Questions, "Not yet decided" paragraph under the RESOLVED entry.
   Repository owner call, genuinely undecided:
   - **(a) Build `Concertable.B2B.Authorization` now**, as its own new plan (own roadmap item, own worktree/
     branch — this is out of Phase 2's scope and out of this worktree per the worktree identity gate, since
     it rewires every B2B module's policy registration and inverts Tenant's dependency direction). Phase 2
     stays open until that plan exists and is at least scoped.
   - **(b) Leave the interim placement** (`ManagerClients` in `Concertable.B2B.Infrastructure/Authorization/`,
     current state — already consolidated, already the right *local* shape, just not the full module move)
     and log the rest as a tracked `api/Concertable.B2B/TECH_DEBT.md` entry with an objective resolution
     condition. Phase 2 can proceed to close-out on its own terms.
   Do not pick one unilaterally — this was raised explicitly for the repository owner to decide, not to
   guess at again the way the old steps 1/2 below were guessed at.
2. **The extension-block conversion is not a same-branch fix — do not attempt it inside a PR that also
   touches consumers.** The old step 1 called it "mechanical, no design call" and cited a scratch-branch
   verification as proof; that verification only proved `Concertable.Auth.Contracts` itself builds with the
   new shape, never that any consumer does. `Concertable.Auth.Contracts` is `PackageReference`-only
   everywhere (confirmed: `git checkout`-reverted after a 31-error build across `Concertable.Auth` alone),
   so `.Id()` → `.Id` etc. is a real breaking-package change needing its own **producer-only** PR (touches
   only `api/Concertable.Auth.Contracts/`, self-contained, publishes independently — same shape as Phase 1),
   then a separate consumer-sync PR after that publish lands. If not already a roadmap item, add one; do not
   fold it into Phase 2 again.
3. Once (1) is decided and, if (b), logged: rerun the suites not yet re-verified this session
   (`Concertable.Auth.UnitTests`, `Concertable.Auth.StartupTests`, `Concertable.Customer.User.UnitTests`,
   B2B Tenant/User/Admin integration — Docker required), review (`review` skill) + record in `## Reviews`,
   confirm CI on the exact head, apply the `merge` skill's tier table.
4. Merge. No sync PR follows this one (see Phase 1 note above) — Phase 2 is delivery-terminal on its own
   merge.
5. Close the whole plan: delete `plans/auth-identity-model/` and `reviews/Refactor-AuthIdentityModel.md`,
   tick the roadmap item, in the Phase 2 PR's own merge commit (not a separate docs tail). If (1a) was
   chosen, its own new plan stays open after this one closes — it is a separate epic from here on.

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
  both up. That worktree's own copies of this plan/ledger are now superseded by this entry.
  **Correction, same evening, later:** its extension-block conversion is *not* a safe reference after
  all — see the entry below. Its code changes are otherwise dead; retire that worktree
  (`worktrees.ps1 retire`) whenever convenient — nothing in it needs preserving beyond what this ledger
  already captures.
- **2026-09-11, later the same evening — the extension-block conversion was attempted and reverted.**
  Converted `InteractiveClients`/`AuthScopes`/`AuthResources`/`ServiceClients` to C# 14 `extension()` blocks
  and updated every call site across all 6 consumers (~20 files). `Concertable.Auth.Contracts` itself built
  and its own 50 tests passed. Building `Concertable.Auth` then failed with 31 errors — `CS0119`/`CS1503`
  at every `.Id`/`.Info`/`.Audience`/`.AcceptedScopes`/`.IncludedClaims` call site, because every consumer
  restores `Concertable.Auth.Contracts` as a `PackageReference` pinned to the already-published
  `0.1.0-alpha.0.1383`, which still has the *legacy method* shape — confirmed via
  `api/Concertable.B2B/src/Modules/User/Concertable.B2B.User.Infrastructure/*.csproj`'s own comment
  ("Keep as PackageReference, never a ProjectReference") and `api/PlatformSourcePackages.targets` (does
  not list `Concertable.Auth.Contracts` among the locally source-swappable packages), and
  `.github/workflows/publish-packages.yml` (`on: push: branches: [main]` only — no PR-time publish, so
  there is structurally no way to build a consumer against the new shape before merge). Reverted with
  `git checkout --` on every extension-block-only file, plus a manual partial revert on the two files that
  mixed the extension-block change with the (kept) `AuthParty` removal (`InteractiveClients.cs`,
  `InteractiveClientsTests.cs`). This is exactly the `dotnet:package-cutover` "expand merge, structural red"
  situation, except normally expand and sync are separate PRs — this branch tried to do both against an
  unpublished shape in one PR, which cannot work. See Next Steps step 2.
- **`ManagerClients` design iteration, same evening.** First cut: two independent hand-written maps, one per
  consuming handler (`TenantTypeByClient` in `TenantProvisioningHandler`, a `ManagerClients` `HashSet` in
  `CredentialRegisteredHandler`), and free static methods instead of extension members. Both wrong per this
  repo's own `csharp-naming`/`keyed-strategies` conventions (parallel maps risk drift; new extension members
  must be `extension()` blocks, not `this`-parameter methods) — consolidated to one table with extension
  properties before landing. Location also iterated: `Concertable.B2B.Infrastructure/Auth/` (wrong — reads
  as Auth-service content) → `.../Registration/` (wrong — this repo already has a real `Authorization/`
  naming convention, in `Concertable.B2B.Tenant.Infrastructure/Authorization/PermissionAuthorizationHandler`,
  that should have been searched for and matched first) → `.../Authorization/` (current, matches the
  existing convention). See the Plan's Open Questions "Not yet decided" entry for the bigger, still-open
  question this raised — whether B2B needs a real Authorization module rather than this interim placement.
- **A citation in the Plan's "Design decisions" section was factually wrong and got corrected twice this
  evening** — see that section directly (`AUTH_IDENTITY_MODEL_PLAN.md`, the `extension(AuthScope scope)`
  discriminated-union bullet). First claimed a repo-wide Dunet avoidance that doesn't exist; the fix for that
  then wrongly reframed the reasoning around the published-package boundary instead of the actual test
  (is the shape a genuine union of heterogeneous cases, or a flat homogeneous table — `InteractiveClient` is
  the latter). Read that section directly rather than this summary before citing it again.
