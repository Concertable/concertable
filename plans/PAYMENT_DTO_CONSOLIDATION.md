# Payment DTO Consolidation — one shared shape, kill the `Response` suffix

**Goal:** replace Payment's duplicated service/client DTOs with a **single shared record per shape**
in `Payment.Contracts`, and drop the `Response` suffix from the C# DTOs (it's wrong: `Result<T>` is
already the service wrapper, and `Response` is reserved for the HTTP API layer). Proto message names
stay `*Response` — that's the native gRPC RPC vocabulary and it's wire-only.

## Why this exists (the smell)

Today the same escrow shapes exist **three times**:

| Layer | Type (today) | Notes |
|---|---|---|
| Service DTO | `Payment.Application/DTOs/ServiceDtos.cs` — `EscrowDeposit`/`Transfer`/`Refund`/`PaymentOutcome` | already de-`Response`d (Phase 1 of Feature/EscrowRefund); **internal** to Payment |
| Proto message | `Payment.Client/Protos/payment.proto` — `EscrowResponse`/`TransferResponse`/`PaymentResponse`/`RefundResponse` | generated wire type — **stays `*Response`** |
| Client DTO | `Payment.Client` — `EscrowResponse`/`TransferResponse`/`PaymentResponse`/`RefundResponse` | **public**, consumed by B2B + Customer |

The service DTO and client DTO are hand-duplicated and coincide in shape. They should be **one** type.

## Target design

One record per shape in **`Payment.Contracts`** (the only project both `Payment.Application` and
`Payment.Client` reference):

- `EscrowDeposit(int EscrowId, string ChargeId, EscrowStatus Status, string? ClientSecret = null)`
- `Transfer(string TransferId)`
- `Refund(string RefundId)`
- `PaymentOutcome { bool RequiresAction; string? ClientSecret; string? TransactionId; }`

Then:
- `IEscrowService`/`IPaymentManager` (service) return the Contracts types.
- `IEscrowClient`/`IManagerPaymentClient`/`ICustomerPaymentClient` (client) return the Contracts types.
- Both the **server-side** mappers (`Payment.Infrastructure/Grpc/*Mappers`) and the **client-side**
  mappers (`Payment.Client/Adapters/*Mappers`) map proto ⇄ the shared Contracts DTO.
- Delete the `Payment.Application/DTOs` copies and the `Payment.Client/*Response.cs` copies.

## Why it can NOT be one PR (the constraint that bit us)

`Payment.Contracts` and `Payment.Client` are **published NuGet packages** (org GitHub Packages feed,
lockstep MinVer via `$(ConcertablePlatformVersion)`). B2B and Customer consume them by version — they
compile against the **published package**, not the source in the same solution. Proven: a consumer's
`project.assets.json` resolves `Concertable.Payment.Client/0.1.0-alpha.0.536` with `"type": "package"`.

A **return-type change has no back-compat shim** (you can't have a method return both the old and new
type). So this is a breaking package change and must go **expand/contract** across merges — publish the
new shape first, migrate consumers against the published package, then delete the old.

## ⚠️ BLOCKER — Step 1 as written can't pass the ALLGREEN merge queue (resolve before coding)

Investigated 2026-07-07 (deferred, not yet started). The plan's Step 1 does a **hard return-type swap**
in Payment.Client source. That is **not mergeable as-is** under this repo's merge queue:

- The required **`build` job runs `dotnet build api/Concertable.slnx`** — the *full* solution, which
  includes Payment.Client **source** (`.github/workflows/test.yml`).
- B2B/Customer *production* code consumes Payment.Client/Contracts as **published packages** (old shape),
  but their **test fixtures `ProjectReference` the source** — confirmed
  `api/Concertable.B2B/tests/Concertable.B2B.IntegrationTests.Fixtures/…csproj:20-21` (both
  `Payment.Client` and `Payment.Contracts` as project refs), and the slnx lists the source projects
  (`Concertable.slnx:167-168`).
- So the instant Step 1 changes the source return types, the full-solution build has the test graph
  seeing the **new** shape while production binds the **old** package → mixed-reference collision → the
  `build` job (a required check) goes **red**. The plan's "root CI red until Step 2 — expected" line is
  **incompatible with ALLGREEN** — a red `build` can't merge through the queue.
- Contrast the escrow work (PR1): that was **additive** (a new method), so every step stayed green. A
  return-type swap has no such luxury.

### ✅ Path 3 executed 2026-07-07 (branch `Feature/PaymentDtoConsolidation`) — empirical red/green

Made the full Step 1 edits (Payment-internal only) on the branch and built. **The Payment-internal
refactor is sound; the cross-package swap is confirmed un-green-able in one PR.** Results:

- **`Concertable.Payment.slnx` → GREEN** (0 errors). Payment is all-source here, so moving the four
  DTOs + `EscrowStatus` into `Payment.Contracts`, wiring `Domain → Contracts` (new ProjectReference for
  the shared enum), retargeting both mapper sets and the `Stripe.*` aliases, and deleting the old
  `Application/DTOs` + `Client/*Response.cs` copies all compiles clean.
