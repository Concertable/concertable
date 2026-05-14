# Concert Workflow → Rust: Analysis Reference

Why Rust is the best language for the Concert workflow dispatch problem, and why C# can't finish the sentence the code is already trying to say.

---

## The problem in one sentence

The Concert workflow is a sparse matrix of (contract type × operation), where both axes can grow independently, and cells in the same column have different input shapes.

---

## The shape of the problem

| Axis           | Today                                                              | Open? |
|----------------|--------------------------------------------------------------------|-------|
| Operations     | Apply, Accept, Verify, Settle, Finish, ApplyCheckout, AcceptCheckout | Yes — `IVerifies` only exists because two contracts needed it; a future contract could bring a new stage |
| Contract types | FlatFee, DoorSplit, Versus, VenueHire                             | Yes — new contracts are explicitly a design goal |
| Cells          | (op × contract) — some absent, some present with different signatures | Sparse + heterogeneous |

Three concrete facts from the code:

1. **Some cells don't exist.** Only `VersusWorkflow` and `DoorSplitWorkflow` implement `IVerifies`. Only `VenueHireWorkflow` implements `IHasApplyCheckout`. Missing cells today are a `BadRequestException` thrown from the fallthrough arm of an `is`/`switch` in each executor.

2. **Same-name operations have different signatures across contracts.** `IAcceptsSimple` takes `(int applicationId)`. `IAcceptsPaid` takes `(int applicationId, string paymentMethodId)`. The current workaround: two parallel capability interfaces per stage, then re-discriminate at the executor with a `when pmId is { } p` guard. The discriminator lives in three places at once: controller routing, dispatcher overloads, and executor switch arms.

3. **"Missing payment method" and "contract doesn't support this stage" are the same failure.** Both are things the type system should rule out. Both currently land as the same runtime `BadRequestException`.

Two corollaries:

