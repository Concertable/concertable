# Documentation review — Docs/Customer10ARehearsal

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `3c49f235167eca17889034278a9ebb6a874f286b`  `(2026-09-10)`
**Judgment:** `approved`

Checkpoint 10A of [`REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`](../plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md).
Meta-only: one plan file, no deletions. Records the `customer` extraction rehearsal beside the
existing `auth` and `payment`/`search` ones.

## Review pass — 2026-09-10 — docs

**Candidate base:** `d3bc6e1302bac349aa5885be76699e6bdae3c5c8`
**Candidate head:** `b8314cb5abf8f4e11a87bd7feefae8b1a258dbe3`
**Candidate branch:** `Docs/Customer10ARehearsal`
**Candidate scope:** `all`
**Candidate path-set:** `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md` `(1 path)`
**Work-order path:** `reviews/Docs-Customer10ARehearsal.md`
**Work-order mode:** `new`
**Pass judgment:** `changes-requested`

Scope guard: the single frozen path is `plans/**`, so no runtime, package manifest, migration or CI
test-selection path is in the candidate and the docs route is correct. Not a pure close-out — the net
diff is insertions only — so the lenses below apply. Routed rules: `plans`, `docs-and-debt`.

Lenses run: accuracy, contradiction, one-rule-one-home, recurring-context concision, dangling
references, followability. `docs_reachability.py` not run: no agent-instruction path changed.

### Findings

- [x] **ACC1 — MEDIUM — accuracy** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md:1001`
  The section closed "all 64 projects restore against the live feed and build Release with zero
  errors", which in this document reads as the same claim the `auth` and `payment` rehearsals make two
  sections above — and theirs were default builds. This one was not: a default per-project output
  layout exhausted the disk twice, and the green build needed `-m:1 -p:DebugType=none
  -p:OutDir=<shared>/`. Stating it unqualified overstates the evidence and hides the one piece of it
  another disk-starved track would actually reuse. Fixed in `3c49f23`: the flags, why they were
  needed, and the 1.9 GB → 178 MB figure now follow the claim.

- [x] **CON1 — MEDIUM — contradiction** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md:920`
  The `payment`/`search` section states "**Pass two leaves the solution file dangling.** It deletes the
  excluded projects and does not touch the `.slnx` that still lists them." The `customer` finding
  added below establishes that pass two deleted nothing on Windows in those same runs, so that
  sentence describes something neither rehearsal can have observed. Leaving both in the document
  without connecting them leaves a reader to trust whichever they reach first. Fixed in `3c49f23`: the
  earlier bullet now says to read it as reasoning rather than observation, and points at the finding
  that overturns it.

Verified against repository evidence rather than the handoff: `api/PlatformSourcePackages.targets`
exists at `api/`, carries the "a carve must NOT copy this file" comment, and is imported by
`api/Concertable.Customer/Directory.Build.targets:52` under `Condition="Exists(...)"`; the extraction's
AppHost class is `AppHost` where the target's is `CustomerAppHost`; the target's CI is
`dotnet restore/build/test/pack Concertable.Customer.slnx` at `.github/workflows/ci.yml:58-73`;
`payment` and `search` both declare `exclude` in `eng/repository-split/map.yaml`.

No findings from one-rule-one-home (the rehearsal record belongs in the plan that owns the other two),
concision (the plan is not recurring context), dangling references (every reference is to tracked
repository content), or followability.