- **`api/Concertable.slnx` (full) → RED, 20 errors, ALL in the four B2B/Customer integration-test
  fixture mocks** (`MockEscrowClient`, `MockEscrowClientFail`, `MockManagerPaymentClient`,
  `MockCustomerPaymentClient`). CS0246 (old `*Response` names gone) + CS0738 (mock return types no
  longer match the source interface). **Production B2B/Customer code compiled clean** — it binds the
  *old published package*, the fixtures bind the *source* (project ref). Exactly the predicted split.
- **Second experiment — co-migrated the four mocks to the new types and rebuilt → STILL RED, 8 errors**,
  and the residual errors are the real mixed-reference collision, not fixable in this PR:
  - **CS7069 / CS1061 in `TicketApiTests.cs`**: production `TicketPayment : Client.PaymentResponse`
    binds the *package* (old), but the test project also pulls *source* `Payment.Client` (which deleted
    `PaymentResponse`) → base type "could not be found", so `TicketPayment` loses `.TransactionId` /
    `.RequiresAction`. Unfixable until production `TicketPayment` moves off `Client.PaymentResponse`,
    which needs the republished package.
  - **CS0104** `Transfer`/`Refund` ambiguous with `Stripe.*` in `MockEscrowClient` (has `using Stripe;`)
    — the exact alias gotcha noted below; consumer mocks need the same `using` alias.
  (These experimental mock edits were reverted; the branch holds only the clean Payment-internal Step 1.)

### ✅ Resolved 2026-07-07 — root cause was the fixtures, not the swap itself

The `build` job (`dotnet build api/Concertable.slnx`, which every required `carve-*`/`e2e-*` check
`needs:`) went red only because the B2B/Customer **integration-test fixtures `ProjectReference`d Payment
source** — so the test graph compiled against Payment's *new* shape while production bound the *old*
package, and they collided (fixture mocks + `TicketApiTests`). That source ref was a latent boundary
leak: the mocks implemented Payment's *source* `IEscrowClient`, not the *package* one production binds —
only accidentally compatible while shapes matched.

**Fix:** the fixtures now consume `Payment.Client`/`Contracts` as the **published package** (like
production). With that, the full-solution build is **green** (verified: 0 errors) with the Payment
rename in place — nothing references Payment's source shape, so the swap no longer breaks the build. The
consumer migration is then carried by the **platform-sync** machinery (`plans/PLATFORM_PACKAGE_SYNC.md`):
merge the Payment side → new packages publish → the auto sync-PR bumps the pin → it goes red at exactly
the 4 fixture mocks + `TicketPayment` → migrate them **in that PR** → green → merges. Every step green,
no `--admin` bypass.

## Steps

- [x] **CODE_CONVENTIONS.md naming rule** — `Response` = HTTP-API layer only; `Result<T>` is the service
  wrapper; C# service/client DTOs carry no suffix (Stripe-aligned `Transfer`/`Refund`/`EscrowDeposit`/
  `PaymentOutcome`); proto message names stay `*Response`. (Landed on branch `Feature/PaymentDtoConsolidation`.)

- [x] **Step 1 — shared DTOs in `Payment.Contracts` + migrate Payment's own service + client + fixtures.**
  On branch `Feature/PaymentDtoConsolidation`:
  - Four records + `EscrowStatus` moved to `Payment.Contracts`; `Domain`/`Application`/`Infrastructure`/
    `Client` retargeted; both mapper sets + the `Stripe.*` aliases fixed; old `Application/DTOs` +
    `Client/*Response.cs` deleted.
  - **B2B + Customer integration-test fixtures switched from a Payment-source `ProjectReference` to the
    published `PackageReference`** — the change that makes the full build green (the fixtures no longer
    compile against Payment's new source shape).
  - **Gate met:** `Concertable.Payment.slnx` green + 30 unit tests; **full `api/Concertable.slnx` green
    (0 errors)** with the rename in place. Ready to open the PR.

- [ ] **Step 2 — merge Step 1, then migrate consumers via the auto sync-PR.**
  - Merge the Step-1 PR → new `Payment.Client`/`Contracts` publish.
  - `platform-sync` auto-opens `chore/platform-sync-<ver>`, red at the 4 fixture mocks
    (`MockEscrowClient`/`Fail`/`Manager`/`Customer`) + production `TicketPayment : PaymentResponse`.
  - Migrate them there: `TicketPayment : PaymentOutcome`; mocks return the Contracts types
    (`EscrowDeposit`/`Transfer`/`Refund`/`PaymentOutcome`); add `using Transfer = …Contracts.Transfer;`
    (+`Refund`) in `MockEscrowClient` (has `using Stripe;`). Green → merges.
  - **Gate:** full `Concertable.slnx` green; `git rm` this plan file in that commit.

## Notes / decisions carried forward

- **Proto stays `*Response`.** Only the C# DTOs change.
- **Naming = Stripe-aligned** (`Transfer`/`Refund`/`EscrowDeposit`/`PaymentOutcome`) — decided on
  Feature/EscrowRefund. Accept the Stripe-SDK name collision (handled by `using` aliases where needed).
- **`MA0053` suppression** for the unsealed `PaymentOutcome` now lives in `Payment.Contracts`
  (`GlobalSuppressions.cs`), retargeted from the old `Payment.Client.PaymentResponse`.
