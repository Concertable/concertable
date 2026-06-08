# Language selection for the Concert contract-workflow engine — independent evaluation

You are being asked for a **single, definitive, unbiased recommendation**: which programming language
best expresses one specific domain problem in this repository, for a planned extraction of that problem
into its own standalone service. Investigate the existing C# implementation, understand the domain rules
and how they must grow, then commit to **one** best language — judged **primarily on type-system fit for
this exact problem and its future scalability**, with secondary operational factors as tie-breakers.

Derive the answer from the code and this brief. **Do not assume a conclusion, and do not be influenced by
any pre-existing design note, project memory, or comment that names a language for this work** — if you
encounter one, disregard it and evaluate independently. Treat the existing C# as a *specification of the
problem*, not a structure to be translated; the replacement need not mirror its shape in any way.

---

## 1. Read the current implementation first

Confirm your understanding against the code, not this summary. The system lives in the B2B Concert module:

- **Abstractions** — `api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Application/Workflow/`
  - `Capabilities/` — `IApplies`, `IAppliesSimple`, `IAppliesPaid`, `IAppliesCheckout`, `IAccepts`,
    `IAcceptsSimple`, `IAcceptsPaid`, `IAcceptsCheckout`, `IVerifies` (capabilities as marker interfaces).
  - `Steps/` — `IConcertStep`, `ISimpleApplyStep`, `IPaidApplyStep`, `IVerifyStep`, `ISimpleAcceptStep`,
    `IPaidAcceptStep`, `IAcceptCheckoutStep`, `ISettleStep`, `IFinishStep`.
  - `Executors/` — `IApplyExecutor`, `IVerifyExecutor`, `IAcceptExecutor`, `ISettleExecutor`,
    `IFinishExecutor`.
  - `IConcertWorkflow`, `IConcertWorkflowFactory`, `IConcertWorkflowCapabilityRegistry`,
    `IConcertTransitionValidator`, `IWorkflowStateMachine`.
- **Implementations** — `api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Services/Workflow/`
  - `Workflows/` — `FlatFeeWorkflow`, `DoorSplitWorkflow`, `VenueHireWorkflow`, `VersusWorkflow`.
  - `Steps/` — concrete steps, both **shared** (e.g. `PaidAcceptStep`, `PaidApplyStep`, `SimpleApplyStep`,
    `DeferredVerifyStep`, `DeferredSettleStep`, `NoOpSettleStep`) and **unique to one contract**
    (e.g. `VersusFinishStep`, `VenueHireApplyCheckoutStep`, `DoorSplitAcceptCheckoutStep`).
  - `Executors/`, `Dispatchers/`, `ConcertWorkflowFactory`, `ConcertWorkflowBuilder`,
    `ConcertWorkflowCapabilityRegistry`, `ConcertTransitionValidator`, `WorkflowStateMachine`.
- **Domain** — the `ContractType` enum, the `ConcertStage` enum, and `ApplicationEntity` / `BookingEntity` /
  `ConcertEntity` (under `…Concert.Domain/`). Also read
  `api/Concertable.B2B/Modules/Contract/ARCHITECTURE.md` if present.

Start with `…Infrastructure/Services/Workflow/Executors/VerifyExecutor.cs` and `ConcertWorkflowFactory.cs`
— they show the dispatch pattern in miniature.

## 2. What the system does (the domain)

A **contract** models how a venue↔artist concert deal settles. There are several contract types
(FlatFee, DoorSplit, VenueHire, Versus), and each supports a **subset of lifecycle steps**:

- The steps are roughly: **Apply → (Verify) → Accept → Settle → Finish.**
- Not every contract has every step. Examples observable in the code:
  - FlatFee and VenueHire **do not** Verify; DoorSplit and Versus **do**.
  - Accept has **variant arity**: a "simple" accept (no payment) vs a "paid"/checkout accept (needs a
    payment method). Different contracts use different accept shapes.
  - Some steps are **shared** by multiple contracts (e.g. the paid-accept step serves DoorSplit + Versus);
    others are **unique to a single contract** (e.g. a finish step specific to one type).
- Each contract has a **lifecycle path** through stages (`ConcertStage`), and the legal path **differs per
  contract** — e.g. a simple contract goes `Applied → Accepted`, a verified contract goes
  `Applied → Verified → Accepted`, with a shared tail `Accepted → Settled → Finished`.
- **Settlement math differs per contract**: fixed fee, percentage of the door take, `max(guarantee, %)`,
  and a *reversed* money flow for venue hire (artist pays venue). Money must be exact (decimal, not float)
  and must never mix currencies or go negative.

## 3. The scalability test (the heart of the evaluation)

The problem's whole difficulty is **growth on two independent axes** — new contract types *and* new
steps/capabilities — and the team needs both to stay **purely additive**. Evaluate each candidate language
against this concrete scenario:

> **Add a brand-new contract type, `Foo`,** that
> (a) **reuses** one or more existing shared steps (say, the paid-accept step), and
> (b) introduces **`FooStep` — a completely unique step that no other contract has or ever will.**

