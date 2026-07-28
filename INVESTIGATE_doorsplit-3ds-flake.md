# Investigate: flaky UI-E2E — "3DS challenge on door split" → "a draft concert is created" times out at 60s

**Goal:** ROOT-CAUSE and fix the flake so it stops intermittently failing. Do **not** re-run to get a
green, and do **not** `@quarantine` it — the owner explicitly wants the underlying race fixed, not hidden.

## Exact failure (real, reproduced in CI)

- CI run `30369219117` (PR #237, `e2e-ui-tests` job): **26/27 passed, 1 failed.**
- Failing scenario: **`Venue manager completes 3DS challenge on door split`**
  (`api/Concertable.B2B/tests/E2ETests/Concertable.B2B.E2ETests.Ui/Features/DoorSplitWorkflow.feature`):
  ```
  Given a door split opportunity has been applied to
  When the venue manager registers a card with a 3DS card
  Then a draft concert is created        <-- FAILS: System.TimeoutException: Timeout 60000ms exceeded.
  ```
- The **non-3DS** sibling (`Venue manager books artist on a door split`) and every other scenario pass.
  The flake is specific to the **3DS** path.

## What the failing step actually waits for

`api/Concertable.B2B/tests/E2ETests/Concertable.B2B.E2ETests.Ui/Steps/VenueManagerSteps.cs:191`
```csharp
[Then(@"a draft concert is created")]
public Task DraftConcertCreated() =>
    browser.Page.WaitForURLAsync("**/my/concerts/concert/**", new() { Timeout = 60_000 });
```
So the assertion is: **the SPA navigates to the draft-concert page within 60s.** The navigation is
driven entirely client-side by a **SignalR notification**, not by the accept HTTP response.

## The async chain that must complete inside 60s (this is where the race lives)

Frontend (`app/web/b2b/venue/src/features/concerts/pages/VenueAcceptCheckoutPage.tsx`):
- `useCheckoutFlow<ConcertDraftCreatedPayload>({ event: "ConcertDraftCreated" })` (hook:
  `app/web/shared/src/features/concerts/hooks/useCheckoutFlow.ts`) subscribes to the **`ConcertDraftCreated`
  SignalR event** and only sets `flow.phase === "success"` when it arrives.
- `useEffect` at line 67-70 navigates to `/my/concerts/concert/$id` **only** on `flow.phase === "success"`
  (id = `flow.result`). No notification ⇒ no navigation ⇒ the Playwright `WaitForURLAsync` times out.

Backend notification source:
- `.../Concert/…Infrastructure/Services/ConcertDraftService.cs:65-68` — after creating the draft, calls
  `notifier.ConcertDraftCreatedAsync(...)` for **both** artist and venue user ids.
- `ConcertNotifier.cs:12` → `notificationClient.SendAsync(userId, "ConcertDraftCreated", payload)` (SignalR).

What triggers `ConcertDraftService` (the workflow **Book** step, `CreateConcertDraftStep`):
- `.../Concert/…Infrastructure/Services/Workflow/Executors/EscrowExecutor.cs:45` and
  `VerifyExecutor.cs:37` call `workflow.Book.ExecuteAsync(bookingId)`.
- **For 3DS specifically** the booking is gated on the Stripe **webhook** confirming the payment, which
  travels: 3DS challenge completes in the browser → Stripe → **stripe-cli** container forwards the webhook
  → `payment-web` (`/api/webhook`) → payment event over the **ASB emulator** → B2B consumes → Book step →
  draft created → SignalR notify. That extra webhook + emulator round-trip is the latency the non-3DS
  path doesn't pay — and the prime suspect for the intermittent >60s.

## Hypotheses to test (in order)

1. **Webhook/ASB-emulator latency spike** — the `payment_intent` webhook or the payment→B2B ASB hop
   occasionally takes tens of seconds under CI load. Instrument timestamps at each hop (stripe-cli forward,
   payment-web webhook receipt, payment event publish, B2B consume, Book step start/finish, notify send)
   and correlate on a failing run. See `api/docs/DEBUGGING_CONVENTIONS.md` before adding logs (promote
   durable ones to the module `Log.cs` with `[LoggerMessage]`; there is already a
   `ConcertDraftCreated` log at `Concert.Infrastructure/Log.cs:67`).
2. **Lost SignalR notification** — the notification is sent (check for the `ConcertDraftCreated` log line
   on a failing run) but the SPA's SignalR connection wasn't subscribed yet / reconnected / missed it.
   If the draft IS created server-side but the browser never navigated, the bug is delivery, not latency —
   the flow needs a reconciling poll/refetch fallback, not a bigger timeout. Check `useCheckoutFlow.ts` for
   whether it has any fallback poll or is purely push-based.
3. **The draft genuinely isn't created in time** — the Book step never ran (executor gating on a payment
   state that didn't arrive). Check whether the concert row exists in the B2B DB after a failing run.

Distinguish 1 vs 2 vs 3 from the **Aspire service logs** on a failing run (grep the test output for
`Resources.payment-web`, `Resources.payment-workers`, `Resources.b2b-web`, and the `ConcertDraftCreated`
log) — do NOT guess from the browser side alone. The right fix differs per hypothesis:
- (1) speed up / make the hop deterministic; a fixed 60s bump is NOT a root-cause fix.
- (2) give `useCheckoutFlow` a reconciling fallback (poll the concert-by-application endpoint) so a missed
  push still resolves — this is the likely correct fix and matches "don't rely on a single async signal".
- (3) fix the executor gating.

## Reproduce locally

The local stack is now healthy (three prior blockers cleared — see PR #237 and below). From the
`Bug/DoorSplitE2ETimeout` worktree:
```
./scripts/docker-health.ps1                  # must exit 0 (real data round-trip, not just `docker ps`)
$env:HEADLESS='true'
dotnet test 'api/Concertable.B2B/tests/E2ETests/Concertable.B2B.E2ETests.Ui/Concertable.B2B.E2ETests.Ui.csproj' `
  --filter "DisplayName~3DS challenge on door split" --logger "console;verbosity=normal"
```
Flakes are intermittent — you may need several runs (or add hop-timing logs and run until one fails).
On failure, `CaptureFailureAsync` saves a screenshot under the test project's
`bin/Debug/net10.0/playwright-failures/`. Use the `e2e-ui-debug` skill.

### Local-setup gotchas (CI provides these automatically; a fresh worktree does NOT)
- **`ServiceAuth:AuthClientSecret`** must be in the `b2b-apphost-dev-secrets` user-secrets or **auth
  crashes on boot** → users never seed → `b2b-web /health` 503 forever. Set any value (self-contained
  within auth): `dotnet user-secrets set "ServiceAuth:AuthClientSecret" "<any>"` in
  `api/Concertable.B2B/src/Concertable.B2B.AppHost`. (PR #237 makes the crash a clear message instead of
  an opaque `Sha256(null)` NRE.)
- **`app/node_modules`** must be installed in the worktree (`npm ci` in `app/`, or mirror from the main
  checkout) or the SPAs fail with `'vite' is not recognized` → SPA readiness times out before any scenario.

## Current state / what's already done (do not redo)

- **PR #237** (`Bug/DoorSplitE2ETimeout`) fixes the *separate* local wall and is otherwise green (26/27):
  1. `fix(e2e)`: wired `ServiceBus:ServiceName` on the E2E `search-workers` resource (it was crashing).
  2. `fix(auth)`: fail-fast + clear message on a missing service-client secret.
  Auto-merge is enabled but **blocked by this flake**. This flake is the ONLY thing standing between #237
  and merge — fixing it here (add the fix as a commit on this same branch/PR) is the clean path.
- This flake is NOT caused by the #237 changes (the 3DS-door-split timeout predates them; it's the exact
  flake that failed→passed-on-rerun on PR #224 and is green-but-flaky on main).

## Do NOT
- Bump the 60s timeout to paper over latency, re-run until green, or `@quarantine` the scenario.
- Conclude from the browser side alone — confirm server-side whether the draft was created and the
  notification sent (hypotheses 1/2/3 above) before choosing a fix.
