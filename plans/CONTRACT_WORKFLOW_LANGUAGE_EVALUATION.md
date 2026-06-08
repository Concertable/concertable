# Language evaluation — Concertable contract workflow + settlement engine

You are evaluating which programming language can best **model and scale** the core of
this system's contract workflow + settlement engine. Reason from first principles. You are
running inside the repo: **READ THE REFERENCED FILES before answering — they are the ground
truth, not this summary.** Every `@`-path below is a real file in this repo; open all of them.

---

## 0. How to run this evaluation (read first — avoid the known failure mode)

This question has been muddied before by conflating two different axes. Do **not** repeat that.
Evaluate on **two clearly separated axes**, and never let one leak into the other:

- **Axis 1 — Raw expressive power + scalability.** How well the language's *type system* can
  model this architecture, and how gracefully it absorbs growth (the Expression Problem below).
  Ignore ecosystem, libraries, interop, ORMs, hiring, and learning curve **entirely** on this axis.
- **Axis 2 — Real-world fit.** Recognition, ecosystem maturity, operational simplicity
  (deployment, runtime, memory behaviour), and suitability for a **startup B2B SaaS + portfolio
  piece** where this engine is one sub-part of a larger system.

Give a **ranked verdict on each axis separately**, then **one** final recommendation that states
which axis you weighted and why. Be decisive. If two are close, name the tiebreaker. Do not
oscillate: when a sub-point is a tie, say "tie" and move on — do not let a marginal detail flip
the overall call.

---

## 1. The system

The core of a B2B live-music booking SaaS: a **contract workflow + settlement engine**. A contract
is exactly one of four kinds today, each settling gross door revenue differently:

- **FlatFee** — fixed fee regardless of revenue.
- **DoorSplit** — a percentage of gross.
- **VenueHire** — the artist *pays* the venue a fixed hire fee (money flows the other way).
- **Versus** — the *greater* of a guaranteed minimum or a percentage of gross.

These variants ARE the product. **More contract types will be added over time**, and new
capabilities will be added, sometimes to only one contract type. The model must SCALE on both
axes (see §5).

Canonical contract enum: `@api/Concertable.B2B/Modules/Contract/Concertable.B2B.Contract.Contracts/ContractType.cs`

---

## 2. How it's modelled in C# today — read the whole workflow folder

The workflow is a lifecycle state machine of **stages**. Each contract type has a per-type
workflow **composed of capabilities**; each capability exposes a **step**; an **executor**
dispatches a step; a **builder** wires which step fills each slot per contract type via DI.

**Domain (stages + lifecycle entity):**
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Domain/Enums/ConcertStage.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Domain/ILifecycleEntity.cs`

**Capabilities** (shared markers + variant refinements — note the empty marker + paid/simple split):
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Capabilities/IAccepts.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Capabilities/IAcceptsPaid.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Capabilities/IAcceptsSimple.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Capabilities/IAcceptsCheckout.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Capabilities/IApplies.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Capabilities/IAppliesPaid.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Capabilities/IAppliesSimple.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Capabilities/IAppliesCheckout.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Capabilities/IVerifies.cs`

**Steps** (note the differing `ExecuteAsync` arities — paid carries a `paymentMethodId`, simple does not):
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Steps/IConcertStep.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Steps/IPaidAcceptStep.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Steps/ISimpleAcceptStep.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Steps/IPaidApplyStep.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Steps/ISimpleApplyStep.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Steps/IApplyCheckoutStep.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Steps/IAcceptCheckoutStep.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Steps/IVerifyStep.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Steps/ISettleStep.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Steps/IFinishStep.cs`

**Executors** (the dispatch layer — note `IAcceptExecutor` takes a nullable `paymentMethodId`):
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Executors/IApplyExecutor.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Executors/IAcceptExecutor.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Executors/IVerifyExecutor.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Executors/ISettleExecutor.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/Executors/IFinishExecutor.cs`

**Orchestration, state machine, and the RUNTIME fallbacks:**
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/IConcertWorkflow.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/IConcertWorkflowFactory.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/IWorkflowStateMachine.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/IConcertTransitionValidator.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/IConcertTransitionValidatorFactory.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/IConcertWorkflowCapabilityRegistry.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/ILifecycleRepository.cs`

**The builder + a concrete step implementation (the composition/DI approach — central to this evaluation):**
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Services/Workflow/ConcertWorkflowBuilder.cs`
- `@api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Services/Workflow/Steps/PaidAcceptStep.cs`

**Service architecture context (so you understand the surrounding system):**
- `@api/Concertable.B2B/ARCHITECTURE.md`

Two things are pushed to RUNTIME because C#'s type system can't express them — these are exactly
what a more expressive language should move into the compiler:
- `IConcertTransitionValidator.CanTransitionTo(from, to)` — stage-transition legality.
- `IConcertWorkflowCapabilityRegistry.Has<TCapability>(contractType)` — capability presence
  (e.g. "does FlatFee have `IVerifies`?"), because the type system can't say a given contract
  type has a capability that other types never have.

---

## 3. The modelling problems (address each with concrete code for the top contenders)

**A. ONE capability, variant-dependent ARITY.** "Accept" is common, but paid contracts need a
payment method and simple ones don't — see `IAcceptsPaid`/`IPaidAcceptStep` vs
`IAcceptsSimple`/`ISimpleAcceptStep`. C# can't unify this, so it splits into sibling interfaces
under an empty marker (`IAccepts`), and `IAcceptExecutor` collapses the difference into a
nullable `paymentMethodId`. **Supplying the wrong parameters for a variant must be a COMPILE error.**

