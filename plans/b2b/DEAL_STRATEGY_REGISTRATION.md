# Investigation: consolidating the per-`DealType` keyed-strategy registration

## Prompt (run in a fresh session)

Read this file in full, then `plans/AGENTS.md` and `api/agents/CODE_PATTERNS.md`. You are implementing
the **staged recommendation** below on a new `Refactor/<Name>` branch off `origin/main` (these resolvers
are already on `main` — do **not** build this on a feature branch).

Start with **step 1 only** — the completeness guard (an architecture/unit test asserting each keyed
family covers its *declared* `DealType` set; coverage is non-uniform, so it is per-family, not "all 4").
Land it as its own PR. **Stop there** and hand back — step 2 (the fluent strategy-map registry) is a
separate PR and starts only when explicitly told to, and only after the open decisions below are settled.

## Why this exists

The codebase resolves every rule that varies by `DealType` through the keyed-strategy resolver pattern
(`CODE_PATTERNS.md`) — a facade holding a `FrozenDictionary<DealType, IStrategy>` and delegating. This
is the *right* call: it keeps `DealType` branching out of agnostic services (there is **zero** `switch`/
`?:` on `DealType` anywhere in B2B — verified). The complaint is not the pattern; it's the
**proliferation** of hand-rolled instances of it and the absence of any safety net over the scatter.

This doc inventories that surface, states the real pains precisely, and lays out design options. It is a
**design investigation** — no option is chosen yet; the open decisions at the end are Tommy's.

> Scope/branching note: these resolvers already live on `main`, so the implementation is a
> `Refactor/<Name>` branch off `origin/main`, **not** work bolted onto `Feature/PricingTransparency`
> (the branch this doc happens to be authored on). This doc is a plan — exempt from branch hygiene — so
> it can ride any branch; the code cannot.

## The surface — every per-`DealType` keyed family in B2B

Two registration idioms coexist today.

### Idiom A — hand-maintained facade + ctor dictionary (the smell)

Each family registers every concrete leaf in DI **and** re-declares the `DealType → leaf` map by hand in
the facade constructor. The map is written **twice** (DI registration + ctor dict) and nothing checks it
is complete.

| Family (facade) | Interface | Module / layer | Lifetime | Leaves | `DealType` coverage |
|---|---|---|---|---|---|
| `DealMapper` | `IDealMapper` | Deal.Application | singleton | 4 | all 4, 1:1 |
| `DealUpdater` | `IDealUpdater` | Deal.Infrastructure | singleton | 4 | all 4, 1:1 |
| `PaymentAmountMapper` | `IPaymentAmountMapper` | Concert.Application | singleton | 4 | all 4, 1:1 |
| `SettlementAmountResolver` | `ISettlementAmountResolver` | Concert.Infrastructure | **scoped** | 3 | all 4 → **3** (DoorSplit+Versus share `RevenueShare`) |
| `ArtistShareCalculator` | `IArtistShareCalculator` | Concert.Application | singleton | 2 | **partial — DoorSplit, Versus only** |
| `TicketPayeeResolver` | `ITicketPayeeResolver` | Concert.Application | singleton | 2 | all 4 → **2** (`Venue`/`Artist`) |
| `SettlementPayeeResolver` | `ISettlementPayeeResolver` | Concert.Application | singleton | 2 | all 4 → **2** (`Venue`/`Artist`, **inverse** of Ticket) |
| `DealTermsRenderer` | `IDealTermsRenderer` | Concert.Application | singleton | 4 | all 4, 1:1 |
| `DealTermsSerializer` | `IDealTermsSerializer` | Concert.Application | singleton | 4 | all 4, 1:1 |

### Idiom B — fluent single-declaration builder (already blessed, already in the tree)

`AddConcertWorkflows` + `ConcertWorkflowBuilder` + `ConcertWorkflowRegistryBuilder` declare each
`DealType` **once**, fluently, and derive everything from that one pass: the per-type steps, the lifecycle
state-machine transitions, the keyed workflow (`AddKeyedScoped<IConcertWorkflow, T>(dealType)`), the
capability registry, and the state-machine registry. Crucially, `ConcertWorkflowBuilder.Build()` **throws**
if a type is under-declared (`"No workflow registered for {dealType}"`) — it fails at composition, not at
the first request in prod.

**The design answer is largely "make Idiom A look like Idiom B."** We are propagating a pattern the module
already commits to, not inventing one — exactly what `CODE_PATTERNS.md` asks.

## The real pains (precise)

1. **Double declaration.** Every Idiom-A family states the same `DealType → strategy` fact in two places
   (DI leaf registrations + the ctor dict). They drift independently.
2. **No completeness safety, and coverage is non-uniform.** A missing entry is a runtime
   `KeyNotFoundException` on one code path in prod, not a compile/boot error. And a naive "every family
   covers all 4 types" check is **wrong**: `ArtistShareCalculator` is legitimately partial (flat-fee deals
   have no artist revenue share). Any guard must work off a **per-family declared expected set**.
3. **The map is many-to-few and not derivable from types.** `Settlement`/`TicketPayeeResolver` map 4 types
   onto the same 2 `Venue`/`Artist` leaves (as inverse maps); `SettlementAmountResolver` maps 4 onto 3.
   The fact "FlatFee settles to the *artist*, VenueHire to the *venue*" lives **only** in the explicit map
   — it cannot be inferred from the leaf types. → **the explicit mapping must be preserved**; this rules
   out pure auto-registration and source-generation-by-convention.
4. **Boilerplate proliferation.** Nine near-identical facade classes (frozen-dict field + ctor + one
   delegating method) — pure ceremony, repeated per concern.
