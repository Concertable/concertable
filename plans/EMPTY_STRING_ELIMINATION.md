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
  design** (secret-less local client). A blanket `?? throw` here bricks dev + every test host. Fix =
  explicit `string.Empty` (or require *only* in Production), never a hard throw.
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
| `Customer.Web:78`, `Auth:84`, `B2B.Web:114`, `B2B.Workers:60` | `Auth:Authority` (has service-discovery fallback) | fail-fast: throw when explicit key AND fallback both missing; skip in env "Testing" |
| `Customer.Web:79`, `Auth:85`, `B2B.Web:115`, `B2B.Workers:61` | `ServiceAuth:ClientId` | fail-fast (Testing-guarded) |
| `Customer.Web:80`, `Auth:86`, `B2B.Web:116`, `B2B.Workers:62` | `ServiceAuth:ClientSecret` | **genuine optional** → explicit `string.Empty` (NOT a throw) |
| `Customer.Web:88`, `Auth:102`, `B2B.Web:126`, `Payment.Web:64`, `Payment.Workers:39`, `Search.Workers:26`, `B2B.Seed.Simulator:22` | asb `ConnectionString` | fail-fast (Testing-guarded) |
| `Payment.Infrastructure/.../Webhook/WebhookService.cs:27` | Stripe `WebhookSecret` | ValidateOnStart / throw at startup (own LOW tech-debt item) |

### B · gRPC wire boundary (proto3 strings can't be null) — PUBLISHED Payment package → CUTOVER
Fix = make the proto fields `optional string` so presence survives; server mappers set conditionally,
client mappers reconstruct `null`, callers test `Has*`. Regenerate proto, publish `Payment.Grpc`/`.Client`,
platform-sync. Use `/package-cutover`.

- `Payment.Infrastructure/Grpc/EscrowMappers.cs:13` — `ClientSecret`
- `Payment.Infrastructure/Grpc/PaymentMappers.cs:19,20` — `ClientSecret`, `TransactionId`
- `Payment.Infrastructure/Grpc/PayoutAccountGrpcService.cs:19,46` — `Url` (OnboardingLink), `ClientSecret` (SetupIntent)

### C · Kernel `GetId()` fails open to `""` — PUBLISHED Kernel package → CUTOVER
- `Shared/Concertable.Kernel/Identity/ClaimsPrincipalExtensions.cs:9` — return `string?`/throw (fail-closed);
  fix `NotificationHub`'s guard so it actually rejects a `sub`-less principal. Publish Kernel → consumers.
  (Own MED tech-debt item in `api/TECH_DEBT.md`.)

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
- **`Customer/.../Ticket/.../TicketService.cs:150,156`** (`currentUser.Email ?? string.Empty`) +
  **`TicketPaymentProcessor.cs:50`** (`GetValueOrDefault("fromUserEmail", string.Empty)`) → the
  `TicketDto.UserEmail` item: drop `UserEmail` from `TicketDto` (SPA reads it from auth state), make the
  list reads queryable projections (exclude `QrCode`), fail-closed/remove the `fromUserEmail` metadata
  fallback. Boundary-free (Customer + SPA).

### Plus (rides a cutover)
- `Messaging.AzureServiceBus/Options/AzureServiceBusOptions.cs` — `= ""` property defaults → `null!`
  (PUBLISHED Messaging package; goes with the config/Messaging publish).

## Current worktree state — `Fix/TechDebtSweep` (off `origin/master`)

Path: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Fix\TechDebtSweep`

**Committed on this branch (builds verified green):**
- **#3 ConcertDetailsResponse** (Cat E) — `0c033bad`. C# `string?` + mapper coercions dropped + shared
  TS optional. B2B build + all 4 web builds green.
- **#2 dup-app 500→400** (B2B LOW) — `f72c939c`. Guard + `ExistsForOpportunityAndArtistAsync` + unit
  test. B2B build green; **unit-test RUN unconfirmed** (fresh test-project restore was stuck) — re-run
  `Concert.UnitTests`. Integration test (apply-after-withdraw → 400) still TODO (Docker).
- **#4 web buy-tickets** (app/web MED) — `02781a8c`. Single `ConcertCard` reflow. All 4 web builds
  green. Narrow-viewport E2E scenario still TODO (Docker).

**Still uncommitted in the worktree:**
- `api/Concertable.Payment/.../EscrowService.cs` — the Payment `Result.Ok<T?>` redundant-type-arg
  cleanup (Payment LOW item). **UNVERIFIED** (Payment not built). Build Payment, then commit or discard.
- `app/web/b2b/{artist,venue}/src/routeTree.gen.ts` + `app/web/customer/src/routeTree.gen.ts` —
  regenerated by the vite builds, not part of any fix. Discard (`git checkout --`) or ignore.

**Reverted / not a fake fix:** the two Payment gRPC mapper edits (Cat B) were reverted to `?? ""` —
they belong to the proto cutover, not a cosmetic swap.

**Not started:** ClientSecret → `string.Empty`; Category A fail-fast; `WebhookService`; `TicketDto.UserEmail`;
Cat B/C cutovers; `AzureServiceBusOptions` defaults.

## PR sequence (multi-PR by necessity — cutovers can't share a branch)

1. **PR1 — `Fix/TechDebtSweep` (boundary-free):** #2 dup-app, #3 ConcertDetailsResponse, #4 web
   buy-tickets, `ClientSecret` → `string.Empty`, `TicketDto.UserEmail`. One commit per fix; update the
   nearest `TECH_DEBT.md` in the same commit. Verify: 4 web builds + touched `dotnet build` + unit tests.
2. **PR2 — Category A fail-fast** (`Authority`/`ClientId`/`asb` + `WebhookService`). **Verify with the
   integration suite (Docker) that every host AND `WebApplicationFactory("Testing")` still boot.** The
   `AzureServiceBusOptions = ""` → `null!` piece is a Messaging-package change → may split into the cutover.
3. **PR3 — CUTOVER: Kernel `GetId` fail-closed** + `NotificationHub` guard. Publish Kernel → platform-sync.
4. **PR4 — CUTOVER: Payment proto `optional string`** (Cat B). Publish `Payment.Grpc`/`.Client` → platform-sync.

## Resume (fresh context)

1. `cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Fix\TechDebtSweep`; `git status` to see the
   uncommitted #2/#3/#4 edits (or the commits, if already landed).
2. Read this file. Finish verifying the in-flight builds; commit each green fix (one per fix,
   `TECH_DEBT.md` update in the same commit). Don't push without Tommy's go-ahead.
3. Work PR1 to done, then PR2 (Docker-verified), then the cutovers via `/package-cutover`.
4. Delete this plan in the commit that lands the final piece (git history is the archive).
