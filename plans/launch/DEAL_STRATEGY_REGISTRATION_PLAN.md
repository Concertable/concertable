# Deal-type strategy registration refactor

> **Next steps live in @plans/launch/DEAL_STRATEGY_REGISTRATION_PROGRESS.md → `## Next Steps`.**

## Goal

Replace the repeated, hand-maintained `DealType → strategy` dictionaries with one explicit strategy
registration mechanism per owning module. Preserve the codebase's existing suffix semantics:

- a **factory** creates or obtains a selected DI strategy for the caller;
- a **resolver** consumes that strategy internally and returns the final domain answer;
- a **mapper**, **renderer**, **serializer**, or **calculator** continues to name the operation it performs.

The refactor is structural. It must not change deal economics, payment direction, terms fingerprints,
workflow transitions, checkout availability, settlement values, or API shapes.

## Decisions locked by the design review

1. **No `IDealPolicy`, `IDealDefinition`, or global `DealContext`.** Grouping unrelated dependencies
   because they share a key creates a vague dependency bag rather than a useful domain abstraction.
2. **The runtime selection abstraction is a factory.** It returns the selected strategy object; named
   resolvers and other facades still return final business values.
3. **Registration is vertical and single-source.** Each `DealType` is configured once at the owning
   module's composition root, including the strategy implementation and lifetime for every concern.
4. **Application use cases do not use `IServiceProvider`, keyed DI, dictionaries, or the generic factory
   directly.** Named facades keep their current operation-specific interfaces and delegate through the
   generic factory. The existing `IConcertWorkflowFactory` remains the named factory used by lifecycle
   orchestrators.
5. **Factories are module-local.** Concert owns the factory for Concert runtime strategies. Deal owns a
   separate equivalent for Deal's mapper/updater strategies. There is no cross-module runtime registry
   and no B2B-specific factory in a cross-service shared package.
6. **`IDealAccessor` stays in Concert.** It resolves Concert-owned identifiers to `IDeal` through
   `IDealModule` and caches the loaded deal for one operation. It neither owns nor performs strategy
   dispatch, and the strategy factory never reads an ambient key from it.
7. **The design must survive the .NET 11/C# 15 union migration.** The outer `DealType → IConcertWorkflow`
   dispatch remains unchanged when Apply/Accept/Checkout capability interfaces become union-valued
   workflow properties.

## Ownership and dependency direction

### Concert module

The primary factory belongs here because the selected strategies are Concert behaviours: terms
presentation/fingerprinting, payment projection, party direction, settlement calculation, and workflow
execution.

Proposed locations:

```text
Concertable.B2B.Concert.Application/
├─ Strategies/IConcertDealStrategyFactory.cs
├─ Interfaces/IDealTerms.cs
├─ Interfaces/IDealPayeeResolver.cs
└─ existing operation-specific interfaces

Concertable.B2B.Concert.Infrastructure/
├─ Services/Strategies/ConcertDealStrategyFactory.cs
├─ Services/Strategies/ConcertDealStrategyBuilder.cs
└─ Extensions/ServiceCollectionExtensions.cs   // one vertical registration block
```

`IConcertDealStrategyFactory<TStrategy>` is internal Application plumbing. Its implementation and all
keyed-DI access remain Infrastructure concerns.

### Deal module

Deal owns EF TPH entities, contract mapping, creation and mutation. Its two keyed families must not be
registered through Concert. A Deal-local factory/builder can use the same pattern under the Deal
namespace without creating a runtime reference between modules.

Longer term, a closed union for the Deal contract may replace these pure mapping/mutation strategies
with exhaustive matching. That does not block the Concert refactor and is not a reason to put the
current factory in Deal.Contracts.

### `IDealAccessor`

`IDealAccessor` remains:

```text
interface:      Concert.Application/Interfaces/IDealAccessor.cs
implementation: Concert.Infrastructure/Services/DealAccessor.cs
```

That ownership is required because its input identifiers belong to Concert:

```text
OpportunityId / ApplicationId / ConcertId
    → Concert repositories resolve DealId
    → IDealModule returns the Deal-owned contract
    → DealAccessor caches IDeal for the operation
```

The dispatch flow stays explicit:

```csharp
var deal = await dealResolver.ResolveByApplicationIdAsync(applicationId);
var workflow = workflowFactory.Create(deal.DealType);

// A selected step may consume IDealAccessor.Deal after the orchestrator seeded it.
await workflow.Book.ExecuteAsync(bookingId);
```

Do not change this to `factory.Create()` with no key, do not inject `IDealAccessor` into the generic
factory, and do not add middleware that establishes a request-global `DealType`. HTTP controllers,
integration-event handlers, and workers must all use the same explicit selection boundary.

## Current surface

Nine hand-written frozen maps currently repeat the same dispatch mechanism:

