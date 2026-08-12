# Customer non-Payment outcomes and lookups progress

- Plan: `plans/typed-result/CUSTOMER_OUTCOMES_PLAN.md`
- Roadmap: `plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md`
- Roadmap item: `typed-result/customer-outcomes`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\typed-result_customer-outcomes`
- Branch: `Feature/typed-result_customer-outcomes`
- PR: [#425](https://github.com/Concertable/concertable/pull/425) - open, non-draft, remote head
  `08a92f91659e4eccc4558e55def5027b26c08348`
- Dependency/package gates: NuGet.org publishes only `Reunion.AspNetCore` `0.1.0-alpha.1` and
  `0.1.0-alpha.2`. Published alpha.2 embeds commit `ab3386a76e83b057bc9498ebc2d7d31be5f62626`
  and does not contain `ToOkOr`; no published package contains flexible Option terminals from
  `113be42f532d5d7e8daf1c362262ff7a7854b7bc`. Phase 8 is implementable and locally verified against
  an exact artifact, but delivery remains gated on a published Reunion-family version containing
  `113be42`. Customer Ticket PR #475 and platform-sync PR #479 remain terminal and out of scope.
- Last reconciled: 2026-08-12 after the clean Phase 8 incremental review through local semantic head
  `9bcb25eea`; PR #425 is not merge-ready and must not be pushed or merged until the publication gate
  and published-baseline verification complete.

## Current state

Phases 1-6 are implemented and locally reviewed. Review, Preference, User, Venue, and Artist now use
operation-owned Results, structured Review validation, application/service Options for ordinary
absence, and materialized `IReadOnlyList<T>` query results while preserving their existing HTTP and
event contracts. Atomic Review and Preference creates translate only the existing unique-key races
to their typed conflicts; collaborator, provider, identity, invariant, and cancellation failures
remain exceptions.

The prior reviewed work head was `297c61192`; PR #425's current remote checkpoint is `08a92f916`.
Local merge `f94eeec09` brought platform `.943` from `origin/main` before Phase 8, and local merge
`9bcb25eea` brought current docs-only `origin/main` `e10fd17fa` in before review. The branch still owns
the same five-module semantic slice; Ticket, Concert, Customer Payment, purchase/checkout, shared
Kernel API, events, models, and migrations are excluded.

Phase 7 implementation and local verification are green. All four existing Customer Reunion-family
pins are `0.1.0-alpha.2`; branch-owned Review and Preference construction sites use raw payload
conversions only where their target types keep success/error intent explicit. Named Dunet cases,
validation factories, and nullable-to-Option boundaries remain explicit where inference would obscure
the owned contract.

The prior alpha.2 work remains published to PR #425 through remote checkpoint `08a92f916`. The full
local range through synchronized Phase 8 semantic head `9bcb25eea` has now passed incremental native,
security, architecture, convention, and coverage review with no new findings.

Phase 8 adopts the flexible Option terminals from exact Reunion commit `113be42`: projected
Artist/Venue `ToOkOr`, User's direct unauthorized alternative, and target-typed Artist/Venue nullable
repository conversions. User.Api replaces its direct `Reunion` package ownership with
`Reunion.AspNetCore`, which supplies both the Option type transitively and the MVC terminal. Review
and Preference now name their existing immediate duplicate-aware primitive `InsertAsync` everywhere
without changing its add/save/duplicate-only behavior. The two custom Created Result mappings are
unchanged. Shared generic DataAccess was not extended; its future published-package standardization
is recorded in `api/Concertable.DataAccess/TECH_DEBT.md`.

The Phase 8 candidate is locally verified and reviewed against exact gitignored packages under
`artifacts/reunion-113be42` with version `0.1.0-local.113be42`. The temporary NuGet config and local
version pins were removed from tracked state after verification. This checkpoint is not published to
PR #425.

## Next Steps

1. Query official package metadata for the exact published Reunion-family version containing
   `113be42`; do not guess. Do not push or merge PR #425.
2. When it exists, pin the whole family to that version, rerun the affected closures,
   unit/integration suites, package/structural audits, and incremental review against the published
   baseline before the plan-managed two-leg PR update.

## Downstream handoffs

- Waiting ledger: `plans/typed-result/REUNION_SHARED_CONTRACTION_PROGRESS.md`.
  Gate: Customer non-Payment must be delivery-ready and identify every remaining old carrier,
  terminal, and third-party dependency outside its owned scope.

## Completed milestones

- Phases 1-4 delivered Review create outcomes, Preference outcomes/Options/lists, User Option/list
  normalization, and Venue/Artist Options with module-owned unit and integration coverage.
- Phase 5 migrated the five-module slice to direct Reunion packages and terminals, completed the
  scope audit, fixed review findings `CV1`-`CV3`, and opened PR #425 through remote checkpoint
  `e60219f7d`.
- Phase 6 added direct `Reunion.Validation` ownership, structured every custom Review DI validator,
  moved star-range rejection into the typed domain factory, and preserved public 201/400/404/409 and
  capability behavior in `5cfdb9427`.
- Full-review findings `NAT1`-`NAT3` were fixed by direct Review error-package ownership and atomic
  Preference/Review inserts in `cfe0667bf`, `45fb3008b`, and `ad9f4a801`; focused integration passed
  and the incremental review through `958c05c5a` was clean.
- Current main `aab321bd2` was merged as `c021d26c9`; its incremental native, security,
  architecture, convention, and coverage review was clean.
- The branch was refreshed to `origin/main` `de80debea` as `7ce4ed10d` before Phase 7; no Customer
  conflict required a semantic resolution.
- Phase 7 aligned the Customer Reunion-family graph to alpha.2 in `47c9ba547`, reconciled platform
  `.939` in `a3c1c1420`, recorded final verification in `22fb61697`, and fixed review finding
  `CV4` in `d623a3501`. The reviewed work range `e60219f7d..297c61192` was pushed to PR #425
  with local, remote-tracking, and PR head equality.
- Phase 8 implemented flexible Option terminals, direct `InsertAsync` naming, nullable-to-Option
  simplifications, Result guidance, and DataAccess debt in `5ee10d11a`; the incremental native,
  security, architecture, convention, and coverage review through `9bcb25eea` was clean. Delivery
  remains gated on publication of a Reunion-family package containing `113be42`.

## Verification and review

- Last verified semantic head `c021d26c9`: Release solution 0 errors; scoped units 80/80;
  Shared.Api 60/60; isolated 36-project Customer carve 0 errors; package, carrier, validation,
  excluded-path, and whitespace inventories clean.
- Last complete Docker-backed candidate before the latest base merge: the five module wrappers passed
  74/74 across Customer Review 12/12, Preference 7/7, User 6/6, Venue 2/2, Artist 2/2 plus the matching
  B2B User 3/3, Venue 25/25, and Artist 17/17 projects. This evidence is historical; Phase 7 reruns it
  on current main.
- Final `.939` Docker-independent candidate: all resolved Customer assets contain Reunion-family
  alpha.2 and no alpha.1; scoped units pass 80/80; Shared.Api passes 60/60; the complete Release
  solution builds with 0 errors and 4 existing warnings; the isolated 36-project Customer carve
  builds with 0 errors and 1 existing warning; structural and whitespace audits pass.
- Final `.939` Docker-backed candidate: `scripts/docker-health.ps1` passed its fresh-container HTTP data
  round trip. Review 12/12, Preference 7/7, B2B/Customer User 3/3 + 6/6, B2B/Customer Venue 25/25 +
  2/2, and B2B/Customer Artist 17/17 + 2/2 passed: 74/74 across eight projects. Every run-owned SQL
  and Ryuk container was removed; a separate session started a new pair after this run completed.
- Review artifact: `reviews/Feature-typed-result_customer-outcomes.md`. All findings are fixed. Both
  review watermarks are `9bcb25eea61794ca982b7f8c1a4f044c7b8f4514`; the Phase 8 incremental
  native, security, architecture, convention, and coverage review found no new issues.
- Push evidence: starting remote head `e60219f7dfe13f0c49c818e2ed7ab7a557f84569`; reviewed work
  head `297c61192117d14e631c5ad5f64364e28ed670db`; the later checkpoint transport made the current
  remote-tracking/PR head `08a92f91659e4eccc4558e55def5027b26c08348`.
- Phase 8 exact-artifact evidence: all local nupkgs embed producer commit
  `113be42f532d5d7e8daf1c362262ff7a7854b7bc`. SHA-256: Reunion
  `9FADC33CD06F3B4A9A92564633E01007CC81EA091AA9F257D821532E046E10CE`;
  Reunion.AspNetCore `5BCE01783D79B99F60FB1F848560B04563169C9346A84CF02815E483A5E8767C`;
  Reunion.Errors `5FB87198717D8A6F4C62226E2E79784F10C81AEE2133DB0DA1D4DABAF0A55BF5`;
  Reunion.Validation `1FBCBD656725D35C575A2A3CE0C84BED41C99894EB3D0A198D22D68370CD77F8`.
- Phase 8 Release verification against `0.1.0-local.113be42`: Artist, Venue, User, Preference, and
  Review API closures build with 0 errors; affected unit suites pass 80/80; Shared.Api architecture
  passes 60/60; Docker fresh-container HTTP data-path preflight passes; the five wrappers pass 74/74
  across eight projects. Resolved Customer assets contain only the exact local Reunion-family version.
  Rename, shared-DataAccess, terminal, Created-mapping, local-workaround, and whitespace audits pass.
- Current review state: all local work is clean through `9bcb25eea`; no findings remain open.

## Decisions and constraints

- Functional carriers stop at HTTP/module terminals and never enter HTTP DTOs, integration events,
  EF models, or persistence contracts. Repository single-item lookups remain nullable.
- Review and Preference duplicate conflicts are expected typed outcomes backed by their unique
  indexes; unrelated database/provider faults are not translated.
- `IReviewValidator` is the only custom DI validator in the five-module scope. FluentValidation and
  framework validators are separate contracts.
- Integration runs from this long worktree use short `--artifacts-path` roots to avoid the confirmed
  Windows native-loader path-length failure.
- No EF model changes exist, so migrations are not required. Local E2E is not duplicated; the merge
  workflow selects and runs the required queue tier.
- Published `Reunion.AspNetCore` alpha.2 is not a valid verification baseline for Phase 8: its nuspec
  embeds `ab3386a76e83b057bc9498ebc2d7d31be5f62626`, and its assembly exposes no `ToOkOr` symbol.
  Recreate the exact local graph from producer commit `113be42` when needed; never commit a local feed,
  temporary version, or machine-specific restore path.