- `IConcertWorkflowCapabilityRegistry.Has<TCapability>()` answers "does this workflow support this operation?" — but it's only consulted by the API response mappers for HATEOAS links. The executors duplicate the same check with `is`/`switch`. The same question has two different runtime implementations.
- `Settle` and `Finish` dispatch via virtual call (they're on `IConcertWorkflow` directly). Apply / Accept / Verify / Checkouts dispatch via capability `is`-check. Three different dispatch styles for what is conceptually the same question.

---

## Why OOP falls short — paradigm level

OOP has one fundamental dispatch model: **the receiver determines the behavior**. `shape.area()` dispatches on `shape`'s type. That's the whole mechanism.

This works when:
- The type of *one* thing is the primary axis of variation
- The operation set is stable
- Behavior naturally belongs to the object

It breaks in four situations — and this problem hits all four:

### 1. When two things together determine behavior (multi-dispatch)

OOP dispatch is single: it dispatches on one receiver. When behavior depends on two types simultaneously there's no native answer. You get Visitor pattern (faking double dispatch by hand) or nested `is`/`switch` chains.

Your case: behavior is determined by the *operation* (Accept, Apply, Verify) AND the *contract type* (FlatFee, VenueHire). You picked the contract as the receiver — so the operation became a runtime `is` check. Pick the other receiver and you'd have the same problem in reverse.

### 2. When both the types AND the operations grow (the Expression Problem)

OOP's subtype polymorphism is the *asymmetric* answer:
- **Open**: add new types (subclasses) — just implement the existing interface
- **Closed**: add new operations — touch every existing class

Functional + ADT languages invert it:
- **Open**: add new operations — new function, no existing code touched
- **Closed**: add new types — update every match

When **both** axes grow, OOP has no answer. `IVerifies` exists because an operation appeared that only some contracts support. Both axes were moving. The language steered you toward runtime checks because that's the only tool available when the type system can't express what you mean.

### 3. When behavior doesn't belong to any one type

OOP asks "whose method is this?" For cross-cutting operations (`serialize`, `validate`, `prettyPrint`) the answer is awkward. OOP solutions: stuff it on the class (bloats it), static utility helpers (functional programming in disguise), or Visitor again.

Traits: `impl Serialize for FlatFee` lives separately from `FlatFee`. The struct doesn't know about serialization. The dispatch still works at compile time.

### 4. When input/output shape varies per type for the same operation

OOP interfaces have fixed signatures. `IAccept.Execute(int appId, string? pmId)` — same for every implementer. The type system can't encode "FlatFee doesn't need pmId but VenueHire does." Variance is pushed to runtime: nullable fields, ignored parameters, runtime casts. That's the fundamental source of the noise in the current code.

---

## Family choice: trait/typeclass dispatch

Two families for the expression problem:

- **ADT + exhaustive match** — sum type over variants, pattern-match per operation. Compiler enforces totality. Adding an operation is trivial. **Adding a variant forces an edit to every match site.** Asymmetric — variants closed, operations open.
- **Trait/typeclass dispatch** — a trait per capability, each variant implements the subset it supports. **Both axes are purely additive.** New trait = new file, no edit to existing code. New impl = new file, no edit to existing code. Symmetric.

The code already tells you which family it wants. The capability interfaces (`IAppliesSimple`, `IAppliesPaid`, `IVerifies`, etc.) *are* traits — the design picked the right family. The pain is that C# can't finish the sentence: no associated types, no compile-time capability constraints on a runtime-tagged value, no exhaustive switch on type-of. So the trait shape degrades into runtime `is` checks with throws as the fallthrough.

---

## Language choice: Rust

### Capabilities map onto traits 1:1

```rust
trait Apply  { type Input; async fn apply(&self,  app_id: ApplicationId, input: Self::Input) -> Result<()>; }
trait Accept { type Input; async fn accept(&self, app_id: ApplicationId, input: Self::Input) -> Result<()>; }
trait Verify { type Input; async fn verify(&self, app_id: ApplicationId, input: Self::Input) -> Result<()>; }
```

Each contract struct implements only the traits it supports. A contract that doesn't implement `Verify` literally cannot have `verify()` called on it — it's not a runtime error, the method doesn't exist.

### The associated type is the key mechanism

The current `IAcceptsSimple` / `IAcceptsPaid` split exists because the input shape varies per contract. Associated types collapse this to one trait:

```rust
trait Accept {
    type Input;
    async fn accept(&self, app_id: ApplicationId, input: Self::Input) -> Result<()>;
}

// Each impl declares its own Input — no parallel interfaces, no re-discrimination
impl Accept for FlatFee   { type Input = ();              ... }   // no payment
impl Accept for DoorSplit { type Input = PaymentMethodId; ... }   // host pays at accept
impl Accept for Versus    { type Input = PaymentMethodId; ... }
impl Accept for VenueHire { type Input = ();              ... }   // artist paid at apply
```

The `IAcceptsSimple` / `IAcceptsPaid` interfaces, the `when pmId is { } p` guard in `AcceptExecutor`, and the parallel capability hierarchy all vanish. One trait. Variance lives in the associated type, not in extra interface levels.

### The arity of the method stays at 2 forever

Because `Self::Input` is **one type slot**, adding new per-contract variance never widens the method signature. If a new `FooContract` needs both a `PaymentMethodId` and a `RefundPolicy`, you give it a struct:

```rust
struct FooAcceptInput {
    pm: PaymentMethodId,
    refund: RefundPolicy,
}

impl Accept for FooContract {
    type Input = FooAcceptInput;
    async fn accept(&self, app_id: ApplicationId, input: FooAcceptInput) -> Result<()> {
        // use input.pm, input.refund
    }
}
```

Nothing on FlatFee / DoorSplit / Versus / VenueHire changes. The trait still says `accept(&self, app_id, Self::Input)`. Two arguments. Always two. No `flatfee.accept(1, (), (), ())` accretion.

The match dispatch grows by one arm — and the compiler forces you to add it:

```rust
match workflow {
    Workflow::FlatFee(w)     => w.accept(app_id, ()).await,
    Workflow::DoorSplit(w)   => w.accept(app_id, pm).await,
    Workflow::Versus(w)      => w.accept(app_id, pm).await,
    Workflow::VenueHire(w)   => w.accept(app_id, ()).await,
    Workflow::FooContract(w) => w.accept(app_id, FooAcceptInput { pm, refund }).await,
}
```

Named-field structs keep the call site readable no matter how many fields — `FooAcceptInput { pm, refund, deposit }` beats positional `(pm, refund, deposit)`.

### Why `()` isn't nullable noise

`()` in Rust is the **unit type** — a type with exactly one value, also written `()`. Not nullable, not optional, not null-with-a-different-name. There is no `null` in Rust.

`FlatFee::Input = ()` says: "there is no payload at this call site." The compiler then refuses any other type in that slot:

```rust
flatfee.accept(1, ()).await        // ✅ compiles
flatfee.accept(1, pm).await        // ❌ compile error — expected (), got PaymentMethodId
venuehire.accept(1, pm).await      // ✅ compiles
venuehire.accept(1, ()).await       // ❌ compile error — expected PaymentMethodId, got ()
```

All four C# equivalent calls would compile — the compiler can't help because the interface erases the per-variant type information.

### The "just use one interface with nullable params" counter-argument

The obvious C# alternative: one `IAccept` interface with `string? pmId`, each impl checks for null or ignores it.

```csharp
interface IAccept {
    Task ExecuteAsync(int appId, string? pmId);
}
class FlatFeeAccept : IAccept {
    public Task ExecuteAsync(int appId, string? pmId) { /* pmId quietly ignored */ }
}
class VenueHireAccept : IAccept {
    public Task ExecuteAsync(int appId, string? pmId) {
        if (pmId is null) throw new BadRequestException("PM required");
        ...
    }
}
```

This compiles and is smaller than the current code. Three things are still wrong:

1. **FlatFeeAccept's signature lies.** It claims to accept `(int, string?)` but ignores `string?`. Callers can't tell from the interface which impls need a PM. The capability registry can't answer "does this workflow require payment?" from the type — it needs metadata or runtime reflection.

2. **The throw didn't go away — it moved.** `VenueHireAccept` still does `if (pmId is null) throw`. The failure category is identical; the location changed. In Rust, calling `accept(app_id, ())` on `VenueHire` is a *compile error*.

3. **The HTTP boundary still lies.** The endpoint takes `string? pmId` and Swagger marks it optional, but for half the contracts it's actually required. The wire shape doesn't tell the client which case they're in.

Adding variance (e.g. `RefundPolicy`) means adding `RefundPolicy?` to the interface — every impl gets a parameter they ignore. The interface accretes forever. In Rust, the existing impls and call sites are literally untouched.

### Type erasure vs monomorphisation

This is why C# gets ugly and Rust doesn't. When C# sees `IAccept.ExecuteAsync(int, IAcceptParams?)`, all per-variant type information is erased to the interface. So inside `FlatFeeAccept` you get back `IAcceptParams?` and have to cast it back — runtime, can fail. The call site doesn't know which concrete subtype is right for which impl.

Rust monomorphises: the compiler resolves the concrete type at every call site.

```rust
Workflow::FlatFee(w) => w.accept(app_id, ()).await
```

The compiler knows `w: &FlatFee`, looks up `impl Accept for FlatFee { type Input = (); }`, and resolves the call to the concrete monomorphised version. No intermediate type, no cast, no null sentinel. The information isn't carried at runtime — it's baked in at compile time.

---

## OOP vs Rust: side-by-side code

### AcceptExecutor (C# current)

```csharp
// AcceptExecutor.cs — today
private Task Dispatch(ApplicationEntity app, string? pmId)
{
    var workflow = workflows.Create(app.ContractType);
    return workflow switch
    {
        IAcceptsPaid w when pmId is not null => w.Accept.ExecuteAsync(app.Id, pmId),
        IAcceptsPaid => throw new BadRequestException("This contract requires a payment method"),
        IAcceptsSimple w => w.Accept.ExecuteAsync(app.Id),
        _ => throw new BadRequestException($"Contract {workflow.Type} does not support Accept")
    };
}
```

Problems: `IAcceptsPaid` / `IAcceptsSimple` are marker interfaces — they carry no behaviour. The switch re-discriminates what the type hierarchy already tried to express. Both throws are failures the type system should rule out.

```rust
// Rust equivalent — the entire AcceptExecutor collapses to one match arm per workflow
async fn dispatch_accept(
    workflow: Workflow,
    app_id: ApplicationId,
    pm: Option<PaymentMethodId>,
) -> Result<(), WorkflowError> {
    match workflow {
        Workflow::FlatFee(w)   => w.accept(app_id, ()).await,
        Workflow::DoorSplit(w) => w.accept(app_id, pm.ok_or(WorkflowError::MissingPm)?).await,
        Workflow::Versus(w)    => w.accept(app_id, pm.ok_or(WorkflowError::MissingPm)?).await,
        Workflow::VenueHire(w) => w.accept(app_id, ()).await,
    }
}
```

No marker interfaces. No throw fallthrough. The match is exhaustive at compile time — add a contract variant and this file won't build until you handle it. `MissingPm` is now an explicitly typed error, not a `BadRequestException` that looks identical to "contract doesn't support Accept".

### IStepExecutor — the 6-overload problem

```csharp
// IStepExecutor.cs — today: 6 overloads because C# can't abstract over arity+return shape
internal interface IStepExecutor<TEntity> where TEntity : ILifecycleEntity
{
    Task ExecuteAsync(TEntity entity, ConcertStage targetStage);
    Task ExecuteAsync(int entityId, ConcertStage targetStage, StepDispatch<TEntity> dispatch);
    Task ExecuteAsync<TInput>(TEntity entity, ConcertStage targetStage, StepDispatch<TEntity, TInput> dispatch, TInput input);
    Task<TResult> ExecuteAsync<TInput, TResult>(TEntity entity, ConcertStage targetStage, StepDispatch<TEntity, TInput, TResult> dispatch, TInput input);
    Task ExecuteAsync<TInput>(int entityId, ConcertStage targetStage, StepDispatch<TEntity, TInput> dispatch, TInput input);
    Task<TResult> ExecuteAsync<TInput, TResult>(int entityId, ConcertStage targetStage, StepDispatch<TEntity, TInput, TResult> dispatch, TInput input);
}
```

6 overloads covering (entity vs entityId) × (no input vs TInput) × (void vs TResult). It stopped the refactor because `ISimpleAcceptStep` does entity *creation* (not mutation), which needs yet another delegate shape — a 7th overload.

```rust
// Rust — one generic function with a closure bound
// () as Input covers "no payload". The same function handles all 6 C# cases.
async fn execute<E, F, Fut, In, Out>(
    repo: &impl LifecycleRepo<Entity = E>,
    state_machine: &impl StateMachine,
    entity_id: EntityId,
    target_stage: Stage,
    dispatch: F,
    input: In,
) -> Result<Out, WorkflowError>
where
    F: FnOnce(&mut E, In) -> Fut,
    Fut: Future<Output = Result<Out, WorkflowError>>,
```

`()` as `In` handles the no-payload case — there's no void/unit split because `()` is a real type. `FnOnce` bounds the closure by its exact signature — the compiler enforces the right shape without enumerating overloads. A creation step just returns `Out = ApplicationId` rather than `Out = ()`. Same function, no new overload.

### Marker interfaces

In the current code: `IApplyable`, `IAcceptable`, `IVerifiable`, `ICheckoutable` — empty interfaces that exist purely so the dispatcher can `is`-check them at runtime. They carry no methods. They exist because C# has no other way to ask "does this value support this operation?" at compile time.

```csharp
// C# — the marker is the only way to ask the question at runtime
if (workflow is IApplyable) { ... }
```

```rust
// Rust — the trait IS the marker. If the impl exists, the operation is supported.
// No zero-method interface needed. The question is asked at compile time via the type.
if let Workflow::FlatFee(w) = workflow {
    w.apply(app_id, input).await  // compiles only because FlatFee: Apply
}
```

The `IConcertWorkflowCapabilityRegistry.Has<TCapability>()` probe becomes unnecessary because `match`-on-enum-variant already narrows to a concrete type at compile time, and the trait impl (or lack of it) is a compile-time fact.

---

## Why the refactor stalled — both attempts

### Old approach: inline-dump

Each workflow class (e.g. `FlatFeeConcertWorkflow`) injected all dependencies and implemented every stage inline:

```csharp
// Old FlatFeeConcertWorkflow — representative shape
public class FlatFeeConcertWorkflow : IHeldConcertWorkflow
{
    // 8 dependencies in constructor
    public Task ApplyAsync(int applicationId) { ... }
    public Task CheckoutAsync(int applicationId) { ... }
    public Task AcceptAsync(int applicationId) { ... }
    public Task SettleAsync(int concertId) { ... }
    public Task FinishAsync(int concertId) { ... }
}
```

Why it broke down: no state machine (no idempotency), all logic duplicated across 4 workflow classes, no shared scaffolding. The amount of code grows as (contracts × operations). The language constraint: C# can't express "this class handles a subset of operations whose signatures vary per implementer" without marker interfaces or runtime casts, so the path of least resistance was to give every workflow every method.

### New approach: StepExecutor refactor

The 4-tier dispatch (`ApplicationService → Dispatcher → Executor → Step`) with `IStepExecutor<TEntity>` as shared scaffolding. Where it broke: `ISimpleAcceptStep.ExecuteAsync(int applicationId)` does entity *creation*, not mutation. The generic scaffolding needs a delegate that produces an entity rather than mutating one — a seventh overload shape. The interface grows indefinitely.

**Root cause of both failures**: C# generics can't bound a delegate by its arity and return type without enumerating overloads, because there's no `FnOnce`-style primitive with an associated `Output` type. Closures aren't first-class in the type system the same way. Rust's `FnOnce<Args, Output>` is exactly that missing primitive — closure type, arity, and return type are all part of one bound.

---

## F# specifically

F# would be a substantially *better* answer than C# for this problem. It's worth stating clearly before explaining why Rust wins.

F# wins over C# because:
- Discriminated unions + exhaustive `match` replace the `is`/switch-with-throw cleanly
- `unit` is first-class — same as Rust's `()`
- `Result<'T, 'E>` / `Option<'T>` replace exception fallthroughs
- Records and immutability-by-default reduce ceremony

The F# idiomatic version:

```fsharp
type Workflow =
    | FlatFee   of FlatFee
    | DoorSplit of DoorSplit
    | Versus    of Versus
    | VenueHire of VenueHire

let dispatchAccept workflow appId pm =
    match workflow with
    | FlatFee w   -> w.AcceptAsync(appId)
    | DoorSplit w -> w.AcceptAsync(appId, pm)
    | Versus w    -> w.AcceptAsync(appId, pm)
    | VenueHire w -> w.AcceptAsync(appId)
```

This is clean. No marker interfaces, no throw-fallthrough. If you add a variant and forget this match, the compiler tells you.

**Why F# still loses to Rust on pure technical fit:**

1. **F#'s native expression-problem mechanism is the asymmetric one.** DUs + match = closed variants, open operations. Adding an operation is a new function with a match: purely additive. Adding a contract variant forces an edit to every match site — which is the point of exhaustiveness, but it's the wrong direction when your operations are also growing.

2. **No traits / no typeclasses.** F# has .NET interfaces (back to runtime `:?>` casts) and SRTP (statically-resolved type parameters — member-shape constraints that approximate typeclasses inline, but with poor error messages, slow compilation, and no associated types). F# 7's trait constraints cleaned SRTP up somewhat but it's still not Rust's trait system.

3. **No associated types.** The mechanism that lets each impl carry its own `Input` type doesn't exist in F#. `IAccept<'Input>` works as a generic interface, but the dispatcher can't hold "any IAccept" without erasing `'Input` to `obj` — runtime casts return.

4. **The F# dispatch doesn't share a method signature across arms.** In the match above, `w.AcceptAsync(appId)` and `w.AcceptAsync(appId, pm)` are two different method shapes. That works — but it means there's no unifying `Accept` trait. You can't write a generic `runAccept<W: Accept>` that abstracts the dispatch shape. The dispatcher is the only place that knows about Accept. New operation = new dispatcher from scratch, no shared scaffolding.

**Honest framing**: F# wins if you weight pragmatism / .NET runtime continuity / smallest migration cost. Rust wins on pure technical fit for the symmetric expression problem. If the criteria includes "stays on .NET, can call existing Stripe SDK, works within Aspire", F# is the right answer. On the criteria of "best language to express this dispatch shape", the associated types + monomorphisation story in Rust is the more complete answer.

---

## Closest competitor: Haskell

Type classes were invented for the expression problem — it's the motivating example in the original Wadler/Blott paper. Haskell's answer is canonical:

```haskell
class Accept contract where
    type Input contract
    accept :: contract -> ApplicationId -> Input contract -> IO (Either WorkflowError ())

instance Accept FlatFee where
    type Input FlatFee = ()
    accept w appId () = ...

instance Accept DoorSplit where
    type Input DoorSplit = PaymentMethodId
    accept w appId pm = ...
```

Adding a class is purely additive. Adding an instance is purely additive. Constraints at call sites (`Accept c => ...`) enforce capability presence at compile time. Errors are values via `Either`/`ExceptT`.

Haskell is the most historically pure answer to this question. It loses to Rust on three practical grounds, none of which are ecosystem:

1. **Strict evaluation.** A payment/booking system has side effects that must happen in a precise order: capture payment, write booking, release escrow. Haskell's laziness means controlling evaluation order requires `seq`/bang patterns/`IO` discipline throughout. Rust is strict by default — effects happen when you call them.

2. **Predictable runtime.** Payment-capture code shouldn't pause for GC. Rust monomorphises trait dispatch with no boxing where the type is statically known. Haskell's typeclass dispatch is dictionary-passing on the heap.

3. **One error mechanism.** Haskell has both `Either` (pure) and `IO` exceptions, and managing the boundary between them is friction. Rust has `Result<T, E>` everywhere. The current code's failure mode is `throw new BadRequestException(...)` from a `switch` — `Result` directly replaces that pattern, uniformly.

Rust traits are type classes with monomorphisation and implementation coherence rules. Same theoretical power; runtime model better suited to orchestration code.

---

## Verdict

**Rust.** Trait + associated types model the capability × contract matrix natively. Sealed `enum` over workflows makes the DB-tag dispatch total at compile time. No inheritance prevents drift back toward runtime casts. No exceptions remove the "wrong thing fell through the switch" failure category.

The honest cost: the entry-boundary `match` *does* edit when you add a contract variant. But the compiler tells you exactly which match sites need updating — which is precisely the property you want from "add a contract variant safely."

---

## Migration scoping

**Moves to Rust** (the whole vertical — Concert + Contract bounded contexts):
- `Concertable.Concert.{Domain,Application,Infrastructure,Api,Contracts}` — including `ApplicationService`, `OpportunityService`, `BookingService`, `ConcertService`. The orchestrators move with the workflow they orchestrate.
- `Concertable.Concert.Api` controllers → `axum`/`tonic` handlers
- `Concertable.Contract.{Domain,Application,Infrastructure,Contracts}` — folded into the Rust Concert service; only Concert consumes it
- Tables in `concert.*` and `contract.*` schemas
- Artist/Venue read-model projection handlers — Rust subscribes to broker events the same way .NET does today
- QR / PDF generation (Rust crates: `qrcode`, `genpdf` / `printpdf`)

**Half-extraction is the wrong target.** Keeping `ApplicationService` in .NET and only the dispatcher in Rust splits the orchestrator from the thing it orchestrates. Every workflow request becomes a round-trip; transactional coordination across the wire becomes a new problem; latency rises for the most common operations. If the whole vertical isn't worth moving, don't move anything.

**Stays in .NET:**
- Identity, Artist, Venue, Customer, Search, Payment, Notification, Messaging, the Web/SPA host, Workers
- Payment remains the single owner of Stripe credentials + webhook handler; Rust calls it via gRPC
- Notification sends emails/push; Rust publishes domain events, .NET Notification subscribes

**Deleted:** nothing (no data loss). The .NET Concert+Contract project tree is removed; their DI registrations are removed; their migrations folder is removed (Rust manages its own schema via `sqlx`/`refinery` or equivalent).

**Cross-boundary mechanics:**
- Rust → Stripe: via gRPC to .NET Payment (one service owns Stripe creds + webhook)
- Stripe → Rust: Stripe → .NET Payment webhook → publish `PaymentSucceededEvent` → broker → Rust subscribes
- Rust → .NET (notifications, search): publish domain events, .NET modules subscribe
- SPA → Rust: directly (Aspire service discovery resolves the Rust host)
- Rust auth: validate JWT locally against the .NET Identity public key

**Scoping nuance — Opportunity:** Opportunity creation is pure CRUD and could stay in .NET, but apply-checkout and apply itself both need the opportunity's contract type at dispatch time. Keeping Opportunity in .NET means adding an Opportunity read-model projection on the Rust side (subscribe to `OpportunityCreated` / `OpportunityUpdated`). Defensible for a smaller Rust surface; the simpler default is to keep the whole Concert lifecycle (Opportunity → Application → Concert → Booking → Ticket) in one bounded context.
