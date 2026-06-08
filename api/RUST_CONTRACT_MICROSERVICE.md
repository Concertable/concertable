# Rust Contract Settlement & Lifecycle Engine — design + build plan

**Status:** decided 2026-06-02. This is the complete, self-contained handoff for building the service.
**One-line summary:** extract B2B's in-process contract settlement/lifecycle workflow into a standalone
**Rust** microservice that is a **stateless gRPC decision engine** — it owns the *rules + settlement math*
(enforced as compile errors via traits + typestate); B2B keeps the *state* and performs the *effects*.

> **Coaching mode — READ FIRST (maintainer may change this).** The maintainer is new to Rust (knows the
> paradigm concepts — ADTs, typeclasses, FP — but not the language) and is learning it *by writing this
> service themselves*. They own and maintain it long-term. So the AI's job here is **coach, not author**:
>
> - **Default = "maintainer writes, AI coaches."** Do **not** pre-write modules. For each module: give a
>   short concept primer + the relevant *Rust Book* section, point at the Appendix A spec slice, then let the
>   maintainer write it. Review every line, explain *why* the compiler objects (its errors are a teaching
>   tool), and set small "make this fail to compile" exercises. Optimise for the maintainer understanding
>   every line over throughput.
> - **Only when the maintainer says "alternate"** do you write a module as a fully-explained worked example
>   for them to study — then hand the next similar one back to them.
> - Prefer idiomatic Rust over clever tricks; introduce concepts incrementally, one module at a time, rather
>   than dumping the whole design. Nudge installing the toolchain so `cargo check`/`clippy` can grade the
>   maintainer's code directly.
> - **Anchor on .NET, don't assume FP fluency.** The maintainer's solid ground is C#/.NET — explain by
>   analogy to it (trait ≈ interface, `Result` ≈ a forced `Try`/`bool`+`out`, `cargo` ≈ NuGet+MSBuild). The
>   paradigm/type-theory vocabulary (ADT, sum/product type, typeclass, affine/linear types, variance,
>   monomorphisation) should be **explained when it comes up, not assumed** — a one-line "what this means and
>   the C# equivalent" each time, even if the maintainer has heard the term. Better to over-explain a concept
>   than skip it. The genuinely new-to-everyone material is ownership/borrowing/moves and trait
>   coherence/sealing; give those the most room.
>
> This note is the maintainer's to edit — if it conflicts with a later instruction in-session, the
> in-session instruction wins.

---

## How to use this document (START HERE)

This file is the single source of truth. A fresh session needs **nothing outside this file** except the
canonical repo docs linked in §13. Everything required to begin — the full reference implementation, the
crate layout, the toolchain bootstrap, the phase plan, and the test spec — is here.

- **First task is Phase 1** (§10): a pure-library Rust crate implementing the domain model with the
  invariants as compile errors. Phase 1 is **not blocked by any open question** (§12).
- The complete, hand-verified reference implementation to port is in **Appendix A**. It has not been
  compiled (Rust was not installed when this was written), so **Phase 1, step 1 is `cargo check` and fix any
  drift** — treat Appendix A as the intended shape, not infallible bytes.
- Do **not** re-litigate the language or design choice (§1, §2). Do **not** reintroduce the runtime-dispatch
  patterns called out in §5.

