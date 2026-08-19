# Deal representation and common-interface dispatch

> **Next steps live in @plans/launch/DEAL_CLOSED_SUM_MODEL_PROGRESS.md → `## Next Steps`.**

## 1. Decision

Deal representation, common-interface dispatch, and heterogeneous operation modelling are separate
decisions.

The long-term published representation is a C# 15 `closed record class Deal` with four sealed direct
cases. It preserves shared Deal members, the explicit `$type` JSON protocol, record semantics, and the
parallel EF TPT entity hierarchy. `DealType` remains the stable, explicitly valued persistence, wire,
display, filtering, and diagnostic identity.

Deal-varying behaviour follows four mutually exclusive shapes:

1. **One honest common interface:** when every Deal case performs the same named call with exactly the
   same parameters and return type, keep one family interface and select its implementation through a
   module-specific invariant factory. A source generator owns the exhaustive factory implementation,
   its switch, the ugly closed generic types, registrations, and coverage diagnostics. Consumers never
   repeat the four-case switch and never use `IServiceProvider`, keyed DI, or a dictionary.
2. **Genuinely heterogeneous input, output, or capability:** use a Dunet union on net10 and a native C#
   15 union on .NET 11, then match it exhaustively in the operation that consumes it. Application
   acceptance is irrevocably exactly the paid/simple union. Lifecycle executors and steps belong here;
   they are not examples for the common-interface factory.
3. **No behavioural variation:** call one collaborator directly.
4. **Closed key-to-data variation:** use one owning immutable value or table, not DI.

This rejects both universal switching and universal keyed services. It also rejects the earlier
operation-owned switches for mapper, updater, and terms: their leaf interfaces already express one
honest common operation, so repeating the same selection switch in every facade would duplicate
dispatch. The switch is defined once by generated factory machinery and reused by every eligible
family.

The current proven common-interface families are exactly:

- Deal-owned `IDealMapper`;
- Deal-owned `IDealUpdater`;
- Application-owned `IDealTerms`.

No current Booking or Concert executor/step family qualifies. Do not manufacture a common interface
for acceptance, confirmation, completion, or other case-specific lifecycle operations merely to use
this factory.

## 2. Scope and dependencies

This plan owns:

- the published Deal representation and EF Deal hierarchy cut-over;
- the B2B-local generator/analyzer for common-interface Deal families;
- the Application and Deal module factories and generated registrations;
- classification of every provisional lifecycle selector delivered by the Application → Booking →
  Concert ownership refactor;
- net10 Dunet equivalents and C# 15 native unions for Deal-varying heterogeneous operations;
- architecture and generator tests that make a fifth Deal case fail at the intended stage.

It does not own unrelated keyed policies in Payment, Search, tax jurisdiction, provider selection, or
configuration selection.

Delivery is phase-ordered rather than blocked as one unit:

1. The B2B-local generator/analyzer, its real two-project fixtures, and the Deal-owned net10
   `IDealMapper`/`IDealUpdater` factory migration are independent of the lifecycle split and land first.
2. That foundation must be terminal on `main` before lifecycle PR #633 resumes. The lifecycle owner then
   consumes it for Application `IDealTerms` and the module-local heterogeneous operation factories on the
   actual split graph; no temporary keyed or handwritten dispatch layer is introduced.
3. The final Application, Booking, and Concert APIs must be delivered before this plan reconciles their
   complete operation catalog and begins the representation cut-over.
4. The .NET 11 workstream must establish the supported C# 15 compiler/runtime/consumer matrix before the
   native-union and published closed-Deal cut-over.

The net10 and C# 15 consumer surfaces are deliberately identical apart from `IDeal` becoming `Deal`.

## 3. Why a generator is selected

The required combination cannot be obtained from built-in keyed DI, a `FrozenDictionary`, or ordinary
handwritten generics alone:

| Requirement | Keyed DI / service provider | Frozen dictionary | Handwritten generic total factory | Generated total factory |
|---|---:|---:|---:|---:|
| No service location | No | Yes | Yes | Yes |
| No repeated per-family switch | Yes | No | Yes | Yes |
| No five-type generic expression in handwritten code | Yes | Yes | No | Yes |
| Build-time missing-family-case failure | No | No | Only by manually constructing every slot | Yes |
| C# 15 closed-hierarchy exhaustiveness | No | No | Yes | Yes |
| Module-specific factory constraints | Partial | Partial | Yes | Yes |
| Registrations derived rather than repeated | No | No | No | Yes |

