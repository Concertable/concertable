# Application Cancellation

Implements the 🔴 open MVP blocker in [LAUNCH_PLAN.md](LAUNCH_PLAN.md): *"Application cancellation
(separate — needs its own plan) — cancelling an artist's bid before it's booked (pre-money, no
refund). Partly covered today by withdraw/reject; a holistic path (incl. cancel from
`Accepted`/`PaymentFailed` pre-capture) is unaddressed."*

**Branch:** `Feature/ApplicationCancel`. Single PR for Phases 1–4 (all B2B + FE, no package-boundary
crossing). Phase 5 (optional hold-release) is its own Payment-first two-PR split — see the phase.

## Scope

**This plan cancels an *application*, not a *concert* — the sibling of the shipped concert-cancel
(PR #76), not a redo of it.**

- **Cancel an application (THIS plan).** An artist's bid on an opportunity **before it is booked**:
  the states `Applied`, `Accepted`, `PaymentFailed`. Mostly pre-money; where money exists it is an
  **unwind of the accept-leg payment** (escrow refund / hold release), never a settled-concert
  refund. The HTTP surface and HATEOAS actions are **application-shaped** — they live on
  `ApplicationController` / `ApplicationActions` (contrast: concert-cancel was deliberately
  concert-shaped on `ConcertController` / `ConcertActions`).
- **Cancel a concert (SHIPPED — do not touch).** `Booked → Cancelled` with escrow refund, venue SPA
  cancel action, API+UI E2E — done in PR #76. Everything from `Booked` onward stays on that path.
  `AwaitingSettlement` / `SettlementFailed` / `Complete` are also out of scope (settlement-recovery
  concerns, not application cancellation).

**NOT in this plan** (separate LAUNCH_PLAN Swim-lane C item, solicitor-gated): the *cancellation
policy matrix* — fees, cutoff windows, who-eats-the-Stripe-fee. Like PR #76, every unwind here is
**full refund, no fee**; the policy matrix later parameterises the amount.

## What exists today (verified in code — the gap is bigger than "extend withdraw")

- The state machine ([`ConcertWorkflowBuilder.WithApply`](../../api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Services/Workflow/ConcertWorkflowBuilder.cs))
  declares `Applied + Withdraw → Withdrawn` and `Applied + Reject → Rejected` for all four contract
  types — **but neither trigger is user-reachable**:
  - **Withdraw has no executor, no endpoint, no UI.** `Withdrawn` is dead state today.
  - **Reject only happens implicitly**: `AcceptExecutor` bulk-rejects rival applications via
    `ApplicationRepository.RejectAllExceptAsync` (a raw `ExecuteUpdateAsync` that bypasses the state
    machine — leave it as-is). A venue cannot decline a single application.
- `Accepted` and `PaymentFailed` have **no exit except payment events** — an application whose
  accept-leg payment never completes (declined card, abandoned 3DS) is stuck forever. This is the
  core gap.
- The FE already *anticipates* this feature: the artist dashboard fixtures render a `withdraw`
  action and the venue fixtures a `decline` action, and `ApplicationCard` has an `onDeny` prop **no
  caller passes**. All fixture-driven dead affordances — nothing is wired.
- The artist app has **no live list of their applications** (the endpoints
  `GET /api/Application/artist/pending` and `artist/recently-denied` exist with zero FE consumers;
  the dashboard is fixtures). Withdraw needs a surface to live on — Phase 3.

## Money map per state × contract type (decision 2 — investigated, not guessed)

Accept-leg payment mechanics, from the workflow steps + `EscrowService`:

| | FlatFee | VenueHire | DoorSplit / Versus |
|---|---|---|---|
| Accept leg | Venue hold at accept-checkout (`CreateHoldSessionAsync`, manual-capture PI), **captured synchronously into escrow at Accept** | Artist SetupIntent at apply; **off-session escrow deposit at Accept** | Venue verify session (SetupIntent-like); **no money ever moves pre-Finish** (Payment auto-cancels the verify PI in `WebhookProcessor`) |
| `Applied` | ⚠ possible live uncaptured **venue hold** if venue ran accept-checkout but never accepted (self-expires ~7 days) | none (SetupIntent holds no funds) | none |
| `Accepted` | escrow **Held** (capture is synchronous — `EscrowService.CaptureAsync` creates the row already confirmed) | escrow **Held**, or **Pending** in the 3DS `RequiresAction` window | none |
| `PaymentFailed` | no escrow row (capture failure throws before row creation); hold may linger | no escrow row (deposit failed) | none — verify PI already auto-cancelled |

Consequences:

- **The unwind from `Accepted` is an escrow refund, not an intent-cancel** — and the existing
  `IEscrowClient.RefundByBookingIdAsync` (shipped in PR #76) already does everything needed:
  refunds `Held`, is idempotent on `Refunded`, and is a graceful no-op when there is no escrow row
  or the row isn't refundable. **No new Payment capability is required for the core feature** — a
  single refund step wired into all four workflows covers every type (no-op where no money).
- **There is no SetupIntent authorization to cancel at `Accepted`/`PaymentFailed` for any type**
  — SetupIntents (VenueHire apply, DoorSplit/Versus verify) ring-fence nothing, and Payment already
  auto-cancels verify PIs. The only ring-fenced-but-uncaptured money anywhere is the **FlatFee
  accept-checkout hold**, which exists mostly while the application is still `Applied`, is the
  venue's own money, and self-expires in ~7 days. Releasing it early is a UX nicety, **no client
  method exists for it** (`ManagerPayment` gRPC has `FindHeldIntent` but no cancel;
  `IStripeHoldClient.CancelAsync` is Payment-internal) → flagged as the optional Phase 5 RPC.
- **Race to handle:** cancelling from `Accepted` while a VenueHire escrow is still `Pending` (3DS
  window). `RefundByBookingIdAsync` no-ops on `Pending`, and the late `payment_intent.succeeded`
  would then confirm money into escrow on a dead application — and its `EscrowPaymentSucceeded`
  transition would throw from `Cancelled`. Phase 2 adds **compensating self-transitions**: payment
  events arriving in `Cancelled` are valid no-op transitions, and a late `EscrowPaymentSucceeded`
  in `Cancelled` re-runs the refund step (which now finds `Held` and refunds it). Covered by an
  integration test.

## Decisions resolved

1. **Which states, by whom** (decision 1):

   | From state | Artist (`POST /api/Application/{id}/withdraw`) | Venue |
   |---|---|---|
   | `Applied` | → `Withdrawn` | `POST /api/Application/{id}/reject` → `Rejected` |
   | `Accepted` | → `Cancelled` (+ escrow refund) | `POST /api/Application/{id}/cancel` → `Cancelled` (+ escrow refund) |
   | `PaymentFailed` | → `Cancelled` | → `Cancelled` |
   | `Booked`+ | — concert-cancel path (shipped) | — |

   One verb per persona: the artist always **withdraws** (terminal depends on state), the venue
   **rejects** a pending bid and **cancels** an accepted one. This keeps auth trivial with the
   single-permission `[HasPermission]` attribute (no OR-policy support):
   withdraw = `ArtistPermissions.ApplicationsSubmit`, reject/cancel =
   `VenuePermissions.ApplicationsDecide` (same authority as accept — cancelling reverses the accept
   decision, exactly PR #76's reasoning). Ownership comes from the existing two-party tenant filter
   (`IVenueArtistTenantScoped` on `ApplicationEntity` — cross-tenant lookups 404) — tested, not
   re-implemented.

2. **Money** (decision 2): see the map above. Refund via the existing client method; one new
   optional Payment RPC (`CancelHeldIntent`) flagged for Phase 5 only.

3. **State model** (decision 3): **no new states, no new triggers.** Reuse `Withdrawn`, `Rejected`,
   and the `Cancelled` terminal PR #76 added (semantics already "booking killed, money unwound" —
   a booking row exists from `Accepted` onward, so it fits). New transitions only:
   `Accepted|PaymentFailed + Withdraw|Cancel → Cancelled` (×4 contract types), plus the tolerant
   payment-event self-transitions in `Cancelled`. No persisted-model change → **no migration**
   (PR #76 confirmed appending states/triggers is a schema no-op; here we add neither).

4. **API surface** (decision 4): application-shaped, on `ApplicationController`, as above. HATEOAS:
   extend `ApplicationActions` (currently `Accept` + `Checkout`, emitted unconditionally) with
   state-gated `Withdraw`, `Reject`, `Cancel` links. Gating `Cancel` needs the underlying
   `LifecycleState` (the wire `ApplicationStatus` collapses `Accepted`/`PaymentFailed`/`Booked`/…
   into `Accepted`) → add `LifecycleState State` to the internal `ApplicationDto` for the response
   mapper, same move as PR #76's `ConcertDetails.State`; the wire shape gains only the action links
   and a `Cancelled` status. Viewer-scoping of action links stays deferred (matches today's
   `Accept` link being visible to artists — endpoint auth is the enforcement, PR #76 made the same
   call). While here: map `LifecycleState.Cancelled => ApplicationStatus.Cancelled` (new enum
   member) — today a concert-cancelled application reads "Accepted" in lists, which is wrong.

5. **UI** (decision 5): **both manager SPAs** (venue: reject + cancel; artist: withdraw). Detail in
   Phase 3. Customer app untouched.

6. **Tests** (decision 6): integration is the gate (deterministic — mocked escrow, no webhook
   races); **no new E2E scenarios** — the refund mechanism is already E2E-proven end-to-end by
   PR #76's `ConcertCancelledTests`, and the `Accepted` window is webhook-raced in a live stack
   (flaky by construction). Run the standard **UI E2E regress** once at the end (Phase 4): the SPAs'
   application surfaces change, which meets the behavioral-risk bar of
   [plans/CLAUDE.md](../CLAUDE.md). Optional future: an API E2E driving a declined card to
   `PaymentFailed` then cancelling — noted, not required.

## Side decisions

- **Opportunity re-opens on cancel.** `QueryableOpportunityExtensions` hides an opportunity once any
  application passes `Applied`/`Rejected`/`Withdrawn`. Add `Cancelled` to the exclusion list so the
  venue can re-recruit (rivals were bulk-rejected at accept; re-opening is the only way to refill
  the slot). ⚠ Deliberate side effect: opportunities of **concert-cancelled** (PR #76) bookings
  re-open too — that's a fix, not a regression (test both).
- **Notifications** mirror `NotifyAppliedAsync`/`NotifyAcceptedAsync`: conversation message + email
  to the counterparty (withdraw → venue manager; reject/cancel → artist). Add
  `ApplicationWithdrawn` / `ApplicationRejected` / `ApplicationCancelled` to the internal
  `MessageAction` enum (Conversations.Contracts, additive).
- **No new integration events.** Nothing pre-`Booked` is projected cross-service (no concert
  exists), and `ConcertApplication*Event` contracts are already dead code per
  [api/Concertable.B2B/TECH_DEBT.md](../../api/Concertable.B2B/TECH_DEBT.md) — don't add more.
- **Booking rows are left in place** on cancel from `Accepted`+ (application state is authoritative;
  concert-cancel leaves them too).

## Phases

- [x] **Phase 1 — B2B: withdraw + reject from `Applied` (pre-money).** ✅ SHIPPED.
  - As planned (executors + dispatchers, endpoints, Pending-gated HATEOAS links, `MessageAction`
    additions), plus deltas found while building:
    - Invalid transitions are **409** (`ConflictException` from `ContractStateMachine.Next`), not
      400 — tests assert 409; same applies to Phase 2's cancel-from-`Booked` gate.
    - Notification plumbing extracted while adding the withdraw/reject notifications:
      `IApplicationNotifier` (who/what per application event) over `INotifier` (conversation
      message + `EmailCopy`, verbs mirroring `IConversationsModule`). `ApplicationService` no
      longer touches conversations/email/user modules directly.
    - "Opportunity re-opens" is asserted via the `WhereOpen`-backed venue opportunity list —
      same-artist re-apply is schema-forbidden (unique `(OpportunityId, ArtistId)`), and the
      duplicate-apply 500 gap is logged in `api/Concertable.B2B/TECH_DEBT.md`.

- [ ] **Phase 2 — B2B: cancel from `Accepted`/`PaymentFailed` (money-aware).** ~1–2 days.
  - Builder: `WithApplicationCancel<TStep>()` adding `Accepted|PaymentFailed + Withdraw|Cancel →
    Cancelled` + the tolerant payment-event self-transitions in `Cancelled`; applied to **all four**
    workflows.
  - `IApplicationCancelStep` + `RefundEscrowByApplicationStep`: resolve booking via
    `IBookingRepository.GetByApplicationIdAsync`, call `IEscrowClient.RefundByBookingIdAsync`
    (refunds FlatFee/VenueHire `Held`; correct no-op for DoorSplit/Versus and `PaymentFailed`).
    One step for all types — the client's no-op semantics make per-type steps pointless.
  - Extend `WithdrawExecutor` to run the cancel step when leaving `Accepted`/`PaymentFailed`; new
    `CancelApplicationExecutor` for the venue verb. Late-capture compensation: on
    `EscrowPaymentSucceeded` in `Cancelled`, re-run the refund step.
  - `POST /api/Application/{id}/cancel`; `ApplicationDto.State` (internal); `ApplicationStatus.Cancelled`
    + mapper fix; state-gated `Cancel` HATEOAS link; `Cancelled` added to the opportunity-availability
    exclusions.
  - **Gate:** build green; integration tests — per-type cancel from `Accepted` (FlatFee/VenueHire:
    `MockEscrowClient.Refunds` fired + terminal `Cancelled`; DoorSplit/Versus: no-op + `Cancelled`),
    cancel from `PaymentFailed`, artist withdraw from `Accepted` refunds too, cancel from `Booked`
    → 409 (concert-cancel's territory), late-`EscrowPaymentSucceeded`-after-cancel auto-refunds,
    opportunity re-opens (application-cancel and concert-cancel cases). No migration (no
    persisted-model change).

- [ ] **Phase 3 — FE: both manager SPAs.** ~1–1.5 days.
  - `app/shared` `applicationApi`: `withdrawApplication`, `rejectApplication`, `cancelApplication`,
    `getPendingForArtist`, `getRecentDeniedForArtist`; `ApplicationActions` type gains
    `withdraw`/`reject`/`cancel`, `ApplicationStatus` union gains `"Cancelled"`.
  - **Venue:** pass `onDeny` from `ApplicationsPage` into the existing `ApplicationCard` affordance
    (confirm dialog + toast, `CancelBookingButton` pattern); add a Cancel button rendered when
    `actions.cancel` is present. Both action-gated, `data-testid` hooks.
  - **Artist:** minimal **My Applications** page — new artist-only route
    `_artist/my/applications` (artist `src/`, not b2b/shared — single-app rule) listing pending +
    recently-denied via the endpoints above, with Withdraw/Cancel buttons per `actions`. Nav link
    from the artist my-page. Align the artist dashboard fixtures' `withdraw` href to the real verb
    (`POST /api/Application/{id}/withdraw`) in passing.
  - **Gate:** all four web builds green (`web-customer`, `web-venue`, `web-artist`, `web-business`).

- [ ] **Phase 4 — Verify + close out.** ~0.5 day.
  - Full B2B Concert integration suite; **UI E2E regress** via the `e2e-ui-regress` skill (Docker
    pre-flight per root CLAUDE.md) — SPA behavior changed in covered flows.
  - Tick the 🔴 blocker in [LAUNCH_PLAN.md](LAUNCH_PLAN.md) (application-cancel line) and `git rm`
    this plan **in the final commit** (or leave both riding the working tree if the merge queue
    forces close-out after merge — per [plans/CLAUDE.md](../CLAUDE.md) doc-only close-out).

- [ ] **Phase 5 (OPTIONAL — decide when reached; skippable without losing correctness) — FlatFee
  hold release.** ~1 day + a merge-queue cycle.
  - Today an orphaned accept-checkout hold (venue ran checkout, application then withdrawn/rejected/
    cancelled) self-expires in ~7 days — money-safe, just slow to release the venue's funds.
  - **PR1 (Payment):** `CancelHeldIntent(payer_id, application_id)` RPC on `ManagerPayment`
    (lookup mirrors `FindHeldIntent`, cancel if cancellable, no-op success otherwise) +
    `IManagerPaymentClient.CancelHeldIntentAsync` + fake/mock impls. Additive package change →
    merges alone, publishes `Payment.Client`.
  - **PR2 (B2B):** pin bump; best-effort hold release on FlatFee withdraw/reject/cancel.
  - Two PRs forced by the package boundary (same split as the ESCROW_REFUND plan — B2B compiles
    against the *published* `Payment.Client`). If skipped, log the ~7-day hold-expiry lag in
    `api/TECH_DEBT.md` and delete this phase.

## Reference

- `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Services/Workflow/`
  — builder, executors, dispatchers, steps (`RefundEscrowStep` is the template; it's concert-keyed,
  the new step is application-keyed).
- `.../Concert.Api/Controllers/ApplicationController.cs`, `.../Concert.Api/Mappers/ApplicationResponseMapper.cs`,
  `.../Concert.Api/Responses/ApplicationResponses.cs` — endpoint + HATEOAS surface.
- `.../Concert.Infrastructure/Services/ApplicationService.cs` — notification patterns
  (`NotifyAppliedAsync`/`NotifyAcceptedAsync`).
- `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/EscrowService.cs` —
  `RefundByBookingIdAsync` no-op/idempotency semantics (lines ~204-233).
- `.../Concert.IntegrationTests/Application/Application*ApiTests.cs` + `Concert/ConcertCancelApiTests.cs`
  — test patterns (incl. `MockEscrowClient.Refunds`).
- `git show 3a411436^:plans/b2b/ESCROW_REFUND.md` — the shipped concert-cancel plan (scope
  discipline + the Payment/B2B two-PR delivery split this plan's Phase 5 mirrors).