| Module | Family | Coverage |
|---|---|---|
| Deal | `DealMapper` | all four, 1:1 |
| Deal | `DealUpdater` | all four, 1:1 |
| Concert | `DealTermsRenderer` | all four, 1:1 |
| Concert | `DealTermsSerializer` | all four, 1:1 |
| Concert | `PaymentAmountMapper` | all four, 1:1 |
| Concert | `TicketPayeeResolver` | all four → Venue/Artist |
| Concert | `SettlementPayeeResolver` | inverse of ticket payee |
| Concert | `ArtistShareCalculator` | DoorSplit/Versus only |
| Concert | `SettlementAmountResolver` | all four → three implementations |

Workflow selection adds keyed DI, a workflow CLR-type registry, and a state-machine registry, but these
are already derived from the single `AddConcertWorkflows` builder. The refactor extends that
single-declaration property to the remaining Concert strategies instead of replacing it with another
parallel mechanism.

There are no production `switch (dealType)` or `DealType == ...` business rules to preserve or migrate.
Runtime type/union matching inside a selected workflow is a different dispatch axis and remains legal.

## Target runtime pattern

### Factory contract

The noun before `Factory` is what it returns. `DealType` is only the selection key:

```csharp
internal interface IConcertDealStrategyFactory<out TStrategy>
    where TStrategy : class
{
    TStrategy Create(DealType dealType);
}
```

Infrastructure owns the only keyed-service lookup:

```csharp
internal sealed class ConcertDealStrategyFactory<TStrategy>
    : IConcertDealStrategyFactory<TStrategy>
    where TStrategy : class
{
    private readonly IKeyedServiceProvider services;

    public ConcertDealStrategyFactory(IKeyedServiceProvider services) =>
        this.services = services;

    public TStrategy Create(DealType dealType) =>
        services.GetRequiredKeyedService<TStrategy>(dealType);
}
```

This service is scoped so a selected strategy may safely depend on scoped repositories, accessors, or
clients. Stateless leaves may remain singleton registrations; scoped consumers obtain them through the
same factory without changing the calling convention. Every unkeyed facade that injects this factory
must therefore become scoped; a singleton facade must never capture it.

### Vertical composition

The builder wraps keyed DI and records coverage/lifetime metadata. The concrete API may be adjusted for
clarity during Phase 1, but the resulting registration must read vertically:

```csharp
services.AddConcertDealStrategies(strategies =>
{
    strategies.For(DealType.FlatFee)
        .AddSingleton<IDealTerms, FlatFeeDealTerms>()
        .AddSingleton<IDealPayeeResolver, VenuePaysArtistDealPayeeResolver>()
        .AddSingleton<IPaymentAmountMapper, FlatFeePaymentAmountMapper>()
        .AddScoped<ISettlementAmountResolver, FlatFeeSettlementAmount>()
        .AddWorkflow<FlatFeeWorkflow>(workflow => workflow
            .WithApply<SimpleApplyStep>()
            .WithCheckout<HoldCheckoutStep>()
            .WithAccept<CaptureEscrowAcceptStep>()
            .WithEscrowPayment()
            .WithBook<CreateConcertDraftStep>()
            .WithFinish<ReleaseEscrowFinishStep>(Complete)
            .WithCancel<RefundEscrowStep>()
            .WithApplicationCancel());

    strategies.For(DealType.DoorSplit)
        .AddSingleton<IDealTerms, DoorSplitDealTerms>()
        .AddSingleton<IDealPayeeResolver, VenuePaysArtistDealPayeeResolver>()
        .AddSingleton<IPaymentAmountMapper, DoorSplitPaymentAmountMapper>()
        .AddScoped<ISettlementAmountResolver, DoorSplitSettlementAmount>()
        .AddWorkflow<DoorSplitWorkflow>(workflow => workflow
            .WithApply<SimpleApplyStep>()
            .WithCheckout<VerifyCheckoutStep>()
            .WithAccept<PaidAcceptStep>()
            .WithVerifiedPayment()
            .WithBook<CreateConcertDraftStep>()
            .WithFinish<PayoutFinishStep>(AwaitingSettlement)
            .WithSettlement()
            .WithCancel<RefundEscrowStep>()
            .WithApplicationCancel());

    // Versus and VenueHire follow in the same block.

    strategies.RequireAll<IDealTerms>();
    strategies.RequireAll<IDealPayeeResolver>();
    strategies.RequireAll<IPaymentAmountMapper>();
    strategies.RequireAll<ISettlementAmountResolver>();
    strategies.RequireAll<IConcertWorkflow>();
});
```

`Build()` must throw during composition for duplicate mappings, a missing required type, an unexpected
extra type, a missing workflow, or a registration whose declared coverage is incomplete. A deliberately
partial family must call `RequireExactly<T>(...)`; no partial coverage is inferred from whichever
registrations happen to exist.

