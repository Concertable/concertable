# Customer non-Payment outcomes and lookups progress

- Plan: `plans/typed-result/CUSTOMER_OUTCOMES_PLAN.md`
- Roadmap: `plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md`
- Roadmap item: `typed-result/customer-outcomes`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\typed-result_customer-outcomes`
- Branch: `Feature/typed-result_customer-outcomes`
- PR: [#425](https://github.com/Concertable/concertable/pull/425) - open, non-draft, remote head
  `306cab6de966ee74688c64b47f2eee7027a4a5e4`; verified local delivery head is based on
  `origin/main` `5bf622fecd600868b4ec437daf6c6ad0389029a6` through merge `6f3a8c4f3`.
- Dependency/package gates: NuGet.org accepted and its official v3 flat-container indexes listed
  `0.1.0-alpha.3` for `Reunion`, `Reunion.AspNetCore`, `Reunion.Errors`, and `Reunion.Validation`
  together at 2026-08-12 17:09:18Z. Alpha.3 was packed from Reunion merged `master` commit
  `91fdc6f2e33d8f396fa463ad309cb1288bea3be5`, the squash merge of PR #8 whose reviewed source head
  was `113be42f532d5d7e8daf1c362262ff7a7854b7bc`; every nuspec records `91fdc6f2e33d8f396fa463ad309cb1288bea3be5`
  as its repository commit. Both `net10.0` and `net11.0` `Reunion.AspNetCore` assemblies expose the
  minimal-API `ToOkOr` overloads with generic/parameter arities `2/2` and `3/3`, and the MVC overloads
  with arities `1/2` and `2/3`. Publication and published-baseline revalidation are terminal. Customer
  Ticket PR #475 and platform-sync PR #479 remain terminal and out of scope.
- Last reconciled: 2026-08-12 after current-main `.955` reconciliation. Merge
  `132f3a568fb6eee4e2635130ab016fd1b6ebdd3d` brings `origin/main` `58e19d938` into the branch;
  the 188-project Release graph builds with 0 errors and the incremental review is clean. Republish
  the current reviewed head through the two-leg push before queueing PR #425.

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
pins are now `0.1.0-alpha.3`; branch-owned Review and Preference construction sites use raw payload
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
The exact official baseline is the four-package `0.1.0-alpha.3` graph from Reunion merge
`91fdc6f2e33d8f396fa463ad309cb1288bea3be5`; the prior local artifact remains historical verification
evidence only. The synchronized candidate has passed against official alpha.3 and is ready for its
required incremental review.

## Next Steps

Deliver the reviewed Phase 8 candidate into its terminal state:

1. Republish the current-main reviewed head through the plan-managed two-leg push and prove local,
   remote-tracking, and PR #425 head equality.
2. Route PR #425 through `/merge` with the required full merge-queue E2E tier, follow it to its exact
   merge SHA, then own the generated platform-sync PR through green/merged before close-out. Keep the
   plan and ledger until all delivery gates are terminal.

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
  was validated against the exact local `113be42` artifact.
- Reunion PR #8 merged as `91fdc6f2e33d8f396fa463ad309cb1288bea3be5`; all four
  `0.1.0-alpha.3` packages were inspected, published, and listed by official NuGet v3 metadata. The
  publication blocker is cleared; published-baseline revalidation and PR #425 delivery remain.
- Customer non-Payment is delivery-ready against the exact `113be42` artifact. Its owned five-module
  production scope has no remaining old carrier, old terminal, or third-party functional dependency;
  that readiness evidence is reconciled into `REUNION_SHARED_CONTRACTION_PROGRESS.md`.

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
  review watermarks are `132f3a568fb6eee4e2635130ab016fd1b6ebdd3d`; the current-main `.955`
  incremental native, security, architecture, convention, and coverage review found no new issues.
- Push evidence: starting remote head `e60219f7dfe13f0c49c818e2ed7ab7a557f84569`; reviewed work
  head `297c61192117d14e631c5ad5f64364e28ed670db`; the later checkpoint transport made the current
  remote-tracking/PR head `08a92f91659e4eccc4558e55def5027b26c08348`. Alpha.3 push leg one
  advanced local, remote-tracking, and PR #425 together to reviewed head
  `306cab6de966ee74688c64b47f2eee7027a4a5e4`.
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
- Alpha.3 release evidence: all four packed nuspecs embed merged repository commit
  `91fdc6f2e33d8f396fa463ad309cb1288bea3be5`; both target frameworks expose the expected flexible
  `ToOkOr` surfaces. Uploaded-file SHA-256: Reunion
  `04084C484EB4A6B3EC0D23E5EF50F9D297DA3A653A22AC157A0873297BC7D943`;
  Reunion.AspNetCore `C6CFE2B28E10C86EEC89AA3726E253556EC45B2431823DA2D3759DD709C46EB0`;
  Reunion.Errors `53D0B558375D7E98223CC82779AB5A73551B6380A738EA7A9BF031DE0297729E`;
  Reunion.Validation `5319FD405FAF7C3D7073CF34AF50FAF53BB5508188A1B9773D4851132F58B472`.
  Official v3 flat-container indexes listed alpha.3 for all four packages together at
  2026-08-12 17:09:18Z.
- Alpha.3 published-baseline verification on synchronized main: the five changed API closures build
  with 0 warnings/errors; affected units pass 80/80; Shared.Api architecture passes 60/60; the Release
  solution builds with 0 errors and 2 unrelated generated B2B UI warnings; the isolated 36-project
  Customer carve builds with 0 errors and 1 existing warning. Docker's fresh-container HTTP data path
  passed. The planned eight integration projects pass 74/74: Customer Review 12/12, Preference 7/7,
  User 6/6, Venue 2/2, Artist 2/2, plus B2B User 3/3, Venue 25/25, and Artist 17/17. Customer Concert
  11/11 and Ticket 25/25 also pass. The long worktree hit the known 261-character SqlClient native DLL
  path in Customer User; the same project passed through a temporary short drive mapping with no
  tracked workaround. Package, carrier, terminal, rename, excluded-scope, local-workaround, and
  whitespace inventories are clean.
- Current review state: all implementation work is clean through `132f3a568`; no findings remain open.

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
  Official `0.1.0-alpha.3` from merged source `91fdc6f2e33d8f396fa463ad309cb1288bea3be5`
  is the only delivery baseline; never commit a local feed, temporary version, or machine-specific
  restore path.