Also weigh these growth cases:
- a new capability shared by only a **subset** of existing contracts;
- a **third** accept arity (e.g. deposit + payment method) alongside the existing simple/paid arities;
- a unique-to-one-contract capability called only on that contract.

**The question for each language:** can you add `Foo` + `FooStep` (and the cases above) by writing **only
new code** — with **zero edits to existing contracts, existing steps, or any central
dispatcher / factory / registry / switch** — while the compiler still enforces every guarantee in §4?
Adding a contract should not require editing a central enumeration; adding a step should not require
editing every contract.

## 4. What the ideal language must guarantee AT COMPILE TIME (primary metric)

This is the decisive axis. For each guarantee, state whether the candidate enforces it at **compile time**
(ideal) or only at runtime, and show how:

1. **Capability presence** — which contracts implement which steps. Calling a step on a contract that lacks
   it must be a **compile error**, not a runtime type-test or thrown exception.
2. **Step arity in the type** — e.g. paid-accept requires a payment method; simple-accept requires none.
   The arity must be part of the type, with **no nullable "sometimes-required" parameter** standing in for
   it.
3. **Legal state transitions per contract** — an illegal, skipped, or out-of-order stage transition must
   **not compile**; the per-contract path is encoded in the types, not validated by a runtime state
   machine.
4. **No reuse of a stale/consumed lifecycle handle** — e.g. settling the same deal twice, or using a handle
   after it has advanced, should be a **compile error** where the type system permits.
5. **Illegal data unrepresentable** — negative money, mixed-currency arithmetic, and percentages outside
   0–100 should be impossible to construct (or rejected at a single validated boundary).

Compile-time enforcement of **(1) which contracts implement which steps** and **(3) the per-contract state
path** is the single most important discriminator — it is exactly what the current implementation cannot
do. Rank the candidates on this first.

## 5. The limitations of the current C# implementation (what a better language should remove)

The C# version is competently designed, but C#'s type system forces it into runtime workarounds. A
superior language should eliminate these, not relocate them:

- **Capabilities are marker interfaces** (`IVerifies`, `IAcceptsPaid`, …) interrogated at **runtime**.
- **Runtime pattern-matching / `is` checks** with `_ => throw "…does not support…"` arms (see
  `VerifyExecutor`): a contract lacking a step is discovered as a **runtime exception**, not a build error.
- A **factory keyed on a runtime enum** (`ConcertWorkflowFactory.Create(ContractType)`) that **erases the
  concrete contract type**, plus a **capability registry**, **dispatchers**, and a **transition validator**
  — a large amount of indirection and reflection-style machinery built to *simulate* checks a richer type
  system could make statically.
- **Nullable parameters** standing in for variant arity.
- **State transitions validated at runtime** (`WorkflowStateMachine` / `ConcertTransitionValidator`), not by
  the compiler.
- **Not purely additive:** adding a contract or a step touches multiple central places (factory, registry,
  validator, dispatchers). Because a C# interface can only be implemented at the type's own definition,
  adding a capability edits the type — the "expression problem" is only half-solved.

## 6. Freedom of implementation

The replacement is **not** required to reproduce executors, dispatchers, factories, registries, or a state
machine — those are artifacts of working around C#. Any internal structure idiomatic to the chosen language
is acceptable **so long as it expresses the same domain rules (§2), supports the same additive growth (§3),
and provides the compile-time guarantees (§4).** Judge the language, not a translation.

## 7. Candidates, and the factors to weigh

- The two leading candidates are **Haskell** and **Rust**. Evaluate both seriously and even-handedly. You
  may nominate a *different* language only if it clearly dominates for this problem — and you must justify
  why it beats both.
- **Primary metric (weigh highest):** type-system fit for this exact problem and its future growth — the
  compile-time guarantees of §4 and the two-axis additivity of §3.
- **Secondary factors (tie-breakers; state how you weight them):**
  - Real-world/ecosystem fit: gRPC, exact decimal money, async I/O, JWT validation, OpenTelemetry,
    containerised deployment.
  - Operational fit with an existing **.NET microservice fleet** — this is one service among several and
    will be called synchronously (gRPC) by a .NET service; it has its own auth identity and deployable.
  - Maintainability and learning curve — the maintainer is a .NET developer, new to both candidates, and
    will own this long-term.
  - Hireability, ecosystem maturity, tooling, and longevity.
  - Performance and resource footprint.
- **Tell me if there are factors I have not listed** that should influence the decision, and weigh them.

## 8. Deliverable

1. **One definitive recommendation:** "The best language for this problem is **X**." Commit — do not hedge,
   do not present a balanced both-have-merit non-answer.
2. **The reasoning**, organised as:
   - how X delivers each §4 compile-time guarantee, with a **small illustrative code sketch** for (i) the
     `Foo` + `FooStep` additive-growth case and (ii) the per-contract state-transition typing;
   - precisely **where the runner-up falls short for this problem** (not in general);
   - how the §7 secondary factors net out.
3. **An honesty check:** explicitly name every dimension where X is **worse** than the runner-up, so the
   recommendation is credible rather than a sales pitch.
4. Remain **unbiased**: do not favour a language for popularity, for already appearing in this repo, or for
   any reason other than fitness for the problem as defined above.