### Named facades remain the business API

Consumers do not receive the generic factory merely to select implementation machinery. A named facade
uses it internally and continues returning the final answer appropriate to its suffix:

```csharp
internal sealed class SettlementAmountResolver : ISettlementAmountResolver
{
    private readonly IConcertDealStrategyFactory<ISettlementAmountResolver> strategies;

    public SettlementAmountResolver(
        IConcertDealStrategyFactory<ISettlementAmountResolver> strategies) =>
        this.strategies = strategies;

    public Task<Money> ResolveGrossAsync(
        int concertId,
        IDeal deal,
        CancellationToken ct = default) =>
        strategies.Create(deal.DealType).ResolveGrossAsync(concertId, deal, ct);
}
```

The unkeyed `ISettlementAmountResolver` registration is the facade. Only leaf implementations are keyed,
so the factory cannot recursively return the facade.

`IConcertWorkflowFactory` remains a named factory because lifecycle callers genuinely need the selected
workflow object. It may delegate to `IConcertDealStrategyFactory<IConcertWorkflow>` after migration:

```csharp
internal sealed class ConcertWorkflowFactory : IConcertWorkflowFactory
{
    private readonly IConcertDealStrategyFactory<IConcertWorkflow> strategies;

    public IConcertWorkflow Create(DealType type) => strategies.Create(type);
}
```

## Cohesive combinations included

### Terms

Replace the parallel renderer and serializer strategy families with one per-type terms strategy:

```csharp
internal interface IDealTerms
{
    string Render(IDeal deal);
    string Serialize(IDeal deal);
}
```

Rendering and canonical serialization remain separate methods with separate formatting rules. Combining
their selection does not permit rendered presentation text to become fingerprint input.

### Payee direction

Replace the inverse ticket/settlement payee maps with one resolver family whose per-type strategy owns
the coherent supply/payment direction:

```csharp
internal interface IDealPayeeResolver
{
    Guid ResolveTicketUserId(ConcertEntity concert);
    Guid ResolveTicketTenantId(ConcertEntity concert);
    Guid ResolveSettlementTenantId(ConcertEntity concert);
}
```

The named resolver returns final identifiers; it does not expose a `Venue`/`Artist` enum for consumers to
reinterpret. FlatFee, DoorSplit, and Versus use the venue-pays-artist resolver; VenueHire uses the inverse.

### Settlement calculation

Remove the nested dispatch `SettlementAmountResolver → RevenueShareSettlementAmount →
ArtistShareCalculator`. DoorSplit and Versus receive type-specific settlement leaves that share the
revenue-loading mechanism but own their distinct formula. Delete the duplicate production-looking
`CalculateArtistShare` methods on Deal entities once no production or test path depends on them; tests
must exercise the real production calculation rather than preserve a second formula as an oracle.

## Compatibility with .NET 11/C# 15 unions

As of .NET 11 Preview 5, C# 15 preview unions support union case types, implicit conversion from a case,
and exhaustive pattern matching. The syntax remains preview and may change before release, so the future
example is illustrative rather than an implementation dependency.

The factory selects the outer workflow by `DealType`. A future union selects the shape of a step inside
that already-selected workflow. These are independent dispatch boundaries:

```csharp
internal union AcceptBehavior(ISimpleAcceptStep, IPaidAcceptStep);

internal interface IConcertWorkflow
{
    DealType Type { get; }
    AcceptBehavior Accept { get; }
    IBookStep Book { get; }
    IFinishStep Finish { get; }
    ICancelStep Cancel { get; }
}
```

A concrete workflow still receives its DI dependency normally; the union conversion occurs at assignment:

```csharp
internal sealed class FlatFeeWorkflow : IConcertWorkflow
{
    public FlatFeeWorkflow(CaptureEscrowAcceptStep accept, /* other steps */) =>
        this.Accept = accept;

    public DealType Type => DealType.FlatFee;
    public AcceptBehavior Accept { get; }
    // Other workflow members omitted.
}
```

The executor changes only its inner capability dispatch:

```csharp
var deal = await dealResolver.ResolveByApplicationIdAsync(applicationId);
var workflow = workflowFactory.Create(deal.DealType);

await (workflow.Accept switch
{
    ISimpleAcceptStep simple => simple.ExecuteAsync(applicationId),
    IPaidAcceptStep paid when paymentMethodId is not null =>
        paid.ExecuteAsync(applicationId, paymentMethodId),
    IPaidAcceptStep => throw new BadRequestException("A payment method is required."),
    _ => throw new InvalidOperationException("The Accept union is unset."),
});
```

The generic factory contract, vertical registration, `IDealAccessor`, state machine, and lifecycle
orchestrators do not change. Until the union migration lands, the existing capability interfaces and
capability registry remain derived from workflow registration. This refactor must not add new marker
capabilities or make the generic factory depend on their current shape.