5. **Discoverability.** Nothing enumerates "the per-`DealType` concerns," so adding a 5th deal type is a
   scavenger hunt across ~9 files in two modules and two layers, with the compiler offering no help.

## Design options

### Option A — completeness guard only (smallest; de-risks #2)

Leave the shape. Add one architecture/unit test (or a startup validation) that reflects over the keyed
facades and asserts each covers its **declared** expected set. Families opt into `all DealTypes` or
declare a subset (so `ArtistShareCalculator` declares `{DoorSplit, Versus}` and passes).

- **Pro:** tiny, zero-risk, immediately turns "silently incomplete" into "fails the build." Independent of
  any later restructure — worth doing first regardless.
- **Con:** addresses only the *risk*, not the double-declaration (#1), boilerplate (#4), or discoverability
  (#5). The smell remains.

### Option B — fluent strategy-map registry (recommended; mirrors Idiom B)

Introduce a generic registry built once via a fluent pass, exactly like `ConcertWorkflowBuilder`:

```csharp
// Registration — the map declared ONCE, explicit, readable at a glance, completeness-gated
services.AddDealStrategy<IPaymentAmountMapper>(m => m
    .For(DealType.FlatFee,   sp => new FlatFeePaymentAmountMapper())
    .For(DealType.DoorSplit, sp => new DoorSplitPaymentAmountMapper())
    .For(DealType.Versus,    sp => new VersusPaymentAmountMapper())
    .For(DealType.VenueHire, sp => new VenueHirePaymentAmountMapper())
    .RequireAll());          // Build() throws now if a DealType is missing — like the workflow builder

// Inverse many-to-few reuse stays explicit and obvious:
services.AddDealStrategy<ISettlementPayeeResolver>(m => m
    .For(DealType.FlatFee,   Artist).For(DealType.DoorSplit, Artist)
    .For(DealType.Versus,    Artist).For(DealType.VenueHire, Venue)
    .RequireAll());

services.AddDealStrategy<IArtistShareCalculator>(m => m
    .For(DealType.DoorSplit, ...).For(DealType.Versus, ...)
    .RequireExactly(DealType.DoorSplit, DealType.Versus));   // partial by design, asserted
```

Consumers inject `IDealStrategyMap<IPaymentAmountMapper>` and call `.For(deal.DealType).ToPaymentAmount(deal)`,
**or** we keep a one-line named facade where the name earns its keep (`IPaymentAmountMapper` reads better at
a call site than `IDealStrategyMap<IPaymentAmountMapper>`).

- **Pro:** kills the nine hand-rolled facades and the double-declaration; the fluent block *is* the explicit
  map (keeps `CODE_PATTERNS`' "the dictionary is the rule, written once, readable at a glance" spirit);
  completeness is baked into `Build()`; respects per-family lifetime; handles many-to-few reuse naturally;
  it is the *same* mechanism the workflow builder already proves.
- **Con:** one new generic abstraction to learn; a `CODE_PATTERNS.md` update to make this *the* way (see open
  decisions). Migration touches ~9 families (mechanical, one family at a time, each independently green).

### Option B-variant — keyed DI instead of the fluent registry (not recommended)

Register leaves with `AddKeyedSingleton<IPaymentAmountMapper, FlatFeePaymentAmountMapper>(DealType.FlatFee)`
and have one generic facade resolve via `IKeyedServiceProvider`.

- **Con:** this is exactly the service-location `CODE_PATTERNS.md` argues *against* inside these facades
  ("not `GetRequiredKeyedService`, not `IServiceProvider`"); it scatters the map across N `AddKeyed…` lines
  (less readable-at-a-glance than one fluent block); and many-to-few reuse gets clumsy. The workflow builder
  uses keyed DI only for the *workflow object itself*, resolved inside the composition root — not as the
  consumer-facing dispatch. Prefer the fluent registry.

### Option C — source generator (strongest guarantee, heaviest; not recommended now)

A Roslyn generator emitting facades + a **compile-time** incomplete-coverage diagnostic.

- **Con:** because the map is many-to-few and not type-derivable (#3), the generator still needs explicit
  per-entry attributes — so it buys compile-time-vs-boot-time exhaustiveness at the cost of build infra and
  a generator to maintain, for a 4-value enum. Not worth it. Revisit only if `DealType` families explode.

## Recommendation

Staged, each step independently valuable and shippable:

1. **Option A first** — land the completeness guard as its own small PR. Immediate de-risk, no shape change,
   and it becomes the safety net that makes the Option-B migration provably behaviour-preserving.
2. **Then Option B** — migrate the nine Idiom-A families to the fluent registry, one family per commit
   (build + that family's unit tests green each time), and fold the completeness assertion into `Build()`.
   Do **not** collapse concerns into a per-type god-object and never reintroduce a `switch` — the
   concern-based split stays; only the registration mechanism consolidates.

Both keep the architecture the user explicitly wants (no switches) while removing the "disjointed frozen
dictionaries everywhere" smell and the silent-incompleteness footgun.

## Open decisions (Tommy)

1. **`CODE_PATTERNS.md` stance.** The keyed-resolver section currently mandates the explicit ctor dictionary
   and argues against keyed DI/service-location. Option B replaces the ctor dict with a fluent registry —
   still explicit, but a different mechanism. Do we amend `CODE_PATTERNS.md` to make the registry *the* way
   (Idiom A retired), or have the two coexist with a rule for when to use which?
2. **Named facades vs. raw generic.** Keep thin named facades (`IPaymentAmountMapper`) over
   `IDealStrategyMap<T>` for call-site readability, or have consumers inject the generic directly for zero
   boilerplate?
3. **Scope.** Concert module only, or include Deal's `DealMapper`/`DealUpdater` in the same sweep? (They
   share the exact shape, so one sweep is cheaper than two.)
