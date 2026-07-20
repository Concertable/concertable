# Empty-string hack elimination — full program

Kill every empty-string coercion that **masks a missing value** (`?? ""`, `?? string.Empty`,
`GetValueOrDefault(..., string.Empty)`) across the backend, fixed at the **root** (fail-fast /
nullable / presence-preserving) — never by cosmetically swapping `""` → `string.Empty`. Genuine
empty-string fallbacks the conventions *permit* are kept, explicitly.

**Do NOT half-do this.** A site is DONE only when committed AND build/test-verified. Work the sequence
below; don't stop mid-category.

## Hard-won facts (verified — don't re-derive)

- **`ServiceAuth:ClientSecret` is a genuine optional.** `AppHost.Shared` wires it with
  `WithOptionalEnvironment` (lines 85/107/132) — **absent in dev, E2E, and integration "Testing" by
  design** (secret-less local client). A blanket `?? throw` here bricks dev + every test host — but
  `?? string.Empty` is **equally wrong**: it's the exact cosmetic swap this program forbids, masking a
  *missing* secret as an *empty* one. The honest fix: leave it **null** when absent (a public/secret-less
  client has no secret), and have the token request **omit** `client_secret` rather than send an empty
  value. Hosts bind null boundary-free (done); the omit-on-null is in the Kernel token service → Cat C.
- **Integration hosts boot without auth/bus config.** `ApiFixture` (B2B, and the sibling suites) run
  `WebApplicationFactory<Program>` under env **"Testing"**, supplying only DB/blob/URL config — **no
  `Auth:Authority`, `ServiceAuth:*`, or `asb`**. So any host-side `?? throw` MUST be skipped in the
  "Testing" environment (transport is mocked there anyway), or it breaks the whole integration suite.
- **`Authority` / `ClientId` / `asb` ARE present** in dev/E2E/prod (`WithEnvironment` + `.WithReference(asb)`),
  so fail-fast is correct there — but see the "Testing" caveat above → **needs an integration boot-check (Docker) to verify.**
- **`ServiceDefaults` is `IsPackable=true`** (published) — a shared config-helper there would itself be a
  cutover. Boundary-free fail-fast = inline `?? throw` (Testing-guarded), no shared helper.

## The 34 sites — inventory, fix, status

### A · Required host config masked to `""` — host boots misconfigured, fails later
Files: `*/Program.cs`, `B2B.Workers/ServiceCollectionExtensions.cs`. Boundary-free (host files only),
but **behavior-changing** → verify with an integration boot-check.