Built-in keyed DI and dictionaries index by runtime keys. Neither makes a key set closed or makes a
missing entry a C# compile failure. The existing `IKeyedServiceProvider` wrapper also permits any
`TStrategy` to be requested and discovers an undeclared family only during resolution.

For four cases, the generated type switch is not a performance concession to a `FrozenDictionary`.
It performs a small set of runtime type tests followed by a direct field return; a frozen dictionary
still performs key hashing/indexing and a table lookup. Exact machine code remains a JIT detail and is
verified by benchmark only if dispatch appears in a measured hot path. The architectural win is earlier
failure and type coherence, not an asserted nanosecond result. A dictionary wins when keys are runtime-
extensible; Deal is deliberately closed.

The factory interface still has value even though its implementation is mechanical: it is the stable
module-owned component-selection contract, carries the invariant generic and marker constraints, hides
the generated long closed factory, and gives facades a substitutable test seam. Injecting the generated
concrete type directly would leak generator infrastructure into business code; deleting the interface
would put the selection switch back into every facade.

A handwritten total factory is mechanically sound, but it exposes this at registrations and tests:

```csharp
ApplicationDealStrategyFactory<
    IDealTerms,
    FlatFeeDealTerms,
    DoorSplitDealTerms,
    VersusDealTerms,
    VenueHireDealTerms>
```

That expression is generated code only. No application source, composition root, test, or consumer may
spell it.

The existing `DealStrategyFactory<TStrategy>` is not global: it is internal to Deal Application and
implemented in Deal Infrastructure. Application cannot consume it without creating an illegal
cross-module runtime dependency. The reusable part of the new design is the generator template, while
each emitted factory remains owned by the module whose families it selects.

Application and Deal factories have identical generated selection mechanics but disjoint type catalogs.
`IApplicationDealStrategyFactory<TStrategy>` admits only Application family interfaces implementing
`IApplicationDealStrategy` and is registered only with Application leaves in Application Infrastructure.
`IDealStrategyFactory<TStrategy>` admits only Deal family interfaces implementing `IDealStrategy` and is
registered only with Deal leaves in Deal Infrastructure. Consequently neither
`IApplicationDealStrategyFactory<IDealMapper>` nor `IDealStrategyFactory<IDealTerms>` satisfies its
marker constraint. A single shared runtime factory interface would erase that compiler-visible ownership
or require an additional module type parameter everywhere; it provides no useful simplification over the
short module-owned interfaces.

Booking and Concert do not receive factories for symmetry. A module gets its own marker, short factory
interface, Infrastructure anchor, and generated registrations only when it owns at least one proven
same-interface family. No current Booking or Concert executor/step family qualifies.

The generator is B2B-owned analyzer infrastructure, not a cross-service runtime registry. It emits
module-local code into each module Infrastructure compilation, so Application and Deal retain
independent composition roots without putting B2B concepts into a shared package. Its template is the
single maintained definition of dispatch; generated expansion in each Infrastructure assembly is the
intended equivalent of macroing the same proven switch.

## 4. Complete factory design

### 4.1 Handwritten Application surface

The module marker remains on net10 and .NET 11. It is not a per-case
`IDealStrategyFor<TDeal>` relationship and does not perform dispatch. It gives the generic factory a
real compile-time module boundary and gives the generator the exact family catalog.

```csharp
internal interface IApplicationDealStrategy
{
}

[DealStrategyFactoryContract(typeof(IApplicationDealStrategy))]
internal interface IApplicationDealStrategyFactory<TStrategy>
    where TStrategy : class, IApplicationDealStrategy
{
    TStrategy Create(IDeal deal);
}
```

The family interface inherits the module marker once. The leaf implementations implement only the
honest family contract:

```csharp
internal interface IDealTerms : IApplicationDealStrategy
{
    string Render(IDeal deal);
    string Serialize(IDeal deal);
}

internal sealed class FlatFeeDealTerms : IDealTerms
{
    public string Render(IDeal deal) =>
        $"Flat fee: {((FlatFeeDeal)deal).Fee}";

    public string Serialize(IDeal deal) =>
        $"Fee={((FlatFeeDeal)deal).Fee}";
}
```

The factory generic parameter is deliberately invariant: it declares neither `in` nor `out`. The
compiler therefore rejects assignment between factories of different strategy types. The marker
constraint also rejects requesting a Deal-module family such as `IDealMapper` from the Application
factory.

