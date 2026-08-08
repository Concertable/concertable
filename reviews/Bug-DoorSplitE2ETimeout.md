# Code review — Bug/DoorSplitE2ETimeout

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `8d709a33fca980f9955fabdc88e02f90f7c2dac4`  _(2026-07-28)_

> Range reviewed: `c3e30a68..8d709a33` (substantive change: `8d709a33` "fix(concert): resolve the accept/verify booking race with a durable join"; the rest of the range is merges + already-reviewed infra commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **BUG1 — HIGH — correctness (missed rendezvous / lost update)** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Services/Workflow/Executors/VerifyExecutor.cs:31`
  **Fixed:** unified the three entry points onto a single `ConvergeAsync` that reads committed `{State, Verification}`
  *after* the writer commits its own half. The webhook arm now `RecordPaymentVerified`/`RecordPaymentFailed` +
  `SaveChangesAsync` then `ConvergeAsync`, instead of branching on the pre-commit in-memory `application.State`.
  Whichever writer commits last sees both halves and books; a double-book races on the `Concert.BookingId` unique
  index (swallowed). Rendezvous no longer depends on timing.
  The webhook arm decides whether to Book from the **pre-commit, in-memory** `application.State` read at
  the top of `ExecuteAsync` (and `ExecuteFailedAsync`), not from committed state read *after* it commits
  its own half. So the rendezvous is one-sided: only the accept arm (`ConvergeAfterAcceptAsync`) re-reads
  committed state; the webhook arm does not. Interleaving that misses the join, no RowVersion to catch it:
  1. webhook `ExecuteAsync` runs `GetByIdAsync` → reads `State = Applied` (accept hasn't committed yet);
  2. accept commits `Applied → Accepted` (+ booking) and runs `ConvergeAfterAcceptAsync`, whose
     `GetConvergenceSnapshotAsync` reads committed `{Accepted, None}` (webhook's `Verified` not yet saved) →
     switch falls to `_ => Task.CompletedTask`, no-op;
  3. webhook resumes, sees stale in-memory `Applied` → `IsBookingPending` false → `SaveChangesAsync` writes
     `Verified` and returns cleanly.
  End state `{Accepted, Verified}`, **no concert booked, nothing retries** — the ASB message completed
  (no throw → no redelivery) and the accept request already returned. That is a *permanent* stall, strictly
  worse than the late-heal this fix replaces. The two writers touching disjoint columns (`State` vs
  `PaymentVerification`) is exactly why the DB raises no error here — so the plan's "no lost update" claim
  holds for column integrity but does **not** hold for the rendezvous, and this is precisely the
  check-then-act-on-a-pre-commit-snapshot the plan's "The hazard to get right: lost update" section forbids.
  The two new integration tests don't catch it because they fully serialise webhook-then-accept (the webhook
  is committed before `/accept` is sent), never exercising the concurrent window.
  **Fix:** make the webhook arm symmetric with the accept arm — in `ExecuteAsync`/`ExecuteFailedAsync`
  `RecordPaymentVerified`/`RecordPaymentFailed` + `SaveChangesAsync` first, then call a converge that
  re-reads committed `{State, Verification}` and books/fails when booking-pending (swallowing the same
  `ConflictException`/duplicate-key), instead of branching on the pre-commit in-memory `application.State`.

- [-] **BUG2 — MEDIUM — DEFERRED — correctness/resilience (un-retried inline converge on the accept side)** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Services/Workflow/Executors/AcceptExecutor.cs:67`
  **Decision needed:** the fix is architectural, not mechanical — making the accept-driven Book durably retriable
  (drive it off the message/outbox path, or accept the narrow stall window as a known tradeoff pending Phase 3).
  Left for a human call; code untouched.
  `ConvergeAfterAcceptAsync` is `await`ed inline in the accept HTTP request, *after* the `Accepted`+booking
  transaction has already committed, and it swallows **only** `ConflictException` and duplicate-key. When the
  webhook arrived first (`Verified` recorded), the converge drives Book inline; if the Book step itself throws
  anything else — `workflow.Book.ExecuteAsync` (concert-draft creation / SignalR `ConcertDraftCreated`), or a
  non-duplicate `DbUpdateException` — it propagates as a 500 to the venue manager with the accept already
  committed. The deal is then `Accepted` with a booking but never `Booked`, and **nothing retries**: a repeat
  `POST /accept` throws `ConflictException` (state is no longer `Applied`), and the webhook message already
  completed so ASB won't redeliver. The pre-fix design self-healed via ASB redelivery; the accept-driven book
  has no equivalent path, so this is a new (narrower) stall window, not one Phase 3's messaging-retry work
  would cover (the accept arm isn't a message handler). **Fix:** don't leave a post-commit converge/Book
  failure un-retried — drive Book from the durable message/outbox path (idempotent, redelivered) rather than
  inline-and-lost in the accept request, so a transient Book failure re-runs instead of stranding the booking.

## Focus points checked and cleared

- **Disjoint-column concurrency vs RowVersion (BUG1 aside):** sound for *data integrity* — accept writes only
  `State`, webhook writes only `PaymentVerification`/`PaymentTransactionId`, and EF's modified-columns-only
  UPDATE means neither clobbers the other; the 1:1 unique index on `Concert.BookingId` genuinely makes a
  double-draft impossible (concurrent second Book fails at the DB, swallowed as duplicate-key). The gap is in
  the rendezvous *logic* layered on top (BUG1), not the column model.
- **`ConvergeAfterAcceptAsync` swallowing `ConflictException` + duplicate-key:** correctly targeted — the only
  transitions attempted (`VerifyPaymentSucceeded`/`VerifyPaymentFailed` from `Accepted`) are valid, so a
  `ConflictException` can only mean the other writer already advanced the state, and the duplicate-key can only
  be the `Concert.BookingId` unique index. Not over-broad; no finding.
- **Tenancy scope for converge:** using each writer's own scope (accept = venue request scope, webhook = host
  message scope) rather than a fresh `IScoped` is consistent with the pre-existing path — the webhook already
  loaded the application via `transitioner` in the same scope before this change, and both
  `GetByIdAsync`/`GetConvergenceSnapshotAsync` read `context.Applications` identically. No fail-closed regression.
- **`api/agents/CODE_PATTERNS.md` (keyed strategy resolver / no branch-on-`DealType`):** clean. `workflows.Create(app.DealType)`
  is the keyed resolver; the new `switch` is on the `PaymentVerification` outcome (a convergence decision), not
  on `DealType`, so it is not the branch-on-closed-key anti-pattern.
- **Zero-comments rule:** no new comments in production code (the removed `// Verify events ring-fence…` block
  is deleted, not replaced). The two `// Arrange —` comments in the new tests carry the non-obvious race-ordering
  *why* and are within bounds.
