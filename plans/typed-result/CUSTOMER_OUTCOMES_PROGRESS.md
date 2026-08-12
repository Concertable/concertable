# Customer non-Payment outcomes and lookups progress

- Plan: `plans/typed-result/CUSTOMER_OUTCOMES_PLAN.md`
- Roadmap: `plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md`
- Roadmap item: `typed-result/customer-outcomes`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\typed-result_customer-outcomes`
- Branch: `Feature/typed-result_customer-outcomes`
- PR: [#425](https://github.com/Concertable/concertable/pull/425) - open, non-draft, remote head
  `297c61192117d14e631c5ad5f64364e28ed670db`
- Dependency/package gates: NuGet.org publishes exact `0.1.0-alpha.2` packages for `Reunion`,
  `Reunion.Validation`, `Reunion.Errors`, and `Reunion.AspNetCore`. The separate
  `REUNION_ALPHA2_BASELINE` workstream owns the repository-wide pin cutover; this service workstream
  may independently prepare its Customer-owned alpha.2 code. Customer Ticket PR #475 and its
  platform-sync PR #479 are terminal on platform `.910` and remain outside this scope.
- Last reconciled: 2026-08-12 after the reviewed work range `e60219f7d..297c61192` was pushed;
  local work head, remote-tracking branch, and PR #425 head all verified as `297c61192`

## Current state

Phases 1-6 are implemented and locally reviewed. Review, Preference, User, Venue, and Artist now use
operation-owned Results, structured Review validation, application/service Options for ordinary
absence, and materialized `IReadOnlyList<T>` query results while preserving their existing HTTP and
event contracts. Atomic Review and Preference creates translate only the existing unique-key races
to their typed conflicts; collaborator, provider, identity, invariant, and cancellation failures
remain exceptions.

The reviewed work head `297c61192` is current with `origin/main` `b94028d3f` at platform `.939`
and 60 commits ahead of it. The current-main merge was automatic in Customer code and plan state.
The branch still owns the same five-module semantic slice; Ticket, Concert, Customer Payment,
purchase/checkout, shared Kernel API, events, models, and migrations are excluded.

Phase 7 implementation and local verification are green. All four existing Customer Reunion-family
pins are `0.1.0-alpha.2`; branch-owned Review and Preference construction sites use raw payload
conversions only where their target types keep success/error intent explicit. Named Dunet cases,
validation factories, and nullable-to-Option boundaries remain explicit where inference would obscure
the owned contract. No project package reference was added or removed.

The complete Release solution, five scoped unit suites, Shared.Api architecture suite, isolated
Customer carve, package/resolved-graph inventories, structural audits, Docker health preflight, and
five integration wrappers are green. The incremental native, security, architecture, convention, and
coverage review is clean through `d623a3501`; `CV4` was fixed by qualifying five inherited
`Query` calls and the affected Release unit suites pass 25/25. The reviewed work head is published
to PR #425 with exact local, remote-tracking, and PR OID equality.

## Next Steps

1. Run `/merge` for PR #425 from the final pushed checkpoint head. Require the plan's full E2E queue
   tier, follow the source PR and generated platform-sync PR to terminal green/merged state, then
   perform plan-managed close-out.

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
  review watermarks are `d623a35014cd23632f190e557ee37668953680b9`; the follow-up review of the
  `CV4` fix is clean.
- Push evidence: starting remote head `e60219f7dfe13f0c49c818e2ed7ab7a557f84569`; reviewed work
  head and resulting remote-tracking/PR head `297c61192117d14e631c5ad5f64364e28ed670db`.

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