On C# 15 the only handwritten signature change is the base Deal type:

```csharp
[DealStrategyFactoryContract(typeof(IApplicationDealStrategy))]
internal interface IApplicationDealStrategyFactory<TStrategy>
    where TStrategy : class, IApplicationDealStrategy
{
    TStrategy Create(Deal deal);
}
```

There is no `IDealStrategyFor<TDeal>` in either version.

### 4.2 Handwritten Deal-module surface

The Deal module has its own marker and factory. It needs both the contract and entity selectors because
mapping starts from either hierarchy:

```csharp
internal interface IDealStrategy
{
}

[DealStrategyFactoryContract(typeof(IDealStrategy))]
internal interface IDealStrategyFactory<TStrategy>
    where TStrategy : class, IDealStrategy
{
    TStrategy Create(IDeal deal);
    TStrategy Create(DealEntity entity);
}

internal interface IDealMapper : IDealStrategy
{
    IDeal ToDeal(DealEntity entity);
    Result<DealEntity, ValidationErrors> ToEntity(IDeal deal);
}

internal interface IDealUpdater : IDealStrategy
{
    UnitResult<UpdateDealError> Apply(
        DealEntity existing,
        IDeal source);
}
```

On C# 15, `IDeal` becomes `Deal` and `DealEntity` becomes closed. The generator owns one total selector
for each distinct input hierarchy, not one selector per family.

### 4.3 Infrastructure generation anchors and discovery contract

The factory interfaces and markers stay in each Application assembly. Their
`DealStrategyFactoryContract` annotation drives Application-compilation diagnostics only; it does not
request generated runtime output. A source generator can add source only to the compilation in which it
runs, while the factory implementation, DI registrations, and composition root belong to the module
Infrastructure assembly. `InternalsVisibleTo` lets Infrastructure consume Application internals; it
does not make an Application annotation generate code in Infrastructure.

The exact placement is:

- the family marker, short factory interface, and business family interface live in the owning module's
  Application project;
- each concrete case leaf stays in the natural layer of the family it implements: terms and mapper
  leaves remain in their module Application projects, while updater leaves remain in Deal
  Infrastructure;
- the generated long factory, generated registrations, and composition-root call always live in the
  owning module's Infrastructure project;
- the reusable generator/analyzer lives once in B2B-local build tooling and adds no runtime dependency.

Each Infrastructure project therefore owns one handwritten generation anchor:

```csharp
[GenerateDealStrategyFactory(
    typeof(IApplicationDealStrategyFactory<>),
    typeof(IApplicationDealStrategy))]
internal static partial class ApplicationDealStrategyRegistration
{
}
```

The Deal Infrastructure project has the equivalent anchor:

```csharp
[GenerateDealStrategyFactory(
    typeof(IDealStrategyFactory<>),
    typeof(IDealStrategy))]
internal static partial class DealStrategyRegistration
{
}
```

The generator emits `DealStrategyFactoryContractAttribute` and
`GenerateDealStrategyFactoryAttribute` into the respective consuming compilations during
initialization; the analyzer/generator reference adds no runtime package or cross-module dependency.
In an Application compilation, the analyzer discovers each annotated factory contract, verifies its
marker constraint, treats every direct interface inheriting that marker as a declared family, and
rejects invalid factory uses visible in that compilation. It emits no runtime implementation there.

For each annotated Infrastructure anchor, the generator:

1. resolves the factory interface and marker supplied by the anchor and verifies that the factory's
   `TStrategy` constraint is that marker;
2. scans the Infrastructure compilation and its directly referenced Application assembly, using the
   existing `InternalsVisibleTo` boundary rather than requiring Application to reference Infrastructure;
3. treats every direct family interface inheriting that marker as a declared family;
4. reads each factory `Create(...)` input hierarchy;
5. derives the contract cases from the C# 15 closed descendants, or on net10 from concrete contract
   cases plus `[JsonDerivedType]` and `DealType` agreement;
6. maps a family case by the existing `<case stem><family name without I>` convention, such as
   `FlatFeeDealTerms`, `DoorSplitDealMapper`, and `VenueHireDealUpdater`;
7. emits a build error for a missing, duplicate, unsealed, unexpected, cross-module, inaccessible, or
   unconstructable case implementation;
8. emits a build error when factory `TStrategy` is the marker itself or is not one of that factory's
   declared family interfaces;