## Delivery shape

One implementation PR is sufficient: no package or service boundary changes. Keep every phase green and
commit it independently so review can verify the migration incrementally. The merge queue must run full
E2E because this is a broad refactor of booking, checkout, payment, and settlement dispatch.

### Phase 1 — Factory infrastructure and terms pilot ✅

- Add characterization tests for current rendering and canonical serialization across all four types.
- Add `IConcertDealStrategyFactory<TStrategy>`, its scoped Infrastructure implementation, and the
  vertical builder with duplicate/coverage/lifetime validation.
- Combine renderer/serializer leaves into `IDealTerms` and migrate the terms facades as the first user.
- Change the migrated unkeyed terms facade from singleton to scoped so it cannot capture the scoped
  factory; retain singleton lifetime only for stateless keyed leaves.
- Keep terms fingerprints byte-for-byte stable.
- Do not migrate another family until the factory tests demonstrate correct keyed resolution from a
  request scope and failure at composition for invalid registration.

Verification gate:

- `dotnet build api/Concertable.slnx` — 0 errors.
- Concert unit and integration tests via `integration-debug`.

### Phase 2 — Payee direction and payment projection ✅

- Replace `TicketPayeeResolver`, `SettlementPayeeResolver`, `VenuePayeeResolver`, and
  `ArtistPayeeResolver` with the cohesive `IDealPayeeResolver` facade plus two directional leaves.
- Migrate `PaymentAmountMapper` to the factory without changing its response union/wire shapes.
- Move these registrations into the same vertical per-`DealType` composition block.
- Add table-driven tests covering ticket user, ticket tenant, and settlement tenant for all four types.

Verification gate:

- `dotnet build api/Concertable.slnx` — 0 errors.
- Concert unit and integration tests via `integration-debug`.

### Phase 3 — Settlement calculation

- Migrate `ISettlementAmountResolver` to the factory.
- Remove the second `ArtistShareCalculator` dispatch by giving DoorSplit and Versus settlement leaves
  their formula directly over a shared revenue-loading collaborator/base seam.
- Remove duplicated Deal-entity share formulae after replacing tests that use them as a second oracle.
- Verify exact gross values for FlatFee, DoorSplit, Versus, and VenueHire, including ticket plus declared
  door revenue for revenue-share deals.

Verification gate:

- `dotnet build api/Concertable.slnx` — 0 errors.
- Deal and Concert unit/integration tests via `integration-debug`.

### Phase 4 — Workflow composition convergence

- Make the vertical builder the sole source for workflow keyed registration, workflow-type metadata,
  lifecycle state machines, and the migrated Concert strategy families.
- Retain `IConcertWorkflowFactory` as the named factory used by executors and checkout dispatch.
- Keep the existing capability registry operational until the separate union migration replaces it.
- Add startup/architecture tests proving each `DealType` has exactly one workflow and state machine.

Verification gate:

- `dotnet build api/Concertable.slnx` — 0 errors.
- Concert unit and integration tests via `integration-debug`.

### Phase 5 — Deal-module families and convention cleanup

- Add the Deal-local strategy factory/builder and migrate `DealMapper` and `DealUpdater` without allowing
  Concert to reference Deal runtime projects.
- Preserve EF TPH creation/update validation and JSON-polymorphic contract shapes.
- Delete the unused `IDealStrategy` marker and correct the stale Deal architecture claim that Payment
  still provides `IStripeValidationStrategy`.
- Update `api/agents/CODE_PATTERNS.md` with the factory/builder pattern and suffix distinctions.
- Add an architecture gate with an explicit allowlist proving:
  - no hand-written `FrozenDictionary<DealType, ...>` facade remains;
  - no consumer directly uses `IKeyedServiceProvider`/`GetRequiredKeyedService`;
  - every required strategy family declares exact coverage;
  - the only keyed-service lookup is inside the module-local factory implementation.

Verification gate:

- `dotnet build api/Concertable.slnx` — 0 errors.
- Deal and Concert unit/integration tests via `integration-debug`.
- Final local gate stops here; full API + UI E2E run in the merge queue.

## Definition of done

- Each `DealType → strategy` mapping is declared once per owning module.
- Adding a new `DealType` makes composition/tests fail until every required family is registered or
  deliberately declares partial coverage.
- Consumers retain operation-specific interfaces and suffix-correct behaviour.
- `IDealAccessor` remains Concert-owned and is not a hidden dispatch key.
- The current capability implementation still works, and the documented union replacement requires no
  factory or registration redesign.
- The Deal and Concert module boundaries remain package-clean.
- Build, affected unit/integration tests, code review, full merge-queue E2E, merge, and platform-sync
  observation are all terminal before the plan and ledger are deleted.
