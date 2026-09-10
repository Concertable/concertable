# Reunion Shared contraction progress

- Plan: `plans/typed-result/REUNION_SHARED_CONTRACTION_PLAN.md`
- Roadmap: `plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md`
- Roadmap item: `typed-result/reunion-shared-contraction`
- Worktree: not created
- Branch: `Refactor/typed-result_reunion-shared-contraction`
- PR: not opened
- Dependency/package gates: implementation inventory waits only for the B2B preparation ledger;
  Payment, Auth, Customer Ticket, and Customer non-Payment are terminal
- Last reconciled: 2026-08-13 after Customer PR #425 and platform-sync PR #535 completed

## Current state

The reserved branch name above has not been created yet — implementation hasn't started. Payment, Auth,
and Customer Ticket are terminal. Customer non-Payment is terminal on published Reunion
`0.1.0-alpha.3` and platform `.963`; its five-module production scope has no remaining old carrier,
old terminal, or third-party functional dependency. Auth reports no old Kernel functional/error
carrier, Shared.Api terminal, third-party functional carrier, or legacy Reunion factory in its owned
scope. B2B
preparation is not yet delivery-ready. Search has no carrier conversion work. Starting contraction
now would still guess at the B2B surface.

## Next Steps

Blocked: The exact post-conversion Shared, messaging, and background-path inventory does not exist yet.
Blocked by: plans/typed-result/B2B_PROGRESS.md.
Unblock action: Complete and review the B2B local preparation ledger, then update this ledger with its remaining-call-site evidence.
Resume when: B2B is delivery-ready and its ledger identifies every remaining old carrier, terminal, and third-party dependency outside its owned scope.

## Completed work

- Reserved one authoritative final-contraction owner.
- Confirmed Search requires no independent Reunion conversion.
- Auth PR #517 and platform-sync PR #531 are terminal. Its owned source directly references `Reunion`
  and `Reunion.Errors` `0.1.0-alpha.2` and reports zero legacy carrier or terminal call sites.
- Recorded Customer Ticket and Customer non-Payment as terminal, with no remaining old carrier, old
  terminal, or third-party functional dependency in their owned production scopes. Customer PR #425
  merged as `9c3192066`; publication succeeded; platform-sync PR #535 merged as `8249fa5c9`.

## Verification

- Current source inventory found no Search functional carrier/error import.
- Auth inventory found zero imports or construction calls for the old Kernel functional/error
  carriers, Shared.Api result terminals, CSharpFunctionalExtensions, FluentResults, OneOf, ErrorOr,
  or LanguageExt. Auth does not require Reunion.Validation or Reunion.AspNetCore.
- Customer PR #425 passed full merge-queue API/UI E2E. Platform-sync PR #535 passed its build, carve,
  unit, and integration matrix and delivered `ConcertablePlatformVersion` `0.1.0-alpha.0.963`.

## Decisions, discoveries, blockers, and deviations

- Delivery order is not used to suppress independent consumer preparation.
- The contraction itself is implementation-blocked because its safe public surface depends on the
  prepared-consumer inventory, not merely on an unmerged PR.

## Downstream handoffs

- Repository cleanup and architecture enforcement resume after this plan is terminal.