9. emits the exhaustive factory implementation and closed family registrations into Infrastructure,
   adding the registration extension to the annotated partial registration class.

The analyzer reference is present in both sides of the module pair: the Application-local contract
annotation drives factory-use diagnostics, while the Infrastructure-local anchor drives those
diagnostics plus generated output. Phase 0 must prove this actual two-project topology, including that
each annotation is invisible from the opposite compilation unless reached through the referenced factory
symbol; a single-compilation fixture is not acceptable evidence.

Compilation scope is deliberate. Application Infrastructure references Deal Contracts, not Deal Domain,
so its generator validates only the public Deal/JSON/enum catalog and emits only the contract selector.
Deal Infrastructure legitimately sees Deal Contracts, Application, and Domain; its generator additionally
validates the entity cases and emits both contract and entity selectors. Cross-module agreement with EF
metadata and generated TypeScript remains an architecture-test responsibility because neither artifact is
part of the Application generator compilation. The generator must not widen a project reference to obtain
a stronger-looking catalog guarantee.

The convention removes manual slot assignment, so a composition root cannot accidentally place
`DoorSplitDealTerms` in the FlatFee slot. It cannot prove that a correctly named class contains
semantically correct business code; no dispatch mechanism can prove that.

If a future family deliberately shares one implementation between cases, the generator may support an
explicit multi-case declaration only after a real need exists. The first implementation must not add
an unused aliasing feature. The current three families each have one real case implementation.

### 4.4 Representative generated net10 output

This is illustrative `.g.cs` output. It is never handwritten or referenced by its long closed type.

```csharp
internal sealed class ApplicationDealStrategyFactory<
    TStrategy,
    TFlatFee,
    TDoorSplit,
    TVersus,
    TVenueHire> : IApplicationDealStrategyFactory<TStrategy>
    where TStrategy : class, IApplicationDealStrategy
    where TFlatFee : class, TStrategy
    where TDoorSplit : class, TStrategy
    where TVersus : class, TStrategy
    where TVenueHire : class, TStrategy
{
    private readonly TFlatFee flatFee;
    private readonly TDoorSplit doorSplit;
    private readonly TVersus versus;
    private readonly TVenueHire venueHire;

    public ApplicationDealStrategyFactory(
        TFlatFee flatFee,
        TDoorSplit doorSplit,
        TVersus versus,
        TVenueHire venueHire)
    {
        this.flatFee = flatFee;
        this.doorSplit = doorSplit;
        this.versus = versus;
        this.venueHire = venueHire;
    }

    public TStrategy Create(IDeal deal) => deal switch
    {
        FlatFeeDeal => flatFee,
        DoorSplitDeal => doorSplit,
        VersusDeal => versus,
        VenueHireDeal => venueHire,
        _ => throw new ArgumentOutOfRangeException(
            nameof(deal),
            deal,
            null)
    };
}
```

The net10 fallback is unavoidable because `IDeal` is open. The generator/analyzer makes the known
catalog total at build time; the fallback remains the runtime guard against an external, corrupt, or
otherwise uncatalogued implementation.

There is no separate generated map and no forwarding factory. The long generated factory directly
implements the short handwritten interface and owns the only selection switch. The generator registers
one closed construction of this type for each declared family, so no handwritten source spells its five
generic arguments.

### 4.5 Representative generated C# 15 output

The generated factory keeps the same slots and constructor. Only the input and exhaustive switch change:

```csharp
public TStrategy Create(Deal deal) => deal switch
{
    FlatFeeDeal => flatFee,
    DoorSplitDeal => doorSplit,
    VersusDeal => versus,
    VenueHireDeal => venueHire
};
```

The compiler accepts the missing fallback because the four reachable direct descendants exhaust
`closed Deal`. Adding a fifth direct descendant makes this generated switch non-exhaustive until the
generator can bind the fifth family implementation. Non-exhaustive switch diagnostic `CS8509` is
elevated to an error in the affected B2B projects; exhaustiveness is not left as a warning.

The Deal-module factory additionally contains the equivalent `Create(DealEntity)` switch. This is one
generated switch per input hierarchy in the generic factory definition, not one handwritten switch per
mapper, updater, renderer, or serializer.

### 4.6 Generated registrations

The handwritten Application composition root contains only ordinary facades plus one generated
registration call:

```csharp
services.AddScoped<IDealTermsRenderer, DealTermsRenderer>();
services.AddScoped<IDealTermsSerializer, DealTermsSerializer>();
services.AddApplicationDealStrategies();
```

