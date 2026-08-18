# Code review — Docs/GuidanceDocsRestructure

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `fc6a14d2c53baf8f8a7eebe74d311586c4492d7d`  _(2026-08-18)_

**Security-reviewed up to commit:** `fc6a14d2c53baf8f8a7eebe74d311586c4492d7d`  _(2026-08-18)_

> Range reviewed: `9205e82d..2b93b45b` (12 commits reviewed; markers moved to `54b91961`, the fix commit, 73 files — markdown plus one Python hook).
> Markers moved forward three times with nothing re-reviewable in between: once to the fix commit
> `54b91961`; then to the first `origin/main` merge required before enqueueing — that merge brought in only
> `plans/frontend/*`, `reviews/Fix-merge-gate-command-matching.md` and `.claude/hooks/merge-review-gate.py`,
> all already reviewed and merged on `main`; then to `c8302694` after a second such merge and two ledger
> commits, the merge's only content outside `plans/`/`reviews/` being the `<ConcertablePlatformVersion>`
> bump `0.1.0-alpha.0.1055` → `0.1.0-alpha.0.1061` across five `Directory.Packages.props`, reviewed and
> merged on `main` as #645. None of it is in this branch's scope.
>
> **The markers are no longer clean-and-idle.** The Phase 6 enforcement review below covers
> `c8302694..e29cd957` and found three real defects, fixed in `645ca501`. A fourth, found while
> verifying the skill deployment, is fixed in `07800262`, where the markers now sit. See
> "Incremental review — 2026-08-17 (Phase 6 enforcement)".
>
> **This review is not merge authorization.** Tommy has not read the PR himself; the
> ledger's `## Next Steps` carries the human-gated `Paused:` line. Do not enqueue on the strength of
> this file alone.
>
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

## Incremental review — 2026-08-17 (docs-review)

> Range: `aa5759d5..adb7e255` — the two plan/ledger checkpoints, the `origin/main` merge (69 commits)
> and its three conflict resolutions, and this reconciliation. The merged-in `main` commits are already
> reviewed on `main` and are out of scope; what is in scope is the branch's own surviving content.

Verified: `docs_reachability.py` **0 errors / 22 warnings** (all pre-existing `plans/` working docs),
`plan_graph.py` **0 errors**, hook suite **72 passed**. No merge marker survives anywhere in the tree.

Three findings, all fixed in `adb7e255`:

- [x] **ACC11 — MED — Lens A: stale fact** — `plans/docs/GUIDANCE_DOCS_RESTRUCTURE_PROGRESS.md:10`, `:84`
  The shared-repo split was recorded as "35 skills, 28 generic … the 8 TypeScript/React ones".
  Enumerating `dotagents/.agents/skills` (39 directories, 10 of them utilities) and
  `agent-standards/.agents/skills` (7) gives **29 generic — 20 .NET + 9 TS/React — and 36 total**. The
  plan repeated the same three numbers.

- [x] **ACC12 — MED — Lens A: described behaviour contradicts the config it documents** —
  `plans/docs/GUIDANCE_DOCS_RESTRUCTURE_PROGRESS.md:154–161`
  Step 1 asserted the enqueue was blocked by the main checkout still holding "the 180-line copy" of
  `merge-review-gate.py`. That file is the **281-line** version, with `review_only` at `:128` and the
  `reviews/`-only whitelist at `:226`. The gate is satisfiable; the only thing outstanding is Tommy's
  merge authorization, which is not a hook problem and should not be described as one.

- [x] **ACC13 — HIGH — Lens A: stale fact** — `plans/docs/GUIDANCE_DOCS_RESTRUCTURE_PROGRESS.md:9`, `:12`
  The header claimed head `aa5759d5`, "all 54 PR checks pass" and "0 behind at review time". At review
  the branch was **69 behind** `origin/main` and the PR was **`DIRTY`** — a state the ledger would have
  sent the next session straight into `gh pr merge` against.

One deliberate addition, not a finding: main added a rule to `api/agents/CODE_CONVENTIONS.md` after the
Phase 3b cut — integration events version the **wire identity** (`[MessageType(…​.v1)]`), never the CLR
type. No skill owns event wire versioning, so the reduction kept it rather than dropping a live rule;
`docs/INDEX.md:73` names the new topic and the ledger lists it as a promotion candidate.

## Incremental review — 2026-08-17 (Phase 6 enforcement)

