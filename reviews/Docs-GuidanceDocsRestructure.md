# Code review — Docs/GuidanceDocsRestructure

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `54b91961969e0480a25dcb91330f274fb1f25c06`  _(2026-08-17)_

**Security-reviewed up to commit:** `54b91961969e0480a25dcb91330f274fb1f25c06`  _(2026-08-17)_

> Range reviewed: `9205e82d..2b93b45b` (12 commits reviewed; markers moved to `54b91961`, the fix commit, 73 files — markdown plus one Python hook).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).
>
> Security layer ran because the range touches `api/Concertable.Payment/**` (the merge gate's
> `_SECURITY_PATTERNS`). Those 12 lines are markdown-only pointer rewiring; no authz/authn rule, secret,
> credential, controller or workflow changed, and no secret-shaped literal appears in the diff. Nothing
> to report.
>
> Native-catalog layer (correctness/reuse/simplification/efficiency/error-handling) was applied inline
> over the only executable change — `.agents/hooks/docs_reachability.py` and its tests — rather than via
> the `code-reviewer` subagent, per this session's standing directive not to call the Agent tool. No
> defect found there: fence toggling, the working-doc warn/error split, the generator refactor and the
> root-absolute check all behave as their five new tests assert. Verified locally: `72 passed`,
> `docs_reachability.py` → **0 errors, 21 warnings** (all pre-existing `plans/` working docs).

## Findings

### Dead references the restructure created or left behind

- [x] **DOC1 — MEDIUM — dead reference (5 sites, one of them a runtime message)** — `api/Concertable.B2B/tests/Concertable.B2B.ArchitectureTests/ModuleBoundaryTests.cs:11`, `:61`, `:116` and `api/docs/MICROSERVICES_ARCHITECTURE.md:394`, `:436`
  This PR renamed `api/agents/CONVENTIONS.md` → `MODULE_STRUCTURE.md`, but five citations still name the
  deleted file. `:116` is a NetArchTest `.Because(...)` string, so a failing boundary test now points the
  developer at a file that does not exist. `MICROSERVICES_ARCHITECTURE.md` was edited in this PR and
  still names it twice. The reachability hook cannot catch these — they are backticked prose and a C#
  string, not markdown links. Repoint all five at `api/agents/MODULE_STRUCTURE.md`.

- [x] **SKILL1 — LOW — dead path in an agent-loaded skill** — `.agents/skills/e2e-api-debug/SKILL.md:215`
  Cites `api/docs/SEEDING_CONVENTIONS.md`; the file is at `api/agents/SEEDING_CONVENTIONS.md`. Same class
  as the `./e2e.ps1` → `./scripts/e2e.ps1` corrections this PR made in this very file (Phase 2). Fix the
  directory.

### Docs that state something the code does not do