The generator adds the complete family registration to the annotated partial registration class:

```csharp
internal static partial class ApplicationDealStrategyRegistration
{
    internal static IServiceCollection AddApplicationDealStrategies(
        this IServiceCollection services)
    {
        services.AddScoped<FlatFeeDealTerms>();
        services.AddScoped<DoorSplitDealTerms>();
        services.AddScoped<VersusDealTerms>();
        services.AddScoped<VenueHireDealTerms>();

        services.AddScoped<
            IApplicationDealStrategyFactory<IDealTerms>,
            ApplicationDealStrategyFactory<
                IDealTerms,
                FlatFeeDealTerms,
                DoorSplitDealTerms,
                VersusDealTerms,
                VenueHireDealTerms>>();

        return services;
    }
}
```

The corresponding Deal call is:

```csharp
services.AddScoped<IDealMapper, DealMapper>();
services.AddScoped<IDealUpdater, DealUpdater>();
services.AddDealStrategies();
```

Its generated extension registers both families against closed constructions of the Deal factory. The
factory definition contains the contract and entity selectors. The analyzer also
requires the generated registration extension to be called exactly once from the owning module
composition root.

### 4.7 Consumer use

Consumers type only the short invariant factory and the family interface:

```csharp
internal sealed class DealTermsRenderer : IDealTermsRenderer
{
    private readonly IApplicationDealStrategyFactory<IDealTerms> strategies;

    public DealTermsRenderer(
        IApplicationDealStrategyFactory<IDealTerms> strategies)
    {
        this.strategies = strategies;
    }

    public string Render(Deal deal) =>
        strategies.Create(deal).Render(deal);
}
```

```csharp
internal sealed class DealMapper : IDealMapper
{
    private readonly IDealStrategyFactory<IDealMapper> strategies;

    public DealMapper(IDealStrategyFactory<IDealMapper> strategies)
    {
        this.strategies = strategies;
    }

    public Deal ToDeal(DealEntity entity) =>
        strategies.Create(entity).ToDeal(entity);

    public Result<DealEntity, ValidationErrors> ToEntity(Deal deal) =>
        strategies.Create(deal).ToEntity(deal);
}
```

This is the intended macro effect: the family call is written once; the selection switch is generated
once per input hierarchy and reused by every family.

## 5. Exact guarantees and limitations

| Stage | Guaranteed | Not guaranteed |
|---|---|---|
| C# compiler | Factory invariance; module-marker constraint; leaf implements family interface; C# 15 closed-hierarchy and native-union exhaustiveness | Semantic correctness of a leaf; built-in DI graph completeness |
| Generator/analyzer build errors | One implementation per visible family/case; no extra or missing cases; valid family use; convention-to-case mapping; generated registration invocation; Application contract/JSON/enum agreement; Deal contract/entity agreement | Cross-module EF/TypeScript agreement; runtime external/corrupt `IDeal`; constructor dependency lifetime correctness inferred from arbitrary code |
| DI startup | Constructor graph, scoped validation, and registration resolvability through `ValidateOnBuild` and `ValidateScopes` | Compile-time proof of arbitrary DI dependencies |
| Runtime/domain boundary | net10 unknown-case fallback; entity/source mismatch Result; persistence and wire validation | Elimination of the leaf's subtype narrowing |
| Architecture tests | Deal/entity/enum/JSON/TypeScript catalog agreement; no service provider, keyed lookup, dictionary, or handwritten Deal-selection switch in common-interface facades | Business semantics inside a correctly shaped implementation |

### Invariance

The factory is invariant because `TStrategy` has no variance modifier. This is genuine compiler
enforcement. The module marker separately prevents a family from another module being requested. The
generator/analyzer tightens the remaining hole by rejecting the marker itself or an undeclared marker
subinterface as `TStrategy`.

### Exhaustiveness

On C# 15, the generated factory's switch is compiler-exhaustive over `closed Deal`; the build promotes its
non-exhaustiveness warning to an error. The generator supplies the different guarantee that every
declared family has a leaf for every Deal case before it emits registrations. Both are required.

On net10, C# cannot close `IDeal`. The generator/analyzer therefore provides best-effort build-time
catalog and family coverage, while the generated switch retains an explicit fallback. The plan must
not describe net10 as natively exhaustive.

### Casting

