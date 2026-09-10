# Documentation review — Docs/B2BCarveReconciliation

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `6722c0603817e8b33a2451c5b5b332759a14331f`  `(2026-09-10)`
**Judgment:** `approved`

Checkpoint 10A of [`REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`](../plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md).
Meta-only: one plan file. Records the `b2b` extraction rehearsal beside the existing `auth`,
`payment`/`search` and `customer` ones, and refreshes the 10A status table's `b2b` row.

## Review pass — 2026-09-10 — docs

**Candidate base:** `e0b2324629b0b6f0f5cbb28eb4c2b48b8dbd0a3f`
**Candidate head:** `7f9429665` → remediated at `6722c0603`
**Candidate branch:** `Docs/B2BCarveReconciliation`
**Candidate scope:** `all`
**Candidate path-set:** `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md` `(1 path)`
**Work-order path:** `reviews/Docs-B2BCarveReconciliation.md`
**Work-order mode:** `new`
**Pass judgment:** `changes-requested`

Scope guard: the single frozen path is `plans/**`, so no runtime, package manifest, migration or CI
test-selection path is in the candidate and the docs route is correct. Not a pure close-out — the net
diff is insertions plus two edited lines — so the lenses below apply. Routed rules: `plans`,
`docs-and-debt`.

Lenses run: accuracy, contradiction, one-rule-one-home, recurring-context concision, dangling
references, followability. `docs_reachability.py` not run: no agent-instruction path changed.

### Findings

- [x] **ACC1 — MEDIUM — accuracy** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md:1066`
  The CI bullet said b2b's 33 EF migration sets sit across "ten module `Infrastructure` projects".
  `git ls-files | grep /Migrations/` in the extraction resolves to **eleven** — Admin, Application,
  Artist, Booking, Concert, Conversations, Deal, Opportunity, Tenant, User and Venue. Fixed in
  `6722c06`.

- [x] **ACC2 — MEDIUM — accuracy** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md:1078`
  The tier-gate bullet compared b2b's 29 test projects against "`auth`'s 2". `Concertable/auth` has
  four test-named projects and three that declare a tier — `ArchitectureTests`, `IntegrationTests`,
  `UnitTests` — with `IntegrationTests.Fixtures` a support library. The comparison also mixed units:
  b2b's 29 is a tier-declaring count. Fixed in `6722c06`: both sides now say tier-declaring, and the
  auth figure is three.

- [x] **ACC3 — MEDIUM — accuracy** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md:1011`
  The opening claimed 1516 warnings "all of them Meziantou style rules". The warnings-only file log
  classifies **1514** of the 1516 by code; two are unattributed, so "all" overstated what was
  measured. Fixed in `6722c06`, which also records that none is `MSB3277` — the contrast with `auth`'s
  246 is the part another track can use.

- [x] **CON1 — MEDIUM — contradiction** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md:1013`
  The section called b2b "the only carve with a frontend and an admin surface". The `customer`
  rehearsal section directly above describes that target's `app/web`, `app/mobile` and `app/shared`
  workspaces, so the frontend half is false; `eng/repository-split/map.yaml`'s own b2b note is the
  narrower true claim ("The admin console is B2B-exclusive"). Fixed in `6722c06`: second carve with a
  frontend, only one with an admin surface.

- [x] **CON2 — HIGH — contradiction** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md:1168`
  The 10A status table's `b2b` row still read "statically reconciled; 1970 files … green build needs
  disk", which the new section directly contradicts by reporting an executed green Release build at
  1972 files. A reader reaching the table first would take the stale row as current. Fixed in
  `6722c06`: the row now records the green build, the 1972-file count and the remaining action, and
  the pointer line below it no longer says both rehearsals are "pending consolidation" now that they
  are consolidated.

Verified against repository evidence rather than the run's own notes: `Directory.Packages.props:7` in
the extraction pins `ConcertablePaymentVersion` to `0.1.0-alpha.0.1330` and feeds
`Concertable.Payment.{Hosting,Client,Contracts,TestKit}`; the published `0.1.0-alpha.0.1370` and
`1371` both carry `AllowInsecureHttpClientEnvironmentVariable`, `HttpPort` and `GrpcPort` while `1330`
carries none, checked by the member name in the `#Strings` heap; all 426 `ProjectReference` entries
across the 109 `.csproj` resolve post-extraction bar one build-time MSBuild wildcard; the `.slnx` has
119 entries of which exactly 12 do not resolve; `map.yaml` renames `app/web/admin/` to itself while
promoting `app/web/b2b/`; the extraction's only container-publishable projects are `b2b-web`,
`b2b-workers` and `b2b-seeding-simulator`, and it holds no `*.Migrations.csproj`; none of `customer`'s
five CI `.ps1` scripts exists in it.

No findings from one-rule-one-home (the rehearsal record belongs in the plan that owns the other
three, and the pass-two no-op finding was deliberately left to the `customer` section that owns it
rather than restated), concision (the plan is not recurring context), dangling references (the two
`~/.claude/plans/` pointers follow the precedent already set at the same location for the `customer`
run), or followability.