**B. A capability present on SOME variants and NEVER on others.** See `IVerifies`/`IVerifyStep`,
which only certain contracts have. Generalise to a hypothetical `IFooStep` used only by one
`FooContract`. **Invoking it on a variant that lacks it must be a COMPILE error, not a runtime
registry miss** (`Has<TCapability>`).

**C. The lifecycle as TYPESTATE.** Stages (read `ConcertStage.cs` — note the integer order does
NOT match the legal lifecycle order, which is itself variant-dependent). An illegal transition
(e.g. Applied → Settled) should not compile. Show how the executors and state machine look under
this encoding. The legal *path* can differ per variant (some contracts skip Verify) — encode that.

**D. The workflow as a COMPOSITION of capabilities — without nullable-slot spaghetti (READ THIS
ONE CAREFULLY; it is the crux).** The current design (`ConcertWorkflowBuilder`) composes each
contract's workflow out of **shared, reusable step implementations**, wired per-contract-type at
startup, and dispatches **contract-agnostically**: the caller asks "what fills the Accept slot?"
not "what contract is this?". Crucially, **different contracts share the same step** (e.g. FlatFee
and VenueHire can share a single Finish step, since both are already paid by then — write the
behaviour once, attach it to both). A naive translation to a struct of slots produces:

```rust
struct Workflow {
    accept: Arc<dyn AcceptStep>,
    verify: Option<Arc<dyn VerifyStep>>,   // not every contract verifies
    settle: Arc<dyn SettleStep>,
    finish: Arc<dyn FinishStep>,
}
```

As capabilities grow, this rots into a struct full of `Option<…>` nullable slots and runtime
`None` checks — spaghetti. **The requirement:** a workflow value whose **type reflects exactly
which capabilities it has** — so `verify` is only callable on workflows that actually have it,
with **no `Option`, no runtime nil-check** — while STILL being: (a) contract-agnostic at the call
site, (b) composed from shared/reused behaviours, and (c) open to new capabilities without
editing existing contracts. For each candidate, show whether this is expressible (consider:
row polymorphism / extensible records, type-level capability sets, heterogeneous lists, open
unions, tagless-final algebras, dependent records) and how elegantly the `ConcertWorkflowBuilder`
composition pattern translates.

**Also: illegal data unrepresentable** — no negative fees, percentages outside 0–100, mixed
currencies, or settling a contract that isn't yet confirmed.

---

## 4. The two RUNTIME fallbacks to eliminate

State explicitly, per candidate, what happens to each:
- `CanTransitionTo(from, to)` → should become unconstructable illegal transitions (Problem C).
- `Has<TCapability>(contractType)` → should become a compile-time absence (Problem B / D).

And name the one place — if any — where even the winning language still falls back to a runtime
check (hint: the deserialization / DB boundary, where a contract type arrives as untyped data).

---

## 5. The Expression Problem (the scalability test — address explicitly)

The system grows on two axes: (a) new contract VARIANTS, and (b) new CAPABILITIES, sometimes on
only one variant. Sum types make adding operations easy but adding variants painful; OOP/trait
dispatch makes adding variants easy but adding operations painful. For EACH candidate: how
gracefully does it absorb **a new contract type** AND **a new single-variant capability**, and
how does adding a capability interact with the composition/builder model in Problem D (does the
nullable-slot spaghetti return)? Which way does the language bias, and is there an encoding
(typeclasses/traits, polymorphic variants, GADTs, tagless-final, open unions, row types,
extensible records, dependent records) that mitigates the trade-off here?

---

## 6. Candidates

Evaluate at least **Rust, Haskell, OCaml, F#, and PureScript** (PureScript included specifically
for its first-class row polymorphism, relevant to Problem D). If a language outside that set is
genuinely more expressive/scalable for this — including dependently-typed languages (**Idris 2,
Lean 4, Agda**) — include it and say why.

---

## 7. What I want back

1. **A ranked verdict on Axis 1** (raw expressive power + scalability), least to most, with reasoning.
2. **A ranked verdict on Axis 2** (real-world fit for a startup SaaS + portfolio), with reasoning.
3. **For the top 2–3 on Axis 1, concrete code** showing: Problem A (variant-dependent arity),
   Problem B (single-variant capability; wrong-variant call must not compile), Problem C
   (typestate state machine incl. what the executors look like), **Problem D (composition without
   nullable-slot spaghetti, incl. how the `ConcertWorkflowBuilder` pattern translates and how
   shared steps are reused)**, adding a 5th contract type, and adding a new single-variant capability.
4. **One definitive recommendation**: the single best language to express AND scale this domain
   for a startup B2B SaaS + portfolio piece — which axis you weighted, why it beats the runner-up,
   the tiebreaker if close, and where (if anywhere) even the winner still falls back to a runtime check.

Be decisive and concrete. Prefer real code over prose. Do not transliterate the C# design — each
language may solve the same problem in a COMPLETELY DIFFERENT way that fits the language, as long
as the result is the same. The goals are: **expressive, dynamic, contract-agnostic composition
(builder-like), with no nullable-slot spaghetti as capabilities grow.**