Yes, the leaf still narrows the base `Deal`/`IDeal` value to its concrete case. Selecting a component
and invoking a base-typed common interface cannot make the runtime subtype statically known. The total
factory proves which leaf is selected for the runtime case, but C# has no associated/existential type that
changes the parameter type of the common interface after selection.

The architecture removes repeated switches and manual registration casts; it does not claim to remove
the leaf's runtime narrowing. A source-generated typed adapter could move that cast into generated code,
but it would have to understand every family method signature and would materially enlarge the
generator. It is not part of the first implementation. There is no `IDealStrategyFor<TDeal>` or CRTP
base.

### Lifetimes and construction

The generated factory and leaves are scoped by default. Direct constructor injection means resolving a
family factory constructs all four case leaves for that scope. This is the unavoidable price of
simultaneously rejecting service location and lazy provider delegates.

The design therefore does not preserve selected-only construction. It is approved only for honest
common-interface families whose complete leaf set is safe to construct together. The three current
families are small local collaborators. An operation with materially different I/O graphs, expensive
construction, incompatible lifetimes, or heterogeneous capabilities does not get forced through this
factory; it is modelled as a union/match or another operation-owned boundary.

No singleton factory may capture a scoped dependency. A later explicit lifetime option is added only when
a real family needs it and the generator can validate it honestly; the initial implementation does not
infer lifetimes from constructors or reintroduce `IServiceProvider` to simulate laziness.

## 6. Operation disposition

| Operation | Classification | net10 | C# 15 / .NET 11 |
|---|---|---|---|
| Entity ↔ contract mapping | Honest common interface | Generated Deal factory; one contract selector and one entity selector; leaves retain common calls | Same; both total selectors switch exhaustively over closed hierarchies |
| Existing-entity update | Honest common interface with pair coherence | Generated Deal factory selected by source; leaf returns typed mismatch for wrong entity case | Same; source selection is exhaustive and mismatch remains a domain error |
| Terms render/serialize/fingerprint | Honest common two-method family | Generated Application factory; named renderer/serializer facades | Same with closed-Deal total selector |
| Checkout payment amount | Heterogeneous result | Dunet value plus exhaustive match | Native payment-amount union plus exhaustive match |
| Payer/payee direction | Closed key-to-data | One Concert-owned immutable direction fact | Same |
| Settlement gross | Concert-owned closed financial data | Delete dead resolver; calculate from immutable settlement terms | Closed value/native union where cases carry different data |
| Application acceptance | Fixed heterogeneous input | Exactly Dunet paid/simple `Accept` | Exactly native `Accept(PaidAccept, SimpleAccept)` |
| Booking confirmation | Heterogeneous accepted/financial facts | Dunet values and operation-owned match | Native unions and exhaustive match |
| Concert cancellation | No variation | Direct refund collaborator | Same |
| Concert completion | Heterogeneous effect/capability cases | Dunet operation values plus match; no common step factory | Native unions plus exhaustive match; no common step factory |
| Lifecycle states/triggers/outcomes | Closed module-owned values | Enum/value/Dunet according to case data | Native union only where cases carry different data |
| API/PDF/UI identity | Identity only | Stable `DealType` and `$type` | Same |

The generator is not a license to wrap every Deal-varying operation in a marker interface. A family is
eligible only when its public method set, parameter shapes, return shapes, and semantic operation are
the same for all cases. Different required parameters or results select unions even if an erased
interface could be fabricated.

## 7. Acceptance is fixed

Net10:

```csharp
[Union(EnableImplicitConversions = false)]
internal abstract partial record Accept
{
    internal partial record Paid(
        ApplicationEntity Application,
        string PaymentMethodId);

    internal partial record Simple(
        ApplicationEntity Application);
}
```

C# 15:

```csharp
internal sealed record PaidAccept(
    ApplicationEntity Application,
    string PaymentMethodId);

internal sealed record SimpleAccept(
    ApplicationEntity Application);

internal union Accept(PaidAccept, SimpleAccept);
```

This shape may not be reclassified. It is never a keyed/common-interface family, a four-Deal-case
switch, an enum, nullable request, generic context wrapper, or service hierarchy.

## 8. Deal representation

The C# 15 target remains:

```csharp
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(FlatFeeDeal), "flatFee")]
[JsonDerivedType(typeof(DoorSplitDeal), "doorSplit")]
[JsonDerivedType(typeof(VersusDeal), "versus")]
[JsonDerivedType(typeof(VenueHireDeal), "venueHire")]
public closed record class Deal
{
    public required int Id { get; init; }
    public required PaymentMethod PaymentMethod { get; init; }
    public abstract DealType DealType { get; }
}

public sealed record FlatFeeDeal : Deal
{
    public override DealType DealType => DealType.FlatFee;
    public required decimal Fee { get; init; }
}
```

