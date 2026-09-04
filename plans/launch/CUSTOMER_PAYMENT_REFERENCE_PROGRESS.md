# Customer payment-reference migration progress

- Plan: `plans/launch/CUSTOMER_PAYMENT_REFERENCE_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/customer-payment-reference`
- Worktree: not created (open as `Feature/launch_customer-payment-reference` when work starts)
- Branch: not created
- PR: not opened
- Dependency/package gates: the Payment Contracts and Client packages from PR #933
  (`launch/payment-operation-ownership`) must be published and the Customer platform pin advanced
  before Delivery item 1 can complete; local preparation against exact local producer artifacts
  (`./scripts/local-platform.ps1 prepare` in the producer worktree) is permitted, delivery-gated.
- Last reconciled: 2026-09-04 at plan authoring

## Current state

Plan authored; no implementation started. The flaw is verified in code:
`TicketPurchaseParams.PaymentMethodId` → `Ticket.Infrastructure/Services/TicketService.cs:76` →
`ICustomerPaymentClient.PayAsync(..., paymentMethodId)`, relayed from the customer web and mobile
surfaces via `app/customer/shared/src/features/tickets/types.ts`. Rationale and the boundary
verdict: `plans/launch/PAYMENT_BOUNDARY_DECISION.md` §1.

## Next Steps

1. After the Payment packages publish (PR #933) and the pin advances: open the worktree, execute
   Delivery items 1–4 in order, run the review workflow, and deliver through merge and platform
   sync.

## Completed work

- Authored the plan and ledger (2026-09-04).

## Verification

- None yet; the branch does not exist.

## Reviews

- No review yet; the branch does not exist.

## Decisions, discoveries, blockers, and deviations

- On-session purchase collects no mandate; saved-card selection, if ever built, is Payment's
  surface per `PAYMENT_BOUNDARY_DECISION.md` §1 — Customer proxies at most an opaque token and
  persists nothing.
- Owner decision, 2026-09-04: PR #933 includes the legacy cull and breaking vocabulary pass, so
  Customer migrates directly from the raw-identifier client to the final reference surface after
  that package publishes; there is no later producer-cull dependency.
