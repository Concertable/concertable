# Reunion Shared contraction progress

- Plan: `plans/typed-result/REUNION_SHARED_CONTRACTION_PLAN.md`
- Roadmap: `plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md`
- Roadmap item: `typed-result/reunion-shared-contraction`
- Worktree: not created
- Branch: `Refactor/typed-result_reunion-shared-contraction` (reserved)
- PR: not opened
- Dependency/package gates: implementation inventory waits for the four consumer preparation ledgers
- Last reconciled: 2026-08-12 after the Auth preparation handoff

## Current state

Payment is reviewed locally. Auth is delivery-ready and reports no old Kernel functional/error
carrier, Shared.Api terminal, third-party functional carrier, or legacy Reunion factory remaining in
its owned scope. B2B, Customer non-Payment, and Customer Ticket preparation are not yet complete.
Search has no carrier conversion work. Starting contraction now would still guess at the remaining
public surfaces and duplicate work.

## Next Steps

Blocked: The exact post-conversion Shared, messaging, and background-path inventory does not exist yet.
Blocked by: plans/typed-result/B2B_PROGRESS.md; plans/typed-result/CUSTOMER_OUTCOMES_PROGRESS.md; external owner `Feature/TypedResultMigrationPhase2`.
Unblock action: Complete and review the B2B, Customer non-Payment, and Customer Ticket local preparation ledgers, then update this ledger with their remaining-call-site evidence.
Resume when: The three remaining consumers are delivery-ready and their ledgers identify every remaining old carrier, terminal, and third-party dependency outside their owned scopes.

## Completed work

- Reserved one authoritative final-contraction owner.
- Confirmed Search requires no independent Reunion conversion.
- Auth preparation is reviewed and GREEN for PR delivery. Its owned source directly references
  `Reunion` and `Reunion.Errors` `0.1.0-alpha.2` and reports zero legacy carrier or terminal call sites.

## Verification

- Current source inventory found no Search functional carrier/error import.
- Auth inventory found zero imports or construction calls for the old Kernel functional/error
  carriers, Shared.Api result terminals, CSharpFunctionalExtensions, FluentResults, OneOf, ErrorOr,
  or LanguageExt. Auth does not require Reunion.Validation or Reunion.AspNetCore.

## Decisions, discoveries, blockers, and deviations

- Delivery order is not used to suppress independent consumer preparation.
- The contraction itself is implementation-blocked because its safe public surface depends on the
  prepared-consumer inventory, not merely on an unmerged PR.

## Downstream handoffs

- Repository cleanup and architecture enforcement resume after this plan is terminal.