**Extraction source (C# treated as a spec, not a template):** B2B `Modules/Concert` workflow —
`AcceptExecutor`, `ApplyExecutor`, `VerifyExecutor`, `SettleExecutor`, `*AcceptStep`, `ConcertWorkflowFactory`,
`ConcertWorkflowBuilder`, `ConcertTransitionValidator`. We keep *which contract has which capability* and
*the legal per-variant path*; we replace the enum→factory, enum-keyed DI, the `switch` on interface type with
its `_ => throw "does not support…"` arm, the downcasts, and the sequence-array validator with static trait
dispatch + typestate.

---

## 1. Decision summary

- **Language: Rust.** Chosen on type-system fit for *this* problem; real-world fit seals it.
- **Design: capability dispatch (behaviour-centric).** Capabilities are traits; each contract `impl`s the
  subset it has; call sites name the capability, never the contract. **Not** a closed discriminated-union of
  contracts matched in the interior (non-additive on new contracts); **not** tagless-final (its
  multi-interpreter strength is unused here and it erases the capability-precise types we want).
- **Lifecycle: typestate.** The stage is a phantom type parameter; illegal transitions don't compile; the
  legal path differs per variant; transitions consume `self` by value, so reusing a stale / already-settled
  handle is a compile error (affine ownership — no Haskell-style `-XLinearTypes` needed).
- **One dynamic edge.** The only runtime dispatch is the inbound request parse (untyped wire → typed value).
  The interior is 100% static: no `dyn`, no downcast, no registry, no capability `switch`.
- **Service shape: stateless decision engine, synchronous gRPC** (§7).

---

## 2. Why Rust (condensed — settled, do not relitigate)

**Type-system axis.** In the abstract, Haskell is more powerful (HKT, type families, existentials) and
Idris 2 / Lean 4 more powerful still (dependent + linear). But for *this* problem the only discriminator
that is actually **exercised** is linearity: transitions consume `self`, giving no-stale / no-double-settle
for free. Haskell needs `-XLinearTypes` or an indexed monad to match it. Haskell's compensating superpower —
packing an open, multi-capability existential at the erasure boundary, which Rust's single-non-auto-trait,
object-safety-restricted trait objects cannot — is **latent here**, because the architecture (static
interior, single edge) dispatches the dynamic tag into monomorphic code and never needs a capability-erased
existential. Both-axes additivity is a tie: both close the expression problem because an `impl`/`instance`
is declared *separately from the type* — the exact thing C# interfaces can't do, which is why the current C#
is only half-additive (adding a capability edits the contract class).

**F#, judged honestly:** no typeclasses, so capability dispatch falls back to interfaces + `:?` downcast
(identical to the C# being replaced) or to SRTP, which is too limited to carry the typestate handle, the
arity refinement, or the boundary. Units-of-measure is a real win for money but doesn't rescue dispatch.
Best real-world fit (it *is* .NET), worst type-system fit — that tension is why it loses.

**Real-world axis.** Rust wins outright: `tonic` (gRPC) + `prost`, `tokio`, `rust_decimal` (exact decimal
money), AMQP for ASB, mainstream hireability, strong portfolio. A non-.NET service in the fleet is fine.

**The one place a runtime check remains:** the inbound request parse — turning an untyped wire message into
a typed contract + claimed stage. One total parse at the edge; everything past it is static.

---

## 3. Invariants — the contract this service must always uphold

Each must hold by construction. Note which are **compile-time** vs **boundary-checked** (the one sanctioned
runtime bucket — value-range checks at the single construction edge, same bucket as the request parse):

| # | Invariant | Mechanism | Enforced |
|---|-----------|-----------|----------|
| **A** | Accept arity per variant; wrong-arity call rejected | one `Accept` head trait with `type In`; arities are supertrait refinements (`SimpleAccept`, `PaidAccept`, `DepositAccept`) | **compile-time** (`trybuild`) |
| **B** | A partial capability invoked on a variant lacking it | capability is a trait; absence ⇒ unsatisfied bound, no registry/downcast | **compile-time** (`trybuild`) |
| **C** | Illegal/skipped transition; per-variant path; stale-handle reuse / double-settle | stage as phantom type; transitions `Concert<C, From> -> Concert<C, To>`; `self`-by-value ⇒ affine | **compile-time** (`trybuild`) |
| **D** | Capability callable only where present; no Option/nil; call site contract-agnostic; shared behaviour reused | trait bounds + generics; shared step bodies are default methods | **compile-time** |
| **E.1** | No mixed-currency arithmetic | `Money<C>` phantom currency tag | **compile-time** (`trybuild`) |
| **E.2** | No settling an unconfirmed contract | `settle` exists only on `Concert<C, Accepted>` | **compile-time** (`trybuild`) |
| **E.3** | No negative money; percentage in 0–100 | checked smart constructors (`Money::parse`, `Percent::parse`); private fields behind a module wall; money API has **no subtraction**, so non-negativity is preserved through the interior | **boundary-checked** (unit tests) |

`rust_decimal::Decimal` is required for exact money, so a negative value is *representable* — hence E.3 is a
constructor check at the single money-entry boundary, not a type-level guarantee. That is consistent with
"parse, don't validate": money is validated once where it enters, and the interior cannot reintroduce a
negative.

---

## 4. Core design (skeleton — full reference in Appendix A)

```rust
// E: validated value types behind a module wall (private fields; checked constructors).
pub mod money {
    use rust_decimal::{Decimal, RoundingStrategy};
    use rust_decimal_macros::dec;
    use std::marker::PhantomData;
    use crate::domain::ParseError;

    pub trait Currency { const CODE: &'static str; }
    pub enum Gbp {} impl Currency for Gbp { const CODE: &'static str = "GBP"; }
    pub enum Usd {} impl Currency for Usd { const CODE: &'static str = "USD"; }

    pub struct Money<C: Currency> { amount: Decimal, _c: PhantomData<C> } // private field; GBP+USD won't unify
    impl<C: Currency> Clone for Money<C> { fn clone(&self) -> Self { *self } }  // manual: derive would over-constrain C
    impl<C: Currency> Copy  for Money<C> {}
    impl<C: Currency> Money<C> {
        pub fn parse(amount: Decimal) -> Result<Self, ParseError> {
            if amount.is_sign_negative() { Err(ParseError::NegativeMoney) } else { Ok(Money { amount, _c: PhantomData }) }
        }
        pub fn amount(self) -> Decimal { self.amount }
        pub fn add(self, rhs: Money<C>) -> Money<C> { Money { amount: self.amount + rhs.amount, _c: PhantomData } } // same C only
        pub fn pct(self, p: Percent) -> Money<C> {                                  // rounding policy: banker's, 2dp
            let raw = self.amount * p.value() / dec!(100);
            Money { amount: raw.round_dp_with_strategy(2, RoundingStrategy::MidpointNearestEven), _c: PhantomData }
        }
        pub fn max(self, rhs: Money<C>) -> Money<C> { if self.amount >= rhs.amount { self } else { rhs } }
    }

    #[derive(Clone, Copy)]
    pub struct Percent(Decimal); // field private to this module
    impl Percent {
        pub fn parse(p: Decimal) -> Result<Percent, ParseError> {
            if p >= Decimal::ZERO && p <= dec!(100) { Ok(Percent(p)) } else { Err(ParseError::PercentOutOfRange) }
        }
        pub(crate) fn value(self) -> Decimal { self.0 }
    }
}
```

The rest of the core — `stage` (phantom stages), `Concert<C, S>` + its affine transitions, the `Accept` head
with `SimpleAccept`/`PaidAccept` refinements (shared step bodies written once as default methods), `Verify`,
`Settle`, the `Lifecycle` trait + the generic `drive` — is in **Appendix A**, which is the canonical listing
to port.

**Contract → capability matrix (faithful to the live engine):**

| Contract | Accept arity | Verify? | Settle | Money flow |
|----------|--------------|---------|--------|------------|
| FlatFee | simple (`()`) | no | fixed fee | Venue → Artist |
| VenueHire | simple (`()`) | no | fixed hire fee | **Artist → Venue (reverse)** |
| DoorSplit | paid (`PaymentMethod`) | yes | % of gross | Venue → Artist |
| Versus | paid (`PaymentMethod`) | yes | max(guarantee, % of gross) | Venue → Artist |

`paid_step` is the shared accept body for DoorSplit + Versus (the live `PaidAcceptStep`); `simple_step` for
FlatFee + VenueHire. Per-variant path: simple = `None→Applied→Accepted`; paid =
`None→Applied→Verified→Accepted`. The tail `Accepted→Settled→(Finished)` is **shared in shape, not
mandatory**: `finish` is gated behind a `Finish` capability, so a contract may legitimately **terminate at
`Settled`** by simply not implementing it — calling `.finish()` on such a contract is then a compile error,
not a runtime no-op. All four current contracts `impl Finish` (faithful to the live engine, where each has a
`*FinishStep`), but the path is **per-contract, not a universal pipeline**: each variant owns its own
transition graph and its own terminal stage. (This corrects an earlier "every contract runs to Finished"
assumption — which is exactly what typestate is for, and what the live global `ConcertStage` enum + per-entity
`AdvanceStage` guards make awkward to vary per contract; see its own `ARCHITECTURE.md §6.1`.)

**⚠️ Versus settlement-formula fidelity flag (confirm before §11).** This matrix and Appendix A encode Versus
as `max(guarantee, % of gross)` — the classic industry "versus" deal, and what the evaluation brief stated.
The **live C# does not**: `VersusFinishStep` and `VersusContractEntity.CalculateArtistShare` both compute
`guarantee + (gross × pct)` (additive — guarantee *plus* a percentage, not the greater of the two). Those are
different amounts of real money. Decide which is correct before writing settlement-math tests; the live code
may itself be the bug. Tracked in §13.

**Settle is optional (NoOp eliminated).** The old C# forced an `ISettleStep` on every contract, so FlatFee and
VenueHire carried a `NoOpSettleStep` stub that did nothing (`NoOpSettleStep.cs` — `=> Task.CompletedTask`). In
Rust they simply **don't `impl Settle`** and **skip the `Settled` stage**: their path is `…→Accepted→Finished`,
and their money moves as escrow effects (capture at accept, release at finish — §7.8), not a settle payment.
Only DoorSplit and Versus `impl Settle` (the deferred contracts). `finish` therefore starts from a
**per-contract stage** (`Finish::From` = `Accepted` for escrow contracts, `Settled` for deferred), so each
contract walks only its **own subset** of `ConcertStage`, enforced at compile time even though all six enum
values still exist on B2B's persisted column. Same shape as `Verify`: `Verified` is a legal enum value, but
`flatfee.apply().verify()` does not compile because FlatFee has no `impl Verify`. (`ConcertStage` stays in B2B's
EF Core as the system-of-record column — the engine reconstructs and validates the stage per request, it never
stores it; §7.1/§7.2.)

---

## 5. The one dynamic edge (LOCKED)

**Do NOT** match on capability at the boundary (e.g. `match workflow { IAcceptsPaid … }`): that is runtime
dispatch with a partial `_ => throw`, and it is non-additive — a new Accept arity edits the switch. **Do
NOT** match a discriminated-union of contracts at every operation (non-additive on new contracts). **Do
NOT** reintroduce a registry/factory keyed by tag (that is `ConcertWorkflowFactory` reborn: relocated
enumeration + runtime indirection + lost exhaustiveness).

**DO** parse once, then let trait resolution dispatch. "If it's paid, do X" is expressed as
`impl PaidAccept for DoorSplit {}` — the `if` is the trait bound, the `do X` is the method body, the compiler
does the matching. Match on the **contract-type discriminator** (and, per RPC, the **claimed stage**),
ideally an exhaustive enum so adding a contract fails to compile until its arm exists. The per-variant path
is a property of the contract via a one-line `Lifecycle::reach_accept`, so the dispatcher names no capability
and chooses no pipeline (see `drive` in Appendix A).

The production surface (Phase 2) is **per-operation** gRPC; each handler is the boundary: parse the request →
reconstitute `Concert<C, ClaimedStage>` → apply the one typed transition → return result or a **typed
rejection**. The only runtime checks live here and are total: unknown contract, illegal (operation, stage)
combination, and the one explicitly-sanctioned request-data check — a missing payment method on a
paid-accept contract.

---

## 6. Both-axes additivity contract (the regression bar)

Growth happens on two axes — new **contract types** and new **capabilities** — and both must stay **purely
additive** (no edits to existing code). These six scenarios are implemented in Appendix A as living proof and
must keep compiling:

- **(i) Shared accept reused.** `PaidAccept` (the shared "accept step", written once) used by DoorSplit +
  Versus; `SimpleAccept` by FlatFee + VenueHire; a fourth type with **no Accept at all** (`CompSlot`) —
  calling accept on it must not compile.
- **(ii) New `Escrow` capability**, shared via a default-method body, on a subset (VenueHire + Versus) + an
  `Escrow`-gated transition. FlatFee/DoorSplit can't call it.
- **(iii) Third Accept arity** (`DepositAccept`, deposit + payment method) under the **same** `Accept` head;
  `accept` is already `C::In`-generic, so the transition is untouched.
- **(iv) New contract** (`Residency`) reusing the shared `PaidAccept` plus **one new** capability (`Renew`).
- **(v) Capability unique to one contract** (`IssueInvoice` on FlatFee only); callable only on FlatFee.
- **(vi) Brand-new contract** (`SponsorDeal`) reusing `PaidAccept` + `Verify`, wired into the lifecycle — the
  only edit to existing code is one new arm at the parse (Phase 2), nothing else.
- **(vii) Settle-terminal contract** — a contract whose path **ends at `Settled`** (no `impl Finish`), proving
  per-contract terminal stages are additive: `.finish()` on it must not compile, while every existing contract
  still finishes. (Corrects the earlier universal `Accepted→Settled→Finished` tail.)

For every scenario: no nullable/Option slot reappears; no central dispatcher is edited (except (vi)'s single
parse arm in Phase 2); both axes stay additive. The compile-error cases (A–E, plus (i)/(v)'s "must not
compile") are enforced by the `trybuild` suite (§11).

---

## 7. Service shape & fleet integration

### 7.1 Classification: stateless decision engine (B2B-scoped, sync gRPC)

Rust owns the **rules + settlement math**; B2B owns the **state** and the **effects**. The engine is
effectively a pure function: `(contract, claimed stage, operation inputs) → legal next stage + computed
settlement, or a typed rejection`. No database initially.

It is **adapter-style** (sync-callable) but **not a shared adapter** (Customer never settles contracts) and
**not a peer data service** (B2B calls it synchronously, which the architecture forbids *between* data
services). It is **B2B's private downstream service** — the same dependency shape as B2B → Payment. Its
Aspire wiring therefore lives in **B2B's host**, never in `Concertable.AppHost.Shared` (which is only for
every-host dependencies).

Why not state-owning/hybrid: the lifecycle stage already has an owner — B2B's
`ApplicationEntity`/`BookingEntity`/`ConcertEntity`. Owning it twice creates dual systems of record that
drift, and drags persistence + Payment-calling into the engine — the re-monolithing `api/ARCHITECTURE.md`
forbids. Hybrid earns its complexity only if the engine must be authoritative for lifecycle independently of
B2B, or needs an autonomous audit/event stream — both deferrable and purely additive (§7.7).

### 7.2 Brain vs hands — what moves, what stays

| Concern | Owner |
|---|---|
| Which transitions are legal per contract (typestate path, arity) | **Rust engine** |
| Settlement math (who pays whom; door %, Versus max, VenueHire reverse) | **Rust engine** |
| Lifecycle stage of an application/booking/concert | **B2B** (system of record) |
| Creating booking rows / concert drafts; holding/capturing escrow via Payment | **B2B** (the side-effecting `*Step`s stay) |
| Publishing `ConcertSettledEvent` etc. (B2B already owns the outbox) | **B2B** |

Per-operation flow: B2B calls the engine → engine returns "legal; next stage = X; settlement = …" or a typed
rejection → B2B persists stage X and performs the effects. The compile-time invariants live in the brain;
the brain has no side effects.

### 7.3 gRPC surface (Phase 2)

One RPC per lifecycle operation; the from-stage is implied by the RPC and **validated** against the request's
claimed stage (defense in depth — the engine does not trust the caller's ordering).

```proto
syntax = "proto3";
package concertable.contract.v1;

enum Stage { STAGE_UNSPECIFIED = 0; NONE = 1; APPLIED = 2; VERIFIED = 3; ACCEPTED = 4; SETTLED = 5; FINISHED = 6; }
enum PaymentMethod { PAYMENT_METHOD_UNSPECIFIED = 0; CASH = 1; TRANSFER = 2; }

message Money { string currency = 1; string amount = 2; }            // decimal as string — never float
message Settlement { string from = 1; string to = 2; Money amount = 3; }

// Typed oneof per contract (DECIDED) — the variant tag IS the discriminant the boundary matches on.
message FlatFee   { Money fee = 1; }
message DoorSplit { string artist_pct = 1; }                         // decimal 0–100 as string
message VenueHire { Money hire_fee = 1; }
message Versus    { Money guarantee = 1; string artist_pct = 2; }
message Contract  { oneof kind { FlatFee flat_fee = 1; DoorSplit door_split = 2; VenueHire venue_hire = 3; Versus versus = 4; } }

message SettleRequest  { Contract contract = 1; Stage claimed_stage = 2; Money gross = 3; }
message AcceptRequest  { Contract contract = 1; Stage claimed_stage = 2; optional PaymentMethod payment_method = 3; }
message Decision       { Stage next_stage = 1; }
message SettleDecision { Stage next_stage = 1; Settlement settlement = 2; }

service ContractEngine {
  rpc Apply  (ApplyRequest)  returns (Decision);
  rpc Verify (VerifyRequest) returns (Decision);
  rpc Accept (AcceptRequest) returns (Decision);
  rpc Settle (SettleRequest) returns (SettleDecision);
}
```

Each handler matches the contract-type discriminator once, reconstitutes the typed `Concert<C, ClaimedStage>`
(asserting the claimed stage is legal for the operation — the sanctioned runtime check), applies the one
typed transition, and maps typed rejections to `Status::invalid_argument` / `failed_precondition` with
structured detail. The proto is the contract; the Rust server and the C# client stay in sync via a buf
breaking-change check in CI.

### 7.4 Auth, observability, money

- **Auth (Rust resource server):** Duende issues JWTs via client credentials. The engine validates the
  bearer token with `jsonwebtoken`, fetching JWKS from `{Auth authority}/.well-known/openid-configuration`.
  New API scope `contract:settle`, audience `concertable.contract.api`. B2B obtains the token exactly as for
  Payment — `ITokenService.GetTokenAsync("contract:settle")` injected via the gRPC client's
  `AddCallCredentials`. Add the scope + audience in Auth and the client registration in B2B.
- **Observability:** Rust `opentelemetry` + `tracing-opentelemetry` exporting OTLP to
  `OTEL_EXPORTER_OTLP_ENDPOINT` (Aspire injects it — the same collector `ServiceDefaults` uses). Tonic
  server/client interceptors for gRPC spans; propagate W3C tracecontext so B2B→engine calls stitch into one
  trace.
- **Money:** `rust_decimal::Decimal` behind `Money<C>`. Decimal on the wire as a string. Rounding policy is
  banker's rounding to 2 dp, applied in `pct`/settlement (revisit per §12).

### 7.5 Aspire AppHost wiring (B2B host only)

Precedent for running a non-.NET process: `stripe-cli` via `AddExecutable` (run mode) / `AddContainer`
(publish mode), and npm SPAs via `AddNpmApp`. The engine follows the same shape, in **B2B's** host
extensions:

```csharp
// run mode: cargo binary; publish mode: container image from the crate's Dockerfile
var contractEngine = builder.AddContractEngine(auth);      // WithReference(auth) for JWKS; WaitFor(auth)
var api = builder.AddApi<Projects.Concertable_B2B_Web>(/* … */, contractEngine);  // WithReference + WaitFor
```

`AddContractEngine` uses `AddExecutable("contract-engine", "cargo", workingDir, "run", "--release")` in run
mode and `AddContainer`/`AddDockerfile` in publish mode, exposes an HTTPS (HTTP/2) endpoint, and injects the
Auth authority/JWKS + OTLP env vars. B2B reads the engine URL via service discovery
(`services__contract-engine__https__0`) in `AddContractClient`.

### 7.6 Repo layout

`api/` is organised by **service ownership, not language**, so the engine lives there as the first non-.NET
service under `api/` (non-.NET already exists in the repo under `app/`).

```
api/Concertable.Contract/             # service ownership boundary (polyglot: Rust impl + .NET client SDK)
  contract-engine/                    # the Rust crate (server host). cargo-built, not in the .slnx
    Cargo.toml
    src/                              # see §9
    proto/contract.proto             # single source of truth for the gRPC contract (Phase 2)
    Dockerfile                       # publish-mode container (Phase 5)
  Concertable.Contract.Client/        # .NET: generated gRPC client + AddContractClient() (mirrors Concertable.Payment.Client) (Phase 5)
  Concertable.Contract.slnx           # the .NET projects only
```

The Rust crate folder uses **Rust naming** (`contract-engine`, kebab-case), not the .NET dotted-PascalCase
style — matching repo precedent that non-.NET surfaces keep their own ecosystem's conventions (cf. `app/web`,
`app/mobile`). The parent `Concertable.Contract/` is the ownership boundary and carries the fleet branding;
the `.Client`/`.slnx` inside it are .NET and stay PascalCase. (Folder is `contract-engine` = the eventual
service/binary name; the Cargo *package* is `concertable-contract` — see §9. Folder ≠ package name is fine in
Cargo.)

Single source of truth for the proto is the crate's `proto/`; the C# client csproj references it for codegen
(`<Protobuf Include="..\contract-engine\proto\contract.proto" GrpcServices="Client" />`). Split-repo
future: client ships as a private NuGet, engine as a container image — one AppHost line changes.

### 7.7 If you later need state (hybrid migration path)

Adopt only when a concrete need appears (engine authoritative for lifecycle independently of B2B, or an
autonomous audit/event stream). All additive: add `ContractDb` (DB-per-service, `DbContextBase`,
nuke-and-rescaffold migrations); add `Concertable.Contract.Contracts` integration events + an outbox; publish
e.g. `ContractSettledEvent`; subscribe via a `ContractTopology` on ASB `event-` topics; reconstitute
`Concert<C, S>` from the DB row at the runtime stage (the parse then also validates the stored stage). Until
then: YAGNI.

### 7.8 Residual B2B coupling & open scope decisions (the effect boundary)

The brain/hands split (§7.2) is clean for the *decision*, but three contract-shaped concerns currently stay
on the B2B side as effects. Each is a defensible choice — but each leaves a small per-contract seam in B2B, so
name them now rather than discovering them at cutover:

- **Which Payment primitive to call is itself per-contract.** The live `*Step`s map contract → Stripe op
  (`ARCHITECTURE.md §2.8`): FlatFee/VenueHire use `Escrow.Capture`/`Deposit` + `Release`; DoorSplit/Versus use
  `ManagerPayment.PayAsync` off-session. If the engine returns **only** `Settlement{from,to,amount}` (current
  §7.3), B2B keeps a contract-type switch to pick the primitive — so adding a contract still edits B2B's step
  wiring: additivity holds in the *engine* but not the *effects*. **Decision:** accept that residual switch, or
  have the engine return a **closed effect vocabulary** (`Hold｜Capture｜PayOffSession｜Deposit｜Release｜NoOp`
  + parties + amount) so B2B becomes a generic executor with no contract switch. The set is finite precisely
  because the Stripe primitives are (`ARCHITECTURE.md §4.1`), so it maps cleanly to a proto `oneof`.
  Recommended: the effect vocabulary — it is the only way the "no central switch on a new contract" guarantee
  reaches the effect side too.
- **Ticket-revenue payee direction is also per-contract.** `TicketPayeeResolver` (`ARCHITECTURE.md §2.9`):
  VenueHire → artist, everything else → venue. Same rule-shape as `Settlement` direction, which the engine
  already owns — yet it is currently unaddressed by this plan. **Decision:** fold it into the decision (a
  `ticket_payee` field / its own RPC), or declare ticketing out of scope and leave it in B2B.
- **Apply-arity & checkout are modelled as effects, not decisions.** The engine models *accept* arity
  (Simple/Paid/Deposit) but folds *apply* into one no-arg transition, dropping the live
  `IAppliesPaid`/`IApplyCheckoutStep` distinction (VenueHire pays at apply; the per-contract checkout
  *amount*+*label*, e.g. `VenueHireApplyCheckoutStep` → `Checkout(FlatPayment(hire_fee), …, Charge)`). Fine
  **if** those are pure effects — but the checkout amount/label is a per-contract *decision*. **Decision:** add
  a `Quote`/`CheckoutSpec` decision so B2B's checkout steps go generic too, or accept apply/checkout as
  B2B-owned.

**Data-boundary lead (reinforces §7.1 "no DB initially").** The relational seam is **already cut**:
`Concert.Opportunity.ContractId` is a satellite `int` with **no nav and no cross-context SQL FK**
(`ARCHITECTURE.md:70-72`). So keeping the contract tables in B2B's EF Core is correct *and* severs nothing; and
if §7.7's full-ownership ever happens, B2B already references contracts by opaque id, so the move is additive,
not a schema break. **Hard rule either way:** never let an EF Core entity/nav/FK reach into a store the engine
owns, and never let both read the same physical table.

**Wire-decimal precedent (reinforces §7.4).** `Concertable.Payment.Client/Adapters/EscrowClient.cs` already
sends money over gRPC as a string — `Amount = amount.ToString(CultureInfo.InvariantCulture)` over
`payment.proto` — so the engine's `Money{string amount}` is the established fleet convention, not a new one.
Parse to `rust_decimal::Decimal` at the boundary; never a proto `double`.

---

## 8. Toolchain & environment

Target dev box: **Windows 11, PowerShell**. Rust is **not yet installed**. Bootstrap:

```powershell
winget install --id Rustlang.Rustup -e        # then restart the shell so PATH picks up cargo
rustup default stable
rustup component add clippy rustfmt
rustc --version; cargo --version              # verify
```

- Rust on the MSVC target needs the MSVC linker. If `rustup-init` reports it missing, install
  **Visual Studio Build Tools** with the "Desktop development with C++" workload
  (`winget install --id Microsoft.VisualStudio.2022.BuildTools -e`, then add the C++ workload), or accept
  the rustup prompt to install it.
- **Phase 2 only:** `tonic-build` needs `protoc`. Either `winget install --id Google.Protobuf -e` (puts
  `protoc` on PATH) or add the `protoc-bin-vendored` crate and point `tonic-build` at it — decide in Phase 2.
- Use the **PowerShell tool** for `cargo` commands; keep them single-line.

---

## 9. Crate structure (Phase 1)

A pure-library crate, no I/O. Package name `concertable-contract`, edition 2021.

```
contract-engine/
  Cargo.toml
  rust-toolchain.toml # pins the channel for reproducible builds/CI
  src/
    lib.rs            # crate docs + `pub mod` decls + re-exports
    domain.rs         # PaymentMethod, Deposit, Party, Settlement, AcceptOutcome, EscrowReceipt, ParseError
    money.rs          # Currency, Gbp/Usd, Money<C>, Percent (the §4 module)
    stage.rs          # Stage trait + 6 phantom stage markers
    capabilities.rs   # Accept head + SimpleAccept/PaidAccept/DepositAccept; Verify, Settle, Escrow, IssueInvoice, Renew
    contracts.rs      # FlatFee, DoorSplit, VenueHire, Versus + their impls
    growth.rs         # the (i)–(vi) additive examples — living proof of additivity (trim/relocate later)
    lifecycle.rs      # Concert<C,S>, seed, transitions (apply/verify/accept/settle/finish), Lifecycle trait + drive
  tests/
    settlement_math.rs  # per-contract amounts, rounding, Versus max, VenueHire reverse
    legality.rs         # happy-path drive() per contract; boundary checks (negative money, percent range)
    compile_fail.rs     # trybuild harness
    compile_fail/       # one .rs per negative case + its .stderr
```

`Cargo.toml`:

```toml
[package]
name = "concertable-contract"
version = "0.1.0"
edition = "2021"

[dependencies]
rust_decimal = { version = "1", features = ["macros"] }
rust_decimal_macros = "1"

[dev-dependencies]
trybuild = "1"
```

(`thiserror` is optional for `ParseError`; a plain `#[derive(Debug)]` enum is fine for Phase 1.
`tokio`/`tonic`/`prost`/`tonic-build` arrive in Phase 2; `jsonwebtoken`/`reqwest` in Phase 3;
`opentelemetry`/`tracing` in Phase 4.)

---

## 10. Build-out phases

1. **Crate + domain model (Phase 1).** Implement Appendix A across the §9 modules. Land the `trybuild`
   compile-fail suite (§11) + the settlement-math/legality unit tests. Invariants A–E and scenarios (i)–(vi)
   proven in isolation. **Definition of done in §11.**
2. **gRPC boundary.** `proto/contract.proto`; `tonic` server; per-operation handlers = the single parse;
   typed rejections → gRPC status. Boundary tests.
3. **Auth.** JWT validation via JWKS from Auth; scope `contract:settle`; audience. Reject unauthenticated.
4. **Observability.** OTel/OTLP + tonic interceptors; tracecontext propagation.
5. **Aspire + client.** `AddContractEngine` in B2B's host; `Concertable.Contract.Client` + `AddContractClient`
   in B2B; dev secrets.
6. **Cutover.** Delete B2B's `AcceptExecutor`/`ApplyExecutor`/`VerifyExecutor`/`SettleExecutor`,
   `ConcertWorkflowFactory`, `ConcertWorkflowBuilder`, `ConcertTransitionValidator`. B2B calls the engine for
   legality + settlement; the side-effecting `*Step`s (booking creation, escrow capture, concert draft) stay
   in B2B and run on the engine's decision. Verify against existing E2E/integration suites.
7. **(Deferred) state/events** — only if §7.7's trigger appears.

---

## 11. Phase 1 — definition of done & test spec

**Definition of done:**

- `cargo build` and `cargo clippy -- -D warnings` are clean.
- `cargo test` is green: settlement-math + legality unit tests, and the `trybuild` compile-fail suite (every
  negative case fails to compile with the expected error).
- All four contracts **and** the six growth scenarios (Appendix A) compile — additivity proven at build time.
- No `dyn`, no downcast, no runtime type registry anywhere in `src/` (grep to confirm).

**`trybuild` compile-fail cases** (each is a tiny `tests/compile_fail/*.rs` using only the public API, with a
committed `.stderr`):

| File | Code (essence) | Must fail because |
|---|---|---|
| `arity_simple_given_paid.rs` | `seed(flatfee, g).apply().accept(PaymentMethod::Cash)` | expected `()`, found `PaymentMethod` (A) |
| `arity_paid_given_unit.rs` | `seed(versus, g).apply().verify().accept(())` | expected `PaymentMethod`, found `()` (A) |
| `partial_verify.rs` | `seed(flatfee, g).apply().verify()` | `FlatFee: Verify` not satisfied (B) |
| `path_accept_too_early.rs` | `seed(doorsplit, g).apply().accept(pm)` | `DoorSplit: Accept<From = Applied>` not satisfied (C) |
| `path_skip_to_settle.rs` | `seed(flatfee, g).apply().settle()` | no method `settle` on `Concert<_, Applied>` (C) |
| `double_settle_moved.rs` | `let (h,_) = c.settle(); c.settle();` | use of moved value `c` (C) |
| `settle_settled_handle.rs` | `let (h,_) = c.settle(); h.settle();` | no method `settle` on `Concert<_, Settled>` (C) |
| `finish_on_settle_terminal.rs` | `let (h,_) = settle_only.settle(); h.finish();` | `SettleOnly: Finish` not satisfied — contract terminates at `Settled` (C/vii) |
| `settle_on_escrow_contract.rs` | `seed(flatfee, g).apply().accept(()).settle()` | no method `settle` on `Concert<FlatFee, Accepted>` — FlatFee has no `impl Settle` (no NoOp stub) |
| `no_accept_contract.rs` | `seed(CompSlot, g).apply().accept(())` | `CompSlot: Accept<…>` not satisfied (D/i) |
| `mixed_currency.rs` | `gbp_money.add(usd_money)` | expected `Money<Gbp>`, found `Money<Usd>` (E.1) |
| `single_contract_capability.rs` | `issue_invoice(&door_split)` | `DoorSplit: IssueInvoice` not satisfied (v) |
| `percent_ctor_private.rs` | `money::Percent(dec!(50))` | tuple-struct constructor is private — must use `Percent::parse` (E.3 wall) |

**Unit tests (`tests/`):** door-split % with rounding, Versus `max(guarantee, %)` (pending §13 confirmation),
VenueHire reverse direction; FlatFee/VenueHire expose their fee as the escrow charge (they have no `Settle`);
`Money::parse(negative) == Err`, `Percent::parse(150) == Err`, `Percent::parse(100) == Ok`; happy-path
`drive()` for the settling contracts (DoorSplit/Versus), and per-operation legality for the escrow contracts.

---

## 12. Conventions for this crate

- **Library code returns `Result`**; never `panic!`/`unwrap` on caller-supplied data. The `assert!` inside
  `verify()` guards a domain predicate the typestate has already gated — acceptable; document it. `.unwrap()`
  appears in Appendix A's growth examples only on provably-non-negative literals.
- **Errors:** one `ParseError` enum at the boundary; map to gRPC `Status` in Phase 2.
- **Invariant wall:** value types (`Money`, `Percent`) keep **private fields** in the `money` module; the
  only construction path is the checked `parse`. Never add a public field or `new` that bypasses validation.
- **No interior dynamic dispatch:** no `dyn`, no downcast, no registry. New behaviour is a new trait + impls.
- **Formatting/lint:** `rustfmt` default; `clippy` clean (`-D warnings`).
- **Naming:** snake_case modules/functions, PascalCase types; the crate is `concertable-contract`, the Phase-2
  binary `contract-engine`.
- The repo-wide behavioural rules (branch naming `Feature/`/`Refactor/`/`Fix/` + PascalCase; show the staged
  diff and wait for approval before committing; no `Co-Authored-By`/"Generated with" trailers) apply here too.

---

## 13. Decisions

**Decided:**
- **Proto contract params = typed `oneof`** per contract (one variant per type; the variant tag is the
  boundary discriminant) — keeps the compile-time spirit on the wire. Applied in §7.3. (Phase 2.)
- Money = `rust_decimal::Decimal`; rounding = banker's (MidpointNearestEven) to 2 dp.
- Edition 2021; package `concertable-contract`.
- Growth scenarios live in `src/growth.rs` (compiled as additivity proof; relocate/trim later).

**Deferred (needed by the noted phase, not Phase 1):**
- **Auth scope name** (Phase 3): `contract:settle` vs reuse an existing B2B scope.
- **Rounding policy** sign-off (Phase 1/2): confirm banker's/2dp is correct for door splits.
- **Run-mode launch** (Phase 5): `cargo run` vs prebuilt binary path.

**Open — raised in review 2026-06-02 (confirm before the noted phase):**
- **Versus formula (Phase 1, blocks settlement tests):** live C# is `guarantee + (gross × pct)` (additive);
  this plan + the brief say `max(guarantee, %)`. Different money. Confirm the intended semantics; if additive
  is correct, fix Appendix A's `Versus::settle` and §11. The classic industry "versus" is `max`, so the live
  code may be the bug — a product call, not a code call.
- **Effect vocabulary vs settlement-only (Phase 2):** engine returns a closed effect set (B2B keeps no
  contract switch) or just `Settlement` math (B2B retains the contract→primitive map)? See §7.8.
- **TicketPayee scope (Phase 2/6):** engine owns ticket-revenue direction, or it stays in B2B? See §7.8.
- **Apply/checkout scope (Phase 2):** add a `Quote`/`CheckoutSpec` decision, or leave apply-arity + checkout
  amount/label as B2B effects? See §7.8.
- **Per-contract terminal (Phase 1):** `finish` is now `Finish`-gated (§4); add growth scenario (vii) + its
  compile-fail (§6, §11).

---

## 14. Canonical docs

- [`api/ARCHITECTURE.md`](./api/ARCHITECTURE.md) — adapter vs data service; standalone-is-canonical; DB-per-service.
- [`ARCHITECTURE.md`](./ARCHITECTURE.md) — system-wide monorepo-of-convenience / split-repo premise.
- [`api/docs/MICROSERVICES_ARCHITECTURE.md`](./api/docs/MICROSERVICES_ARCHITECTURE.md), [`api/docs/MODULAR_MONOLITH_RULES.md`](./api/docs/MODULAR_MONOLITH_RULES.md).
- [`api/Concertable.B2B/Modules/Contract/ARCHITECTURE.md`](./api/Concertable.B2B/Modules/Contract/ARCHITECTURE.md) — the extraction source.
- `api/Concertable.Payment/` — the template for a sync gRPC service (`.Client` proto, `AddCallCredentials`, `ServiceToken` auth, `payment.proto`).

---

## Appendix A — canonical reference implementation

Hand-verified against the type rules; **not yet compiled** (no toolchain at authoring time). Port into the
§9 modules and run `cargo check` first; fix any drift. Module headers below map to files. `use` lines are
indicative — let the compiler/`cargo fix` settle exact imports.

```rust
// ======================= src/domain.rs =======================
pub mod domain {
    use crate::money::{Gbp, Money};

    #[derive(Clone, Copy)] pub enum PaymentMethod { Cash, Transfer }
    #[derive(Clone, Copy)] pub struct Deposit(pub Money<Gbp>);
    #[derive(Clone, Copy)] pub enum Party { Venue, Artist }

    pub struct Settlement { pub from: Party, pub to: Party, pub amount: Money<Gbp> }
    pub enum AcceptOutcome { StandardBooking, DeferredBooking { held: PaymentMethod } }
    pub struct EscrowReceipt { pub held: Money<Gbp> }

    #[derive(Debug, PartialEq, Eq)]
    pub enum ParseError { UnknownContract, NegativeMoney, PercentOutOfRange, MissingPaymentMethod, IllegalStage }
}

// ======================= src/money.rs ========================
// (see §4 for the full module — Currency, Money<C>, Percent — reproduced there verbatim)

// ======================= src/stage.rs ========================
pub mod stage {
    pub trait Stage {}
    pub enum None {}      impl Stage for None {}
    pub enum Applied {}   impl Stage for Applied {}
    pub enum Verified {}  impl Stage for Verified {}
    pub enum Accepted {}  impl Stage for Accepted {}
    pub enum Settled {}   impl Stage for Settled {}
    pub enum Finished {}  impl Stage for Finished {}
}

// =================== src/capabilities.rs =====================
pub mod capabilities {
    use crate::domain::{AcceptOutcome, Deposit, EscrowReceipt, PaymentMethod, Settlement};
    use crate::money::{Gbp, Money};
    use crate::stage;

    // A: ONE Accept head — `In` is the arity, `From` is the per-variant pre-accept stage.
    pub trait Accept { type In; type From: stage::Stage; fn accept(&self, input: Self::In) -> AcceptOutcome; }

    // Shared accept STEPS, each written ONCE as a default method on a refinement supertrait.
    pub trait SimpleAccept: Accept<In = ()> {
        fn simple_step(&self) -> AcceptOutcome { AcceptOutcome::StandardBooking }
    }
    pub trait PaidAccept: Accept<In = PaymentMethod> {
        fn paid_step(&self, held: PaymentMethod) -> AcceptOutcome { AcceptOutcome::DeferredBooking { held } }
    }
    // (iii) third arity under the SAME head — additive, existing arities untouched.
    pub trait DepositAccept: Accept<In = (Deposit, PaymentMethod)> {
        fn deposit_step(&self, d: Deposit, held: PaymentMethod) -> AcceptOutcome { let _ = d; AcceptOutcome::DeferredBooking { held } }
    }

    pub trait Verify { fn verify(&self) -> bool; }
    pub trait Settle { fn settle(&self, gross: Money<Gbp>) -> Settlement; }
    // Settle is OPTIONAL: a contract with nothing to settle simply omits `impl Settle`
    // (no `NoOpSettleStep` stub) and skips the Settled stage entirely.
    // Finish carries its own `From` stage, so a contract finishes from wherever its path ends:
    // Accepted if it doesn't settle, Settled if it does. Omitting `impl Finish` => terminal at Settled.
    pub trait Finish { type From: stage::Stage; }

    // (ii) new shared capability on a subset.
    pub trait Escrow { fn hold(&self) -> Money<Gbp>; fn escrow(&self) -> EscrowReceipt { EscrowReceipt { held: self.hold() } } }
    // (v) capability unique to one contract.
    pub trait IssueInvoice { fn invoice(&self) -> Money<Gbp>; }
    // (iv) the one new capability for the new contract.
    pub trait Renew { fn renew(&self) -> Self where Self: Sized; }
}

// ===================== src/lifecycle.rs ======================
pub mod lifecycle {
    use std::marker::PhantomData;
    use crate::capabilities::{Accept, Escrow, Finish, Settle, Verify};
    use crate::domain::{EscrowReceipt, Settlement};
    use crate::money::{Gbp, Money};
    use crate::stage::{self, Stage};

    // Affine handle: every transition takes `self` by value ⇒ stale/double-use is a compile error.
    pub struct Concert<C, S: Stage> { contract: C, gross: Money<Gbp>, _s: PhantomData<S> }

    pub fn seed<C>(contract: C, gross: Money<Gbp>) -> Concert<C, stage::None> {
        Concert { contract, gross, _s: PhantomData }
    }
    impl<C, S: Stage> Concert<C, S> {
        fn advance<S2: Stage>(self) -> Concert<C, S2> { Concert { contract: self.contract, gross: self.gross, _s: PhantomData } }
    }
    impl<C> Concert<C, stage::None>    { pub fn apply(self)  -> Concert<C, stage::Applied>  { self.advance() } }
    impl<C, S> Concert<C, S> where C: Finish<From = S>, S: Stage { pub fn finish(self) -> Concert<C, stage::Finished> { self.advance() } } // finish from a PER-CONTRACT stage (Accepted if no Settle, Settled if it settles); omit `impl Finish` to terminate at Settled

    // The single, non-overlapping accept transition: contract-agnostic; arity = C::In; start stage = C::From.
    impl<C, S> Concert<C, S> where C: Accept<From = S>, S: Stage {
        pub fn accept(self, input: C::In) -> Concert<C, stage::Accepted> { let _ = self.contract.accept(input); self.advance() }
    }
    impl<C: Verify> Concert<C, stage::Applied> {
        pub fn verify(self) -> Concert<C, stage::Verified> { assert!(self.contract.verify()); self.advance() }
    }
    impl<C: Settle> Concert<C, stage::Accepted> {
        pub fn settle(self) -> (Concert<C, stage::Settled>, Settlement) { let s = self.contract.settle(self.gross); (self.advance(), s) }
    }
    // (ii) Escrow-gated query (additive).
    impl<C: Escrow> Concert<C, stage::Accepted> { pub fn hold_in_escrow(&self) -> EscrowReceipt { self.contract.escrow() } }

    // Each contract states its own path Applied -> its accept point. Shared by shape; one line each.
    pub trait Lifecycle: Accept + Sized { fn reach_accept(c: Concert<Self, stage::Applied>) -> Concert<Self, Self::From>; } // note: NOT `+ Settle` — settling is optional and per-contract

    // ONE generic, contract-agnostic driver (full-lifecycle form; used by tests and any in-process caller).
    pub fn drive<C: Lifecycle + Settle + Finish<From = stage::Settled>>(c: Concert<C, stage::None>, accept_input: C::In) -> Settlement { // full settle->finish path (DoorSplit/Versus). Contracts that skip Settle or finish-from-Accepted are driven per-operation (§7.3), not by this helper
        let (settled, settlement) = C::reach_accept(c.apply()).accept(accept_input).settle();
        let _ = settled.finish();
        settlement
    }
}

// ===================== src/contracts.rs ======================
pub mod contracts {
    use crate::capabilities::{Accept, Finish, PaidAccept, Settle, SimpleAccept, Verify};
    use crate::domain::{AcceptOutcome, Party, PaymentMethod, Settlement};
    use crate::lifecycle::{Concert, Lifecycle};
    use crate::money::{Gbp, Money, Percent};
    use crate::stage;

    pub struct FlatFee { pub fee: Money<Gbp> }
    impl Accept       for FlatFee { type In = (); type From = stage::Applied; fn accept(&self, _: ()) -> AcceptOutcome { self.simple_step() } }
    impl SimpleAccept for FlatFee {}
    // FlatFee does NOT settle: its fee is captured into escrow at accept and released at finish (effects, §7.8) — no `impl Settle`, no NoOp.
    impl Lifecycle    for FlatFee { fn reach_accept(c: Concert<Self, stage::Applied>) -> Concert<Self, stage::Applied> { c } }

    pub struct VenueHire { pub hire_fee: Money<Gbp> }   // reverse flow: Artist pays Venue
    impl Accept       for VenueHire { type In = (); type From = stage::Applied; fn accept(&self, _: ()) -> AcceptOutcome { self.simple_step() } }
    impl SimpleAccept for VenueHire {}
    // VenueHire does NOT settle: hire_fee is captured (artist->venue) at accept and released at finish (effects, §7.8) — no `impl Settle`, no NoOp.
    impl Lifecycle    for VenueHire { fn reach_accept(c: Concert<Self, stage::Applied>) -> Concert<Self, stage::Applied> { c } }

    pub struct DoorSplit { pub artist_pct: Percent }
    impl Accept     for DoorSplit { type In = PaymentMethod; type From = stage::Verified; fn accept(&self, pm: PaymentMethod) -> AcceptOutcome { self.paid_step(pm) } }
    impl PaidAccept for DoorSplit {}
    impl Verify     for DoorSplit { fn verify(&self) -> bool { true } }
    impl Settle     for DoorSplit { fn settle(&self, gross: Money<Gbp>) -> Settlement { Settlement { from: Party::Venue, to: Party::Artist, amount: gross.pct(self.artist_pct) } } }
    impl Lifecycle  for DoorSplit { fn reach_accept(c: Concert<Self, stage::Applied>) -> Concert<Self, stage::Verified> { c.verify() } }

    pub struct Versus { pub guarantee: Money<Gbp>, pub artist_pct: Percent }
    impl Accept     for Versus { type In = PaymentMethod; type From = stage::Verified; fn accept(&self, pm: PaymentMethod) -> AcceptOutcome { self.paid_step(pm) } }
    impl PaidAccept for Versus {}
    impl Verify     for Versus { fn verify(&self) -> bool { true } }
    impl Settle     for Versus { fn settle(&self, gross: Money<Gbp>) -> Settlement { Settlement { from: Party::Venue, to: Party::Artist, amount: gross.pct(self.artist_pct).max(self.guarantee) } } }
    impl Lifecycle  for Versus { fn reach_accept(c: Concert<Self, stage::Applied>) -> Concert<Self, stage::Verified> { c.verify() } }

    // Per-contract finish stage. Escrow contracts finish from Accepted (they never visit Settled); deferred contracts finish from Settled.
    impl Finish for FlatFee   { type From = stage::Accepted; }
    impl Finish for VenueHire { type From = stage::Accepted; }
    impl Finish for DoorSplit { type From = stage::Settled; }
    impl Finish for Versus    { type From = stage::Settled; }
}

// ====================== src/growth.rs ========================
// Living proof of both-axes additivity. Nothing in the modules above changes to add any of these.
pub mod growth {
    use rust_decimal::Decimal;
    use rust_decimal_macros::dec;
    use crate::capabilities::{Accept, Escrow, IssueInvoice, PaidAccept, Renew, Settle, Verify};
    use crate::contracts::{FlatFee, Versus, VenueHire};
    use crate::domain::{AcceptOutcome, Party, PaymentMethod, Settlement};
    use crate::lifecycle::{Concert, Lifecycle};
    use crate::money::{Gbp, Money};
    use crate::stage;

    // (i) fourth contract with NO Accept — calling accept on it must not compile (see trybuild).
    pub struct CompSlot;
    impl Settle for CompSlot { fn settle(&self, _g: Money<Gbp>) -> Settlement { Settlement { from: Party::Venue, to: Party::Artist, amount: Money::parse(dec!(0)).unwrap() } } }

    // (ii) Escrow shared on a subset (the trait/transition live in capabilities/lifecycle; here are the impls).
    impl Escrow for VenueHire { fn hold(&self) -> Money<Gbp> { self.hire_fee } }
    impl Escrow for Versus    { fn hold(&self) -> Money<Gbp> { self.guarantee } }

    // (iv) new contract reusing the shared paid-accept step + one new capability (Renew).
    pub struct Residency { pub weekly_fee: Money<Gbp>, pub weeks: Decimal }
    impl Accept     for Residency { type In = PaymentMethod; type From = stage::Verified; fn accept(&self, pm: PaymentMethod) -> AcceptOutcome { self.paid_step(pm) } }
    impl PaidAccept for Residency {}
    impl Verify     for Residency { fn verify(&self) -> bool { true } }
    impl Settle     for Residency { fn settle(&self, _g: Money<Gbp>) -> Settlement { Settlement { from: Party::Venue, to: Party::Artist, amount: Money::parse(self.weekly_fee.amount() * self.weeks).unwrap() } } }
    impl Lifecycle  for Residency { fn reach_accept(c: Concert<Self, stage::Applied>) -> Concert<Self, stage::Verified> { c.verify() } }
    impl Renew      for Residency { fn renew(&self) -> Self { Residency { weekly_fee: self.weekly_fee, weeks: self.weeks } } }

    // (v) capability unique to FlatFee.
    impl IssueInvoice for FlatFee { fn invoice(&self) -> Money<Gbp> { self.fee } }
    pub fn issue_invoice<C: IssueInvoice>(c: &C) -> Money<Gbp> { c.invoice() }
    // issue_invoice(&DoorSplit { .. })  // ❌ does not compile — DoorSplit: IssueInvoice not satisfied

    // (vi) brand-new contract reusing paid-accept + verify; wired into the lifecycle.
    // The ONLY existing-code edit when this ships is one new arm at the Phase-2 parse.
    pub struct SponsorDeal { pub sponsorship: Money<Gbp> }
    impl Accept     for SponsorDeal { type In = PaymentMethod; type From = stage::Verified; fn accept(&self, pm: PaymentMethod) -> AcceptOutcome { self.paid_step(pm) } }
    impl PaidAccept for SponsorDeal {}
    impl Verify     for SponsorDeal { fn verify(&self) -> bool { true } }
    impl Settle     for SponsorDeal { fn settle(&self, _g: Money<Gbp>) -> Settlement { Settlement { from: Party::Venue, to: Party::Artist, amount: self.sponsorship } } }
    impl Lifecycle  for SponsorDeal { fn reach_accept(c: Concert<Self, stage::Applied>) -> Concert<Self, stage::Verified> { c.verify() } }
}

// ======================== src/lib.rs =========================
// pub mod domain; pub mod money; pub mod stage; pub mod capabilities; pub mod lifecycle; pub mod contracts; pub mod growth;
```

Compile-error gallery (each, uncommented, must fail — these become the `trybuild` cases in §11):

```text
A  seed(flatfee, g).apply().accept(PaymentMethod::Cash)     // expected (), found PaymentMethod
A  seed(versus, g).apply().verify().accept(())              // expected PaymentMethod, found ()
B  seed(flatfee, g).apply().verify()                        // FlatFee: Verify not satisfied
C  seed(doorsplit, g).apply().accept(pm)                    // DoorSplit: Accept<From = Applied> not satisfied
C  seed(flatfee, g).apply().settle()                        // no method `settle` on Concert<_, Applied>
C  let (h,_) = c.settle(); c.settle();                      // use of moved value `c`
C  let (h,_) = c.settle(); h.settle();                      // no method `settle` on Concert<_, Settled>
D  seed(CompSlot, g).apply().accept(())                     // CompSlot: Accept<…> not satisfied
E1 gbp.add(usd)                                             // expected Money<Gbp>, found Money<Usd>
E3 money::Percent(dec!(50))                                 // tuple-struct constructor is private
v  issue_invoice(&door_split)                               // DoorSplit: IssueInvoice not satisfied
```
