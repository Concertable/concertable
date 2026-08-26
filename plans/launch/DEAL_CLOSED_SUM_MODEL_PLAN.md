# Deal DTO and strategy-dispatch foundation

> **Next steps live in @plans/launch/DEAL_CLOSED_SUM_MODEL_PROGRESS.md -> `## Next Steps`.**

## 1. Decision

Deal transport, persistence, and strategy dispatch are separate concerns.

The module boundary uses an abstract `DealDto` record with four sealed direct cases:

- `FlatFeeDealDto`;
- `DoorSplitDealDto`;
- `VersusDealDto`;
- `VenueHireDealDto`.

The DTO hierarchy preserves the existing `$type` discriminator tokens and maps to the parallel
`DealEntity` persistence hierarchy. `IDeal` is deleted. The module, projects, assemblies, folders, and
namespaces remain singular `Concertable.B2B.Deal.*`.

On .NET 10, common-interface strategy families use the invariant module-owned
`IDealStrategyFactory<TStrategy>`. Deal Infrastructure implements that seam with Microsoft keyed DI:

- `IDealMapper` and `IDealUpdater` remain the family contracts;
- each family has one concrete implementation registered for each `DealType` key;
- one internal factory resolves a family by `DealDto.DealType` or `DealEntity.DealType`;
- Application and ordinary consumers depend only on the module factory or its mapper/updater facade;
- only Deal Infrastructure sees keyed registration and `IKeyedServiceProvider` APIs.

This is a temporary .NET 10 mechanism behind a stable module API. The hierarchy is not language-closed,
and built-in keyed DI does not prove the family-by-case Cartesian product. Focused catalog and
registration tests protect the known cases until the C# 15/.NET 11 implementation can supply real
compile-time closure.

## 2. Sequencing

### Stage 1: current .NET 10 PR

This PR delivers only the architectural foundation that Concertable can use today:

- replace the transport hierarchy with `DealDto` and sealed `*DealDto` cases;
- delete `IDeal` and migrate B2B consumers without aliases;
- preserve the parallel `DealEntity` hierarchy;
- preserve JSON polymorphism and discriminator compatibility;
- retain invariant `IDealStrategyFactory<TStrategy>` with `Create(DealDto)` and
  `Create(DealEntity)`;
- register mapper/updater implementations with Microsoft keyed DI by `DealType`;
- validate DTO/entity case mismatches before a concrete updater cast;
- align DTO cases, entity cases, `DealType`, JSON registrations, keyed registrations, and existing
  frontend `$type` tokens in focused tests;
- remove all production generator references, annotations, generation anchors, and generated
  registration dependencies.

The source-generator prototype is preserved separately on `Spike/net11-closed-dispatch`. It is research
input, not production infrastructure for this PR.

### Stage 2: separate public .NET 11 library

A later workstream designs a general-purpose public NuGet library for C# 15/.NET 11 consumers. It must
not expose Deal, B2B, `DealType`, mapper, updater, or other Concertable-specific concepts.

That workstream must first settle the semantic model and public API. In particular it must determine
whether ordinary keyed lookup, total closed-case dispatch, and closed-family service resolution are
one abstraction or distinct abstractions. It must investigate:

- collision and conceptual confusion with
  `Microsoft.Extensions.DependencyInjection.IKeyedServiceProvider`;
- whether `GetService` incorrectly implies optional or nullable resolution;
- whether a runtime `TKey` truthfully represents subtype or union-case dispatch;
- correct input and output variance;
- whether `Resolve`, `Create`, `Match`, `Dispatch`, or another operation is semantically accurate;
- whether consumers need a generic public interface or generated module-owned facades implementing a
  small compiler-visible library contract;
- a package split between compiler-visible abstractions/annotations, analyzer/source generator, and an
  optional package referencing both.

The product promise is based on real C# 15 closed hierarchies and native union semantics. The analyzer
binary may target a compiler-host-compatible TFM such as `netstandard2.0`; that does not lower the
minimum language/runtime contract of consuming projects. The library must use Roslyn symbol identity
and compiler-visible protocol types rather than scattered metadata-name or syntax-name matching.

For each declared service family and every closed input case, the library must prove:

- exactly one accessible, constructable implementation;
- diagnostics for missing implementations;
- diagnostics for duplicate implementations;
- an exhaustive generated dispatcher;
- matching generated DI registrations;
- no repeated consumer switches, concrete type lists, registration matrices, generic argument
  matrices, dictionaries, or keyed-service access.

Native unions are reserved for genuinely heterogeneous variant results or handlers. The concept is
called a `variant`, not an operation.

### Stage 3: later Concertable migration

After Concertable moves to .NET 11 and the public library is published:

- make `DealDto` and appropriate entity roots language-closed;
- use native unions for genuinely heterogeneous variants;
- replace only the keyed factory implementation and manual registrations with library-generated
  dispatch and registrations;
- preserve `IDealStrategyFactory<TStrategy>`, `IDealMapper`, `IDealUpdater`, and their consumers;
- retain the JSON protocol and persistence identities through the package cut-over;
- carry every producer, consumer, and platform-sync PR to terminal green.

## 3. Current .NET 10 architecture

### Contracts and persistence

