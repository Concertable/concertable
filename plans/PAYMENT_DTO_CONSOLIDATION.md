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

**Conclusion: a single green PR is impossible for this return-type swap + type deletion.** Migrating the
fixtures fixes the *build* but the collision resurfaces in `TicketApiTests`, and even a green build would
leave the integration `test` job red at runtime (production compiled vs old package, source client loaded
→ type-identity mismatch). Both are required ALLGREEN checks. So path 1 "every step green in one PR" is
**not** achievable here.

**Decision needed — how to land the breaking swap. Recommended, in order:**
1. **Restructured additive sequence (preferred — stays green, no bypass).** The blast radius is tiny:
   4 fixture mocks + **one** production file (`TicketPayment`) + its one test. So:
   - **PR A (additive, green):** add the four records + `EscrowStatus` to `Payment.Contracts` only —
     nothing deleted, nothing's return type changed. Publishes the new Contracts shapes.
   - **PR B (additive, green):** bump consumers' `ConcertablePlatformVersion`; re-point production
     `TicketPayment` to derive from `Contracts.PaymentOutcome` (while `Client.PaymentResponse` still
     exists). Now no production code names a to-be-deleted type.
   - **PR C (the flip):** change `Payment.Client` interface return types to the Contracts shapes, delete
     `Client/*Response.cs`, migrate the 4 fixture mocks in-PR. Needs validating that C's build+tests are
     green given package-vs-source — the remaining open risk; if C still can't go green, fall back to 2.
2. **`--admin`-merge a deliberately-red flip** (original Step 1+2 as written), accepting a short
   knowingly-red `build`/`test` window until the follow-up restores green. Fast, tiny blast radius, but
   merges known-red and bypasses the queue's E2E for that step — **use only with explicit sign-off.**

## Also — update `api/docs/CODE_CONVENTIONS.md`

Part of standardizing this: add a convention documenting the naming rule this refactor establishes —
**`Response` suffix is HTTP-API-layer only; `Result<T>` is the service wrapper; C# service/client DTOs
carry no suffix (Stripe-aligned: `Transfer`/`Refund`/`EscrowDeposit`/`PaymentOutcome`); proto message
names stay `*Response` (wire vocabulary).** Land it in the same effort as the code.

## Steps (expand/contract)

- [ ] **Step 1 — introduce shared DTOs in `Payment.Contracts`; migrate Payment's own service + client.**
  - Add the four records to `Payment.Contracts`.
  - Point `Payment.Application`, `Payment.Infrastructure`, and `Payment.Client` at them; update both
    mapper sets. Delete `Payment.Application/DTOs/ServiceDtos.cs` copies and `Payment.Client/*Response.cs`.
  - **This changes the published contract** (client methods now return `Contracts.Transfer` etc.).
  - **Gotcha:** `Transfer`/`Refund` collide with the Stripe SDK (`Stripe.Transfer`/`Stripe.Refund`) in
    any file with `using Stripe;` (e.g. `MockEscrowClient`). Add `using Transfer = Concertable.Payment.Contracts.Transfer;`
    (and `Refund`) aliases in those files.
  - **Gate:** `Concertable.Payment.slnx` builds green (Payment is self-contained here — all source).
    The root `Concertable.slnx` / CI will be red on B2B+Customer until Step 2, because they still pin the
    old package — expected. Merge order: this publishes new `Payment.Contracts` + `Payment.Client`.

- [ ] **Step 2 — migrate B2B + Customer consumers to the shared types.**
  - Bump each service's `ConcertablePlatformVersion` pin to the version published by Step 1.
  - Replace `*Response` usages: `TicketPayment : PaymentOutcome`; the escrow/manager/customer mocks;
    any other named usages (most consumer code uses `var` and is unaffected).
  - **Gate:** `Concertable.B2B.slnx` + `Concertable.Customer.slnx` build; integration tests
    (`integration-debug`). Root `Concertable.slnx` green again.

- [ ] **Step 3 — remove the old names.**
  - Delete any leftover `*Response` shims and the `MA0053` suppression that references
    `Payment.Client.PaymentResponse` (retarget/remove).
  - **Gate:** full `Concertable.slnx` green; `git rm` this plan file in the commit that closes Step 3.

## Notes / decisions carried forward

- **Proto stays `*Response`.** Only the C# DTOs change.
- **Naming = Stripe-aligned** (`Transfer`/`Refund`/`EscrowDeposit`/`PaymentOutcome`) — decided on
  Feature/EscrowRefund. Accept the Stripe-SDK name collision (handled by `using` aliases where needed).
- The service-layer rename (Phase 1 of Feature/EscrowRefund) already did the `Application/DTOs` half of
  Step 1; Step 1 here relocates those into `Payment.Contracts` and deletes the `Application` copies.