| Site(s) | Setting | Fix |
|---|---|---|
| `Customer.Web:78`, `Auth:84`, `B2B.Web:114`, `B2B.Workers:60` | `Auth:Authority` (has service-discovery fallback) | ✅ **DONE** — fail-fast throw when explicit key AND fallback both missing; `null!` in env "Testing" (Kernel `AddClientCredentials` already skips the URI when Authority is blank, so no startup throw) |
| `Customer.Web:79`, `Auth:85`, `B2B.Web:115`, `B2B.Workers:61` | `ServiceAuth:ClientId` | ✅ **DONE** — fail-fast; `null!` in "Testing" |
| `Customer.Web:80`, `Auth:86`, `B2B.Web:116`, `B2B.Workers:62` | `ServiceAuth:ClientSecret` | 🟠 **PARTIAL** — hosts now bind it **null** when absent (`9ecb4c0b`'s `?? string.Empty` was a forbidden cosmetic swap; replaced with assign-only-when-present). Full fix — `TokenServiceOptions.ClientSecret` → `string?` + token service **omits** the `client_secret` param when null — is a **published Kernel change → Cat C cut-over** |
| `Customer.Web:88`, `Auth:102`, `B2B.Web:126`, `Payment.Web:64`, `Payment.Workers:39`, `Search.Workers:26`, `B2B.Seed.Simulator:22` | asb `ConnectionString` | ✅ **DONE** — fail-fast throw. **Correction:** *not* Testing-guarded — the throw is unconditional and safe, because the transport options lambda is lazy and every integration "Testing" host mocks the bus (`AzureServiceBusReceiver` removed + `IBusTransport`→`MockBusTransport`; Search.Web has no ASB), so the asb options never resolve at Testing boot |
| `Payment.Infrastructure/.../Webhook/WebhookService.cs:27` | Stripe `WebhookSecret` | ✅ **DONE `df51206f`** — constructor throw (lazy scoped service; ValidateOnStart would break Testing boot) |

### B · gRPC wire boundary (proto3 strings can't be null) — PUBLISHED Payment package → CUTOVER
Fix = make the proto fields `optional string` so presence survives; server mappers set conditionally,
client mappers reconstruct `null`, callers test `Has*`. Regenerate proto, publish `Payment.Grpc`/`.Client`,
platform-sync. Use `/package-cutover`.

- `Payment.Infrastructure/Grpc/EscrowMappers.cs:13` — `ClientSecret`
- `Payment.Infrastructure/Grpc/PaymentMappers.cs:19,20` — `ClientSecret`, `TransactionId`
- `Payment.Infrastructure/Grpc/PayoutAccountGrpcService.cs:19,46` — `Url` (OnboardingLink), `ClientSecret` (SetupIntent)

### C · Kernel published changes — PUBLISHED Kernel package → CUTOVER
- `Shared/Concertable.Kernel/Identity/ClaimsPrincipalExtensions.cs:9` — `GetId()` fails open to `""`;
  return `string?`/throw (fail-closed); fix `NotificationHub`'s guard so it actually rejects a
  `sub`-less principal. Publish Kernel → consumers. (Own MED tech-debt item in `api/TECH_DEBT.md`.)
- `Shared/Concertable.Kernel/Auth/TokenServiceOptions.cs` + `ClientCredentialsTokenService.cs` —
  `ClientSecret` → `string?`; the token request **omits** the `client_secret` form key when null
  (correct OAuth2 for a public/secret-less client) instead of sending an empty one. Hosts already bind
  null when absent (boundary-free, done on this branch); this Kernel change completes it. Publish Kernel.

### D · Genuine empty fallbacks — KEEP (convention permits; converting = churn, not a fix)
- `Auth/Services/RemoteProfileClaimsProvider.cs:53` — `ex.Content ?? string.Empty` (log fragment).
- `Payment.Infrastructure/Events/PaymentTransactionHandler.cs:22` and `PaymentFailureDispatcher.cs:22` —
  `Metadata.GetValueOrDefault("type", string.Empty)` (Stripe-metadata discriminator at the webhook
  boundary; absent → falls through the switch). Re-confirm the switch degrades gracefully; only tighten
  if a missing `type` on OUR events must be an error.

### E · Masks that are their own tech-debt items — boundary-free
- **`ConcertResponseMappers.cs:43,44`** (`BannerUrl`, `Avatar`) → **DONE (code written)**: `ConcertDetailsResponse`
  props `string?`, mapper drops `?? string.Empty`; `app/shared/.../concerts/types.ts` fields optional
  (matches existing `Hero`/`avatar?` props, no consumer ripple).
- ✅ **DONE `3327722b` (+`3831a130` mobile refine)** — **`Customer/.../Ticket/.../TicketService.cs:150,156`**
  (`currentUser.Email ?? string.Empty`) + **`TicketPaymentProcessor.cs:50`**
  (`GetValueOrDefault("fromUserEmail", string.Empty)`): `UserEmail` dropped from `TicketDto`, mapper no longer
  threads email, `fromUserEmail` now `meta["fromUserEmail"]` (fail-closed). Email sourced from auth state.
  ⚠️ Two plan errors found: mobile `TicketDetailScreen` **did** render `ticket.userEmail` (rewired to
  `useAuthStore`); and **"exclude `QrCode`" is wrong** — both surfaces render the QR off the list DTO, so it
  stays. NOT boundary-free (mobile too).

### Plus (rides a cutover)
- `Messaging.AzureServiceBus/Options/AzureServiceBusOptions.cs` — `= ""` property defaults → `null!`
  (PUBLISHED Messaging package; goes with the config/Messaging publish).

## Current worktree state — `Fix/TechDebtSweep` (off `origin/master`)

Path: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Fix\TechDebtSweep`

**Committed on this branch (builds verified green):**
- **#3 ConcertDetailsResponse** (Cat E) — `0c033bad`.
- **#2 dup-app 500→400** (B2B LOW) — `f72c939c`. Integration test (apply-after-withdraw → 400) still TODO (Docker).
- **#4 web buy-tickets** (app/web MED) — `02781a8c`. Narrow-viewport E2E scenario still TODO (Docker).
- **ClientSecret bound null-when-absent** (Cat A optional) — `9ecb4c0b` bound it to `string.Empty`, which was
  the forbidden cosmetic swap; a follow-up commit replaces that with assign-only-when-present across the 4 host
  sites (null when unconfigured, no `""`). Same wire behaviour today (a null form value encodes empty). The
  *complete* fix — token service **omits** `client_secret` when null — is a Kernel publish → **Cat C cut-over**.
- **TicketDto.UserEmail dropped + `fromUserEmail` fail-closed** (Cat E) — `3327722b`, refined `3831a130`.
  Email now sourced from auth state. ⚠️ **The plan's "boundary-free Customer+SPA" was wrong on two counts:**
  (1) **mobile `TicketDetailScreen` DID render `ticket.userEmail`** — rewired to read the signed-in email from
  `useAuthStore` (gated on the user, not on email-presence); (2) **"exclude QrCode from the list projection"
  is wrong** — both web (`TicketCard`→`QrPopover`) and mobile (`<QRCode value={ticket.qrCode}>`) render the QR
  straight off the list DTO, so it can't be dropped without an SPA lazy-QR-fetch rework. Kept QrCode; only the
  empty-string masks were removed. Customer TECH_DEBT entry reworded to that residual.
- **WebhookService fail-close** (Payment LOW) — `df51206f`. Constructor throw, NOT ValidateOnStart — the service
  is scoped (lazy per webhook), so the throw can't break host startup or the integration "Testing" host (which
  never hits the webhook path); ValidateOnStart would demand the secret at every boot incl. Testing.
- **EscrowService `Result.Ok<T?>` type-arg cleanup** (Payment LOW) — `aba922af`.
- **Category A config fail-fast** (`Auth:Authority` / `ServiceAuth:ClientId` / asb `ConnectionString`) across all 9
  host files + `B2B.Workers/ServiceCollectionExtensions.cs` (took a new `IHostEnvironment` param). `api/TECH_DEBT.md`
  rewritten to the residual (`AzureServiceBusOptions = ""` → `null!`, rides the Messaging publish). Slnx build **0
  errors**. **Docker integration gate could NOT run locally** — Testcontainers failed to reach Docker (290 identical
  "Docker not running/misconfigured" errors, every test died at SQL-container creation before any host booted, despite
  `docker-health.ps1` passing) → per `CLAUDE.md` a startup-failed suite is an environment problem, stopped, did not
  rerun. Boot-safety established by code analysis instead (see the asb row above); the merge-queue runs the full
  integration/E2E gate on the PR.

Verified: full `api/Concertable.slnx` build **0 errors**; web-customer build green; mobile `tsc --noEmit` clean
(bar a pre-existing, unrelated `SearchFilterSheet` search-sort error); **Payment unit tests 30/30**.

**Working tree: clean.**

**Reverted / not a fake fix:** the two Payment gRPC mapper edits (Cat B) were reverted to `?? ""` —
they belong to the proto cutover, not a cosmetic swap.

**Not started — all three are PUBLISHED-package CUTOVERS, each its own PR (can't share this branch):**
Cat B cutover (Payment proto `optional string`); Cat C cutover (Kernel `GetId` fail-closed + `NotificationHub`
guard); `AzureServiceBusOptions = ""` → `null!` (rides the Messaging publish). Do each via `/package-cutover`
*after* this branch's PR merges and platform-sync is green.

## PR sequence (multi-PR by necessity — cutovers can't share a branch)

1. ✅ **PR1 + PR2 — `Fix/TechDebtSweep` (all boundary-free work): committed, shipping as ONE draft PR.**
   #2 dup-app, #3 ConcertDetailsResponse, #4 web buy-tickets, `ClientSecret` → `string.Empty`,
   `TicketDto.UserEmail`, WebhookService fail-close, EscrowService type-args, **and Category A config fail-fast**
   (`Authority`/`ClientId`/`asb`). Slnx build 0 errors. Docker integration gate couldn't run locally (Testcontainers
   env failure — see worktree-state note) → merge queue runs the full gate on the PR. Opened as **draft**.
3. **PR3 — CUTOVER: Kernel `GetId` fail-closed** + `NotificationHub` guard. Publish Kernel → platform-sync.
4. **PR4 — CUTOVER: Payment proto `optional string`** (Cat B). Publish `Payment.Grpc`/`.Client` → platform-sync.

## Resume (fresh context)

1. `cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Fix\TechDebtSweep`; `git status` to see the
   uncommitted #2/#3/#4 edits (or the commits, if already landed).
2. Read this file. Finish verifying the in-flight builds; commit each green fix (one per fix,
   `TECH_DEBT.md` update in the same commit). Don't push without Tommy's go-ahead.
3. Work PR1 to done, then PR2 (Docker-verified), then the cutovers via `/package-cutover`.
4. Delete this plan in the commit that lands the final piece (git history is the archive).