- [x] **DOC2 — MEDIUM — the authoritative current-state doc asserts three surfaces that don't exist** — `api/ARCHITECTURE.md` "The surface each service actually exposes"
  The table's preamble says "What is specific to this system is which surfaces exist", then gives B2B,
  Customer and Search an internal **gRPC** surface. Only Payment has one: the repo's single `.proto` is
  `api/Concertable.Payment/src/Concertable.Payment.Client/Protos/payment.proto`, and `AddGrpc` /
  `MapGrpcService` appear only in `Concertable.Payment.Web/HostExtensions.cs`. The table came from the
  deleted `api/agents/MICROSERVICE_COMMUNICATION.md`, which was explicitly target-state design ("the only
  exception is a transition window **before** a service has its gRPC surface"); folding it verbatim into
  the doc `docs/INDEX.md:44` declares "current-state — authoritative" converts a plan into a claim. Mark
  the three internal cells as the target (none today), or state that Payment is currently the only gRPC
  surface.

- [x] **CV1 — MEDIUM — names a fixture member that doesn't exist** — `api/agents/INTEGRATION_CONVENTIONS.md:45`
  "Derive expectations from `fixture.Catalog`, never invented literals." No integration `ApiFixture`
  exposes `Catalog` — B2B's exposes `SeedNow` (`GetRequiredService<SeedCatalog>().Now`) and every
  integration test reads `fixture.SeedState.…` (e.g. `ApplicationApiTests.cs:29`). `Catalog` exists only
  on Customer's **E2E** `AppFixture.cs:56`. Following this line as written does not compile. Change to
  `fixture.SeedState` (and `fixture.SeedNow` for the clock).

- [x] **CV2 — MEDIUM — puts every concrete context in the wrong project** — `api/Concertable.B2B/CODE_PATTERNS.md:9`
  "All in `B2B.DataAccess.Infrastructure`." sits directly above a table whose right-hand column is
  *Concrete examples*, so it asserts `ConcertDbContext`, `VenueDbContext`, `ArtistDbContext`,
  `ConcertReadDbContext` and `VenueAdminDbContext` live there. Only the **bases**
  (`TenantScopedDbContext`, `VenueArtistTenantScopedDbContext`, `AdminDbContext`) do; every concrete
  context sits in its own module (`Concertable.B2B.Concert.Infrastructure/Data/ConcertDbContext.cs`,
  `…Venue.Infrastructure/Data/VenueAdminDbContext.cs`, …). The table's own `ReadDbContext (shared
  DataAccess)` cell already contradicts the sentence. Say the bases live there and each concrete context
  in its module's `Infrastructure/Data/`.

- [x] **CV3 — MEDIUM — rosters a type that doesn't exist and omits the two that do** — `app/agents/CODE_CONVENTIONS.md:55`
  "`Contract` is the other umbrella." There is no `Contract` type anywhere in `app/` — the contract is
  fetched as a `Blob` (`concertApi.getContractPdf`). The actual second `$type` union is `Deal` =
  `FlatFeeDeal | DoorSplitDeal | VersusDeal | VenueHireDeal` (`app/web/b2b/shared/src/features/deals/types.ts`,
  mirroring `DealTypeNames`), and a third is the search `Header` / `AutocompleteResult` pair keyed on
  `HeaderType` (`app/shared/src/features/search/types.ts`). For a file whose entire job is the roster of
  real names, name those instead.

### Duplication the restructure was supposed to remove

- [x] **DUP1 — LOW — a generic rule kept in-repo under a "Concertable-specific" heading** — `api/agents/INTEGRATION_CONVENTIONS.md:64–72`
  "The two Concertable-specific seeding shapes tests rely on" restates the `seeding` skill almost
  point-for-point: "**A domain entity never carries a `Seed` static itself** — that leaks test and
  infrastructure concerns into the domain" and "do not guard on `AnyAsync()` … Guard on a specific entity
  only the seeder ever creates" are both in that skill (§"Seed state is constructor-built",
  §"Idempotency and sentinel guards"). `api/agents/SEEDING_CONVENTIONS.md:67–70` states the factory
  placement a third time. Only `CredentialFactory.Seed` vs `.Create` is actually local. Cut the section to
  that precedent plus a link — meta-rule 1, and the file's own "This file is the inventory of what exists
  here".

- [x] **DUP2 — LOW — the summary drifts from the inventory it summarizes** — `api/AGENTS.md:34–41`
  The inlined forbidden-seed list omits **invitation rows (`TenantInvitationEntity`, `tenant.Invitations`)
  and invitation-derived memberships**, which the owning inventory carries at
  `api/agents/SEEDING_CONVENTIONS.md:24–27`. A reader trusting the summary concludes seeding an invitation
  membership is allowed. This is the exact drift the plan's duplication table row 1 recorded, still live.
  Add the bullet now; the import-or-pointer decision stays Phase 4's.

### Reachability

- [x] **ORPH1 — LOW — two live rule docs are loaded nowhere** — `api/Concertable.Shared/tests/Concertable.Testing.E2E/E2E_UI_CONVENTIONS.md`, `E2E_CONSIDERATIONS.md`
  Zero inbound references repo-wide (`git grep` at the merge-base too — pre-existing, not introduced), and
  the hook's orphan check only walks `*/agents/*.md`, so it reports 0 errors while both sit unread. They
  carry rules nothing else states: page objects named after their TSX component with `private ILocator X
  => page.GetByTestId(...)` properties, and "do not add timeouts to fix failures" / "tests must pass in
  isolation". This PR rewrote their natural parent's "Scenario-authoring rules" section and still didn't
  link them. Link both from `Concertable.Testing.E2E/AGENTS.md` (or fold the live rules in and delete the
  files) and give page-object naming a row in `docs/INDEX.md`.

- [x] **IDX1 — LOW — the index gives one topic two owners** — `docs/INDEX.md:45` and `:49`
  Both rows end "what may `WaitFor` what" — one owned by `api/ARCHITECTURE.md`, one by skill
  `microservice-boundaries`. Row 49's real topic is protocol selection; drop the trailing clause so the
  file obeys its own rule 1.