> Range: `c8302694..e29cd957` (21 commits, 101 files). The ledger's `## Next Steps` named `2b93b45b..HEAD`,
> but the marker is the contract between runs and it already sat at `c8302694` — two marker moves later,
> both over merges whose content was already reviewed and merged on `main`. Reviewing from `2b93b45b`
> would have re-covered that. Ledger corrected.
>
> In scope: the build tier gate (`api/TestConventions.targets`, `api/BannedSymbols.UnitTests.txt`, nine
> `Directory.Build.targets` imports), the vendored skill router and its wiring, the `docs_reachability.py`
> test-project rule, 17 new test-project stub pairs, `review` Step 2, `docs/INDEX.md`, `.agents/README.md`.
> Out of scope, arriving by merge and already on `origin/main`: `OpportunityMapper.cs` (#617, verified
> `9a84f45e` is an ancestor of `origin/main`), the `plans/payments/*` closeout (#650), and the
> `<ConcertablePlatformVersion>` bump to `0.1.0-alpha.0.1064` (#647).

Verified: hook suite **92 passed** (+4 subtests), `docs_reachability.py` **0 errors / 23 warnings** (all
pre-existing `plans/` working docs), `plan_graph.py` **0 errors**. Every route in `.agents/skill-routes.json`
matches real files, and the four rows added by tier 3 match their claimed counts exactly (`Seeder.cs` 28,
`Validators?.cs` 29, `Module.cs` 20, `AppHost*/Program.cs` 6). Every `IsTestProject=true` project resolves
a tier under the new gate and none trips the host-package or Shouldly error, so the gate does not break
the build.

Security layer ran (the range touches `api/Concertable.Payment/**` and both harnesses' hook config). The
Payment changes are one import line, the merged platform pin and markdown stubs; the hook config adds a
`PreToolUse` command running a repo-local Python file, the same shape as the existing Stop hook. No authz
rule, secret, credential or workflow changed. Nothing to report.

Native-catalog layer (correctness/reuse/simplification/efficiency/error-handling) was applied inline over
the executable changes — `TestConventions.targets`, `skill_router.py`, `docs_reachability.py` and the hook
wiring — rather than via the `code-reviewer` subagent, per this session's standing directive not to call
the Agent tool. ENF1 and ENF2 below are native-layer correctness findings.

All three fixed on the branch. Verified after the fixes: `agent-standards` **26 passed** (was 20, and the
four new Codex tests fail against the old router — checked by running them against it), Concertable hook
suite **94 passed / 13 subtests** (was 92/4), `docs_reachability.py` **0 errors / 23 warnings**,
`plan_graph.py` **0 errors**, `vendor-hooks -Check` and `sync-generated -Check` clean, and tier resolution
plus a clean build re-checked for a unit and an integration project through different
`Directory.Build.targets` chains.

- [x] **ENF1 — HIGH — correctness: the Codex half of the router is inert, and the test that guards it
  cannot see that** — `.codex/hooks.json:6`, `.agents/hooks/skill_router.py:45`, `:200`
  The Codex `PreToolUse` matcher is `"Write|Edit|MultiEdit|NotebookEdit|apply_patch"`, but the router's
  `WRITE_TOOLS = {"Write", "Edit", "MultiEdit", "NotebookEdit"}` — so `main()` hits
  `if data.get("tool_name") not in WRITE_TOOLS: sys.exit(0)` and allows every Codex write. `apply_patch`
  appears nowhere in the router, here or in `agent-standards`. Two further breaks sit behind that one: an
  `apply_patch` payload carries no `file_path`/`notebook_path`, so `target` would be falsy and exit 0
  anyway, and the routed path lives inside the patch body (`*** Update File: …`), which the router never
  parses. This is not speculative about Codex's vocabulary — the sibling hook in this same repo already
  handles it: `plan_handoff_stop.py:37` lists `apply_patch`/`edit_file`/`write_file`/lowercase variants,
  `:48` reads `path`/`filepath`, and `:49` parses `*** Add|Update|Delete File:` out of the patch.
  `test_vendored_hooks.py:43` asserts only that the string `skill_router.py` appears in both wiring files,
  so it passes while certifying exactly the claim that is false — and `docs/INDEX.md` ("wired in
  `.claude/settings.json` and `.codex/hooks.json`") and `.agents/README.md` ("one harness only is the
  defect") both state that claim to the reader. Note this also makes Next Step 2 moot as written:
  approving the hook in Codex approves an inert hook. Fix upstream in `agent-standards` (add Codex's tool
  names to `WRITE_TOOLS`, derive targets from the patch body and the `path`/`filepath` keys, mirroring
  `plan_handoff_stop.py`), re-vendor, and add a Codex-payload test — `test_non_write_tool_is_ignored`
  currently asserts the wrong side of this for `apply_patch`.
  **Fixed** in `agent-standards` `268796e` (+ `88cf091` regenerating the plugin payload), re-vendored
  here: `WRITE_TOOLS` is now lowercased and carries Codex's names, `written_targets` reads
  `path`/`filepath` and parses `*** Add|Update|Delete File:` out of the patch, deny patterns match only
  **added** lines so a patch that deletes a violation is not blocked by it, and `repo_relative` resolves
  against the payload's cwd because patch paths are session-relative. Four upstream tests cover the Codex
  payload, a lowercase tool name, a multi-file patch and the deletion case; one test here pins this repo's
  own table under a Codex payload. `test_vendored_hooks.py` gained
  `test_every_wired_tool_name_is_one_the_hook_acts_on`, which asserts every tool in each wiring file's
  matcher is one the hook's own vocabulary accepts — the check that would have caught this, instead of
  the filename presence that could not.

- [x] **ENF2 — MEDIUM — correctness: the unit-tier source backstop covers 2 of the 6 host families the
  package check names, so the build tier is weaker than the hook tier** — `api/BannedSymbols.UnitTests.txt`,
  `api/TestConventions.targets:17`, `:30`
  The targets file states the source list exists "because the symbol can arrive through a transitive
  reference", but the `@(PackageReference)` check at `:30` names six families (Mvc.Testing, TestHost,
  Respawn, `Testcontainers*`, `Microsoft.Playwright*`, `Reqnroll*`) while the banned list closes only
  `WebApplicationFactory`, `TestServer` and `WebApplication.Create*`. A unit-test project that reaches
  Respawn, Playwright, Testcontainers or the Aspire testing host through a `ProjectReference` passes both
  gates. That shape is live in the tree today, not hypothetical:
  `Concertable.Payment.E2ETests.Helpers.UnitTests` is Unit tier and project-references
  `Concertable.Payment.E2ETests.Helpers`, which carries `Respawn`, `Microsoft.Playwright` and
  `Aspire.Hosting.Testing`; the same is reachable from any unit project referencing a
  `*.IntegrationTests.Fixtures`. `.agents/skill-routes.json` already denies `Testcontainers|Respawn` at
  write time, so the tier the router's own docstring calls "the tier that guarantees" is the weaker of the
  two for four of six families. Add the fingerprints actually used in this repo: `T:Respawn.Respawner`
  (14 uses), `T:Aspire.Hosting.Testing.DistributedApplicationTestingBuilder` (18),
  `T:Microsoft.Playwright.IPlaywright`, `T:Testcontainers.MsSql.MsSqlBuilder`.
  **Fixed** — those four plus `T:Microsoft.Playwright.Playwright` and `T:Reqnroll.BindingAttribute`,
  every type name and namespace first confirmed present in its assembly, then proved end-to-end: a probe
  calling `Respawner.CreateAsync` inside `Concertable.Payment.E2ETests.Helpers.UnitTests` now fails the
  build with RS0030 and the new message, and the project builds clean once the probe is removed.
  Two corrections to what this finding first claimed, both worth recording because each would have
  landed a wrong change: the qualified filename `BannedSymbols.UnitTests.txt` **is** read by
  BannedApiAnalyzers 3.3.4 — an intermediate run suggested otherwise and prompted a rename into a
  same-named file in its own directory, which was reverted once the run proved confounded (the first
  probe failed in `Concertable.Testing.E2E` and aborted the build before reaching the unit project).
  And RS0030 fires on a symbol being **used**, not on a type named in a member signature, so the
  original four entries were never inert.

- [x] **ENF3 — LOW — nine verbatim copies of a design-narration comment, one of them self-contradictory** —
  `api/Directory.Build.targets:4` and the eight service/test `Directory.Build.targets` import sites
  The same three-line comment ("MSBuild auto-imports only the FIRST `Directory.Build.targets` … so this
  file shadows `api/Directory.Build.targets` for everything beneath it") is pasted at every import site.
  In `api/Directory.Build.targets` it is false on its face — that file *is* `api/Directory.Build.targets`,
  so it shadows nothing. Root `AGENTS.md` "Code comments" disqualifies a comment that restates reasoning
  already stated elsewhere ("two copies drift the day one changes"), and nine copies of a why in a PR whose
  thesis is one-rule-one-home is the disease it is treating. Keep the explanation once, in
  `TestConventions.targets`' own header where the reader already is, and drop it from the nine import sites.
  **Fixed** — 53 lines deleted, imports untouched; tier resolution re-verified through both a service
  chain and a `tests/` chain afterwards.

- [x] **ENF4 — MEDIUM — a BOM made 18 Claude skill stubs unroutable, and the generator could not see it** —
  `.claude/skills/*/SKILL.md`, `.agents/sync-claude-skill-stubs.ps1:40`
  Found while verifying the Phase 6a deployment, not by reading the diff: the harness listed 18 repo-local
  skills with `---` as their description. Each of those files begins with a UTF-8 BOM before the `---`,
  which stops the frontmatter parsing, so `name` and `description` read as empty. The BOM set and the
  undescribed set matched exactly — 18 and 18 — including `merge`, `incremental-review`, `pr-preflight`,
  all four `e2e-*` debug skills, `commit`, `push`, `pull`, `integration-debug` and `address-review`.
  The generator was not the source (it writes UTF8-no-BOM) but could not heal it either: it compares with
  `ReadAllText`, which **strips** the BOM, so every broken stub read back as identical and was reported
  "unchanged" on every run. It now compares the raw bytes for the BOM specifically. Regenerating cleared
  all 18 and created the two stubs that never existed (`update-roadmap`, `techdebt`), 26 → 28 — closing
  the "Claude Code can't see `update-roadmap`" gap the ledger had recorded separately.
  Same failure class as the colon-space truncation this corpus already hit once, and the same signature:
  nothing visibly wrong, the skill simply never loads.

- [x] **ENF5 — HIGH — the controller-visibility rule was backwards, and the wrong version had already
  reached the shared repo** — `api/agents/MODULE_STRUCTURE.md:26`, `dotagents module-structure/SKILL.md:22`
  Found during Tommy's organization review, by checking the rule against the code rather than reading the
  diff. Both docs stated `*.Api` controllers are `public` (the repo one citing a 2026-04-25 revert from
  internal). The code: **36 internal, 3 public** of 40 `*Controller.cs` — the rule described 3 of 39. Worse,
  it had been promoted verbatim into `dotagents`, so it would have taught every .NET repo the wrong default.
  Ruled by Tommy: controllers are generally `internal`.
  Stating `internal` alone would have been worse than the original error, because ASP.NET's default
  `ControllerFeatureProvider.IsController` requires a public type — follow it without a custom provider and
  the routes silently do not exist. This repo has one: `InternalControllerFeatureProvider`
  (`Concertable.Shared.Api/Controllers/`), wired by `ControllerBuilderExtensions`. So the generic skill now
  carries the rule *and* the mechanism it depends on (`dotagents` `3918a85`), and the repo doc carries the
  inventory: the provider, and the three deliberate exceptions (`BlobController`, `FallbackController`,
  `GenreController`).
  **Still open, and Tommy's:** `GenreController` lives in `api/Concertable.Shared/src/Concertable.Shared.Api/`,
  while the skill's layer table says a shared library exposes no HTTP. Either the rule needs a stated
  exception or that controller is misplaced.

- [x] **ENF6 — MEDIUM — the repo half restated five rules the skills already own** —
  `api/agents/MODULE_STRUCTURE.md`
  Meta-rule 1 ("One rule, one home... a second copy is a bug, not emphasis") broken inside the PR that
  establishes it. Verified by probe, not by eye: "same per-layer split", "no shared reference `DbContext`",
  "an enum, not a table", "controller ownership follows the resource's domain module" and "a prefix alone
  does not justify a wrapper" each appeared in both the repo doc and `module-structure`/`http-api`. The
  skill copies are the keepers; the repo copies are cut, leaving only the inventory those rules resolve to
  here (the deleted `ReadDbContext`, `Genre` in `Concertable.Contracts`). 53 -> 48 lines, all six probes now 0.
  The generic file needed no change on this count — it carries no Concertable identifier at all, using
  `WarehouseId` as a deliberately foreign example, so meta-rule 2 holds and extraction stays a `git mv`.