The other cases remain sealed direct descendants. Explicit JSON mappings preserve the established
lowercase discriminator values. `DealEntity` becomes closed in its own assembly when that project
targets C# 15. The generator uses the real type hierarchies; `DealType` is not its runtime dispatch key.

`DealType` keeps explicit numeric values and remains legitimate for persistence, compact contracts,
filtering, display, logging, and diagnostics. Callers do not select common-interface behaviour from it.
Ordinary enums cannot be declared `closed` in C# 15. Exhaustiveness comes from switching on the closed
`Deal` and `DealEntity` class hierarchies, not from treating `DealType` as a closed enum.

One architecture test verifies agreement among Deal descendants, entity descendants, `DealType`, JSON
tokens, EF mappings, and the exported TypeScript discriminated union.

## 9. Fifth-case experience

Adding a fifth Deal requires:

1. one sealed direct descendant of `closed Deal` and one `DealEntity` descendant;
2. one explicit `DealType` value and `$type` token;
3. one convention-matching implementation for every declared common-interface family;
4. new semantics in every affected native/Dunet union or closed data value;
5. EF and TypeScript case updates.

Failure stages are exact:

- net10 generator/analyzer: build error for Deal/entity/enum/JSON catalog drift and for the missing
  mapper, updater, or terms case;
- C# 15 compiler: non-exhaustive generated closed-hierarchy switch, promoted from `CS8509` to error;
- C# 15 generator/analyzer: family-case error until the fifth leaf exists and can be registered;
- native/Dunet unions: exhaustive operation match fails when a genuinely new union case is introduced;
- architecture tests: persistence, JSON, TypeScript, and identity-only drift;
- runtime: only corrupt/unnamed boundary values and semantic entity/source mismatch paths.

Once the fifth leaf is added for each common family, the generator expands the internal generic factory
and registrations automatically. No facade, consumer, composition root, or test writes another
five-case selection switch.

## 10. Delivery phases

Use remote-first validation from `@docs/REMOTE_VALIDATION.md`. Do not run local E2E unless a remote
failure requires targeted diagnosis.

### Phase 0 — generator proof before product migration

- Add the B2B-local incremental generator/analyzer project as an analyzer-only reference in each
  participating Application/Infrastructure project pair, without a runtime cross-service dependency.
- Prove the Infrastructure-local anchor, referenced Application symbol discovery through the real
  `InternalsVisibleTo` boundary, case convention, module constraint, factory-use diagnostics, scoped
  net10 catalogs, generated `.g.cs` factories, and generated registration extensions in generator tests.
- Compile representative Application module fixtures that can see only Deal Contracts and Deal module
  fixtures that can also see Deal Domain. Prove that Application emits only the contract selector, Deal
  emits contract and entity selectors, and neither widens the module graph; reject a single-compilation
  fixture as proof of layer feasibility.
- Add negative compile fixtures for missing fifth case, wrong-module family, factory marker misuse,
  missing registration invocation, and non-invariant assignment.
- Prove the dedicated heterogeneous-factory path separately: a module-local Infrastructure union over
  concrete implementations, deliberate many-Deal-to-one-operation aliases, exact known-case coverage,
  union membership diagnostics, typed constructor injection, and generated registration output.
- Measure generated output and diagnostics before migrating a production family.

Gate: no production family changes; every claimed compiler/generator guarantee has an executable
positive or negative compile test, and no handwritten source contains the five-type closed generic.

### Phase 1 — net10 Deal foundation cut-over

- Add the Deal marker and invariant `IDealStrategyFactory<TStrategy>`.
- Migrate Deal-owned `IDealMapper` and `IDealUpdater` to generated total-known-catalog factories and
  closed family registrations.
- Preserve named mapper and updater facades.
- Remove built-in keyed registrations, `IKeyedServiceProvider`, duplicated builders/factories, and
  per-family coverage declarations for the migrated Deal families.
- Make updater entity/source incoherence a typed operation failure rather than `InvalidCastException`.
- Prove scoped construction, `ValidateOnBuild`, `ValidateScopes`, and all-case behaviour.
- Deliver the foundation PR, its review and CI, package publication, and platform sync to terminal green;
  then update the lifecycle ledger and surface its suspended continuation.