`DealDto` is an abstract record and its four direct cases are sealed records. It is a real transport DTO
used at module/API boundaries. `DealEntity` remains the persistence root with separate concrete entity
cases. Shared enum identity is `DealType`.

The .NET 10 compiler does not prevent another assembly or later source change from adding a new
`DealDto` or `DealEntity` subtype. Tests therefore compare the known catalogs; they do not claim that the
language has closed either hierarchy.

### Stable strategy seam

The Application-owned seam is:

```csharp
internal interface IDealStrategy;

internal interface IDealStrategyFactory<TStrategy>
    where TStrategy : class, IDealStrategy
{
    TStrategy Create(DealDto deal);
    TStrategy Create(DealEntity entity);
}
```

The generic parameter is invariant because it declares neither `in` nor `out`. Family interfaces inherit
`IDealStrategy`. Facades call `strategies.Create(deal)` or `strategies.Create(entity)` and contain no
catalog switch.

### Temporary Infrastructure implementation

Deal Infrastructure owns the built-in DI details. The composition root registers each concrete mapper
and updater as a keyed singleton under its `DealType`. The internal open-generic factory receives
Microsoft's `IKeyedServiceProvider` and resolves `TStrategy` using the DTO or entity `DealType`.

No B2B-local generic keyed-provider abstraction is introduced. No second public dispatcher abstraction
is introduced. Application does not reference Microsoft keyed DI directly.

### Mismatch validation

Updating an existing entity from a DTO first compares their `DealType` values. A mismatch returns the
typed validation failure before dispatch reaches a concrete updater and before any concrete DTO cast.

## 4. Guarantee boundary

The current implementation guarantees:

- an invariant, module-owned generic factory API;
- one selection mechanism shared by mapper and updater facades;
- keyed strategy lookup hidden inside Deal Infrastructure;
- the same selector shape for `DealDto` and `DealEntity`;
- typed DTO/entity mismatch failure;
- test-enforced agreement across the current DTO, entity, enum, JSON, registration, and frontend token
  catalogs;
- DI graph and lifetime validation for the known registrations.

It does not guarantee:

- language-level closure of `DealDto` or `DealEntity`;
- compiler-proven exhaustive subtype matching;
- compile-time proof of the full family-by-case Cartesian product;
- generator-proven missing, duplicate, accessible, or constructable implementations;
- native-union exhaustiveness;
- a reusable public dispatch API.

Those missing compile-time guarantees belong to Stages 2 and 3, not to an emulation layer in this PR.

## 5. Implementation phases

### Phase 1: preserve the prototype

- Commit the complete source-generator prototype, diagnostics, emitters, tests, B2B integration,
  generated-registration design, DealDto selectors, family coverage behavior, and incomplete attribute
  redesign on `Spike/net11-closed-dispatch`.
- Record the unresolved same-pass semantic attribute problem honestly.
- Return to `Refactor/deal-dispatch-foundation` without resetting or losing its dirty work.

Gate: the spike branch has a durable commit from which all prototype files can be recovered.

### Phase 2: deliver the .NET 10 foundation

- Complete the DTO rename and delete `IDeal`.
- Keep the stable invariant factory and both selector inputs.
- replace generated dispatch/registration with built-in keyed DI in Deal Infrastructure;
- retain mapper/updater leaves and typed mismatch behavior;
- remove generator projects and every B2B production reference or anchor;
- add catalog, JSON, registration, lifetime, and factory tests.

Gate: focused Deal and Concert tests, B2B build, architecture tests, invariant scans, diff check, and plan
graph are green without local E2E.

### Phase 3: review and delivery

- Review the committed feature branch.
- Resolve review findings and rerun affected gates.
- Create the feature PR only after review.
- Use remote CI as the authoritative validation and do not run E2E locally.
- After the current foundation is terminal on `main`, update the suspended lifecycle ledger.

Gate: the feature PR and any platform-sync follow-through are terminal green.

### Phase 4: plan the public library

- Start a separate plan and branch from current `main`.
- Use `Spike/net11-closed-dispatch` as evidence and inspiration, not as an API contract.
- Resolve the semantic/API/package questions in Stage 2 before publishing types.
- Prove the design against a real C# 15/.NET 11 consumer fixture and a non-Concertable example.

Gate: the public API is semantically settled, non-Deal-specific, and backed by compile-time negative
fixtures before package implementation begins.

## 6. Verification matrix

| Concern | Current PR evidence |
|---|---|
| DTO protocol | Four-case JSON round trips preserve `$type` tokens and concrete types |
| Catalog agreement | Reflection compares DTO/entity cases and `DealType`; attributes and frontend tokens match |
| Registrations | Each `IDealMapper`/`IDealUpdater` + `DealType` pair appears exactly once with the expected leaf |
| Factory | Both DTO and entity inputs resolve the expected family implementation |
| Lifetimes | Facades/factory are scoped; stateless keyed leaves are singletons |
| Mismatch | DTO/entity case mismatch returns typed failure without mutation |
| Boundaries | Keyed provider and lookup appear only in approved Infrastructure files |
| Consumers | No repeated strategy switch, dictionary, or direct keyed-provider lookup |
| Cleanup | No stale `IDeal`, generator annotation, anchor, analyzer reference, or generator project |
| Delivery | Focused local gates, remote CI, review, and platform-sync terminal state |