Gate: current `main` contains the reviewed generator/analyzer and Deal mapper/updater cut-over; only
generated code performs their selection, and the lifecycle ledger records the exact foundation commit
that permits PR #633 to resume.

### Phase 2 — lifecycle consumption and net10 operation correction

- Resume lifecycle PR #633 from current `main` and add Application's marker plus
  `IApplicationDealStrategyFactory<TStrategy>` through the delivered generator.
- Migrate Application `IDealTerms` to the generated common-interface factory.
- Replace provisional generic step resolvers with generated dedicated acceptance, confirmation, and
  completion factories plus Dunet implementation unions wherever the landed APIs prove heterogeneous
  invocations. Do not route their union outputs through the common-interface strategy contract.
- Delete cancellation selection and call the refund collaborator directly.
- Replace payer/payee services with one owned direction value.
- Delete the dead settlement resolver and replace nullable `DealType` interpretation with immutable
  Concert-owned settlement terms.
- Stabilize explicit `DealType` values, conversions, persistence constraints, and wire tokens.

Gate: lifecycle PR #633 is terminal on `main`; no fake-uniform operation input/result survives; direct
and data cases are absent from DI; all current cases and mismatch paths have focused coverage; and this
ledger has reconciled the delivered module catalog.

### Phase 3 — C# 15 native unions and closed Deal

Dependencies: the .NET 11 compiler/runtime/consumer matrix is green and Phase 2 is delivered.

- Follow the breaking published-package cut-over workflow.
- Replace approved internal Dunet values with native unions without changing their semantic cases.
- Replace `IDeal` with `closed Deal`; close `DealEntity`; retain explicit JSON discriminator mappings.
- Change factory inputs from `IDeal` to `Deal`, regenerate factories without fallback arms, and make
  `CS8509` an error in affected projects.
- Run the deliberate fifth-case negative compile suite against the real target SDK.
- Publish the producer package, migrate every consumer, and carry platform sync to terminal green.

Gate: current payloads round-trip unchanged, all consumers build from the published package, and the
fifth-case fixture fails every intended compiler/generator/architecture surface.

### Phase 4 — enforcement and guidance

- Add architecture checks banning direct keyed Deal resolution, service-provider lookup, per-facade
  Deal dictionaries, and handwritten selection switches in declared common-interface facades.
- Enforce generator registration invocation and the Deal/entity/enum/JSON/TypeScript catalog.
- Update `api/agents/CODE_PATTERNS.md` and the deployed keyed-strategy standard on a separate `Docs/*`
  branch after the implementation proves the pattern.
- Run code review for implementation PRs and docs review for guidance.

Gate: implementation, review, PR, package, platform-sync, and guidance gates are terminal; then delete
this plan and ledger in a docs close-out PR.

## 11. Verification matrix

| Concern | Required evidence |
|---|---|
| Generator correctness | Snapshot/compile tests for emitted exhaustive factories, registrations, family discovery, and every error diagnostic |
| Invariance | Negative compile test assigning two different closed factory types |
| Exhaustiveness | net10 catalog negative fixture; C# 15 fifth-case fixture with `CS8509` treated as error |
| Registrations | Generated extension snapshot plus `ValidateOnBuild`/`ValidateScopes` resolution tests |
| Lifetimes | One scope constructs one instance of every leaf; a second scope gets new instances; no singleton captures scoped state |
| Common families | Focused all-case mapper, updater, terms-render, and terms-serialization tests |
| Heterogeneous operations | Exhaustive Dunet/native-union tests for acceptance, confirmation, and completion |
| JSON | Golden payload round trips with unchanged `$type` tokens and property names |
| Persistence | EF model/migration tests for TPT, explicit identity values, and closed settlement facts |
| Boundaries | Architecture tests reject service location, keyed Deal lookup, dictionaries, manual family switches, and cross-module factory use |
| Delivery | Exact-head draft-PR CI, published consumer build, and platform sync terminal green |

## 12. Sources

- [C# `closed` reference](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/closed)
- [C# closed-hierarchy specification](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/closed-hierarchies)
- [C# switch expression exhaustiveness](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/switch-expression)
- [C# native union proposal](https://github.com/dotnet/csharplang/blob/main/proposals/unions.md)
- [System.Text.Json polymorphism](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism)
- [.NET compiler platform SDK](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/)
- [Dependency injection lifetimes](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/service-lifetimes)
