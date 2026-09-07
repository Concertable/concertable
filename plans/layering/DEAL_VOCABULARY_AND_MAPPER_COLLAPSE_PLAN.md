# Deal vocabulary split and DI-mapper collapse plan

## Outcome

Two coupled corrections, in order:

1. **Layering** — a `*.Domain` project must not reference a `*.Contracts` project. Deal is the proving ground:
   its two shared enums move to a B2B-local vocabulary project, and `Deal.Domain` loses its `ProjectReference`
   to `Deal.Contracts`. The domain then cannot see a wire DTO, and the compiler enforces it.
2. **Mappers** — the keyed-DI "mapper" families that carry no dependencies and no behaviour collapse into
   static, total, dependency-free mappers plus one honestly-named factory. Roughly 24 files become 4.

They are sequenced together because the layering fix decides where DTO→entity dispatch is allowed to live.
With the reference gone, that dispatch is unambiguously Application's, and the mapper reduces to the one
direction that is genuinely a mapping.

## Current baseline

### The layering defect

`Concertable.B2B.Deal.Domain.csproj:10` references `Concertable.B2B.Deal.Contracts`. Deal.Domain uses exactly
two things from it — `DealType` and `PaymentMethod`. It uses no DTO. But the reference is assembly-wide, so
`DealDto` (with its `[JsonPolymorphic]` attributes), `IDealModule`, `CreateDealError` and `UpdateDealError` sit
on the domain's reference graph as collateral.

`Deal.Contracts` is therefore two projects wearing one name: shared **vocabulary** (the enums, which Domain
legitimately needs) and **wire contracts** (which Domain must never see).

12 Domain projects across B2B, Messaging and Payment carry the same reference. The Customer service mostly does
not — `Customer.Ticket.Domain`, `Customer.Review.Domain` and `Customer.Venue.Domain` reference only
`Concertable.Kernel`. That is the target shape and it already exists in-repo.

`Deal.Contracts` is **not** a published package (B2B `Directory.Build.props:39` defaults `IsPackable` false and
Deal.Contracts does not opt in), so no enum duplication is required — there is no independent-versioning
argument for a domain `PaymentMethod` plus a `PaymentMethodDto`. One enum both sides can see is sufficient and
strictly simpler.

### The mapper defect

Five families named "mapper" carry no collaborators and exist only to re-dispatch on a key:

| Family | Files | Per-arm body |
|---|---|---|
| `IDealMapper` (Deal.Application) | 6 | cast, project 2–4 properties |
| `IDealUpdater` (Deal.Infrastructure) | 5 | cast, call `entity.Update(primitives)` |
| `IPaymentAmountMapper` (Concert.Application) | 6 | cast, `new FlatPayment(c.Fee)` |
| `ITransactionMapper` (Payment.Application) | 5 | cast, project ~8 properties |
| `IUserMapper` (User.Infrastructure) | 2 | no dependencies, fake-async `Task.FromResult` |

`ITransactionMapper` does not even use DI — `TransactionMapper.cs:9` is a `FrozenDictionary` of hand-`new`'d
instances, so it pays the ceremony and receives none of the benefit. Two further facades sit over the single
`IDealTerms` family — `DealTermsRenderer` and `DealTermsSerializer` — both the same
`factory.Create(deal.DealType).X(deal)` shape.

### The construct-and-discard validator

`DealService.Validate` (`DealService.cs:42`) answers "is this deal valid?" by calling `mapper.ToEntity(deal)`,
discarding the constructed entity, and keeping only the errors. It is load-bearing: `OpportunityService.cs:183`
calls it through `DealModule.Validate` before creating an opportunity.

That is why `IDealMapper.ToEntity` returns `Result<DealEntity, ValidationErrors>` at all — construction (which
validates, via `FlatFeeDealEntity.Create`) was routed through something named "mapper", so the mapper inherited
a failure mode. A mapper is a total function; the moment it can fail it is not one.

## Design decisions

**A mapper is pure, total and dependency-free.** `entity → DTO` qualifies. `DTO → entity` can fail, so it is
construction, not mapping, and it lives in `DealFactory`, named for what it does.

**The entity keeps primitive-taking methods.** `FlatFeeDealEntity.Create(decimal, PaymentMethod)` and
`.Update(decimal, PaymentMethod)` already have the right shape. DTO unpacking is Application's job. This holds
under either layering outcome, so Phase 2 is not hostage to Phase 1.

**`Apply` as an abstract member on `DealEntity` is rejected.** It would give compiler-enforced exhaustiveness
and delete the dispatch entirely — but it requires `DealEntity` to accept a `DealDto`, which is precisely the
reference Phase 1 removes. Correct layering wins over the nicer dispatch.

**Capability matching is not applicable.** `IConcertWorkflowCapabilityRegistry.Has<TCapability>(dealType)` is a
predicate for gating HATEOAS links without instantiating a workflow. It answers yes/no; it does not dispatch to
an implementation. `Modules/Concert/AGENTS.md` warns explicitly against inventing a marker for a question the
type system already answers.

**Mapperly is adopted per file, not blanket.** It earns its keep where many members map by name —
`ITransactionMapper` (~8 properties × 3 arms). For `DealDto` (3–4 properties × 4 arms) a hand-written switch is
comparable in size and adds no dependency. `Riok.Mapperly` is already pinned at 4.3.1 (unused) on three
in-flight branches; that collision is resolved here.

**`MemberVisibility.All` is used nowhere in this plan.** Mapping into private setters via `UnsafeAccessor`
bypasses the validating factory exactly as thoroughly as a public setter would, so it is not a DDD-preserving
option. It is also broken across assemblies on 4.3.1 (riok/mapperly#1458, fixed by #2139 merged 2026-02-04, not
in a stable release). The `entity → DTO` direction needs none of it: entity getters are public, DTO setters are
`init`.

## Phases

### Phase 1 — B2B vocabulary project, Deal layering fix

- Add `api/Concertable.B2B/src/Concertable.B2B.Vocabulary` (`net10.0`). Service-local: no cross-folder escape,
  so the standalone carve is unaffected and nothing needs publishing.
- Move `DealType` and `PaymentMethod` out of `Deal.Contracts/Enums/` into it; update every consuming `using`.
- `Deal.Domain`: drop the `ProjectReference` to `Deal.Contracts`, add one to `Vocabulary`.
- `Deal.Contracts`: add a `ProjectReference` to `Vocabulary`.
- Add an architecture test asserting no `*.Domain` project references a `*.Contracts` project, with the
  remaining 11 modules on an explicit allowlist the test also asserts is still accurate.

**Consumption contract:** `Concertable.B2B.Vocabulary` is the home for B2B enums that both a domain and a
contract need. It holds no DTOs, no interfaces and no behaviour. Consumers reference it by `ProjectReference`
from inside `api/Concertable.B2B/`; nothing outside that folder may reference it.

**Gate:** `api/Concertable.slnx` builds; `DealStrategyArchitectureTests` green; the new layering test green with
Deal absent from the allowlist. Build/integration verification inherits `docs/REMOTE_VALIDATION.md`.

### Phase 2 — Deal mapper and updater collapse

- `DealMappers` — static, C# 14 extension members, `entity → DTO` only. Mapperly `[MapDerivedType]` over the
  four arms, plain `[Mapper]`, no visibility overrides.
- `DealFactory` — `Create(DealDto) → Result<DealEntity, ValidationErrors>` switching to
  `XDealEntity.Create(primitives)`; `Apply(DealEntity, DealDto) → UnitResult<ValidationErrors>` as a tuple
  pattern switch to `entity.Update(primitives)`, whose default arm absorbs the deal-type mismatch guard
  currently hand-written in `DealUpdater.Apply`.
- Delete `IDealMapper`, `DealMapper` and the four arms; `IDealUpdater`, `DealUpdater` and the four arms.
- `DealService.Validate` becomes `DealFactory.Create(deal).ToUnit()`. Named honestly it is still
  construct-and-discard; lifting the per-arm guards out of the entities is explicitly **not** in scope.
- Remove `RequireAll<IDealMapper>()`, `RequireAll<IDealUpdater>()` and both keyed registration blocks.
- Rewrite the `DealStrategyArchitectureTests` rows and `DealStrategyFactoryTests` that name deleted types.
- Add an exhaustiveness test asserting both switches carry an arm per `DealType`.

**Consumption contract:** `DealService` and `DealModule` keep their current signatures; no caller outside the
Deal module changes. `IDealService.Validate` continues to return `UnitResult<ValidationErrors>`, so
`OpportunityService.cs:183` is untouched.

**Gate:** build; `Concertable.B2B.Deal.UnitTests`; B2B integration suite.

**Known regression accepted:** adding a `DealType` member now fails a test rather than composition.
`DealStrategyArchitectureTests.DealDtoEntityEnumJsonAndTypeScriptCatalogs_Agree` already fails independently on
a new member, so the uncovered gap is narrow — an arm present everywhere but missing from a switch — and the
new exhaustiveness test closes it.

### Phase 3 — Concert payment-amount and terms families

- `IPaymentAmountMapper` (6 files) collapses to a static mapper on the same rules.
- `IDealTerms` keeps its keyed family only where an arm genuinely needs collaborators. `DealTermsRenderer` and
  `DealTermsSerializer` are two facades over one family and at least one is a pure per-arm computation. Commit
  `c587e73d4` (on a later branch) already retired `IDealTermsRenderer` on exactly this reasoning — align with
  it rather than diverging.
- Drop the corresponding `RequireAll` rows.

**Gate:** build; Concert unit and integration suites.

### Phase 4 — Payment and User

- `ITransactionMapper` → one static Mapperly mapper. Widest arms in the codebase; this is where Mapperly
  clearly wins. Requires the `Riok.Mapperly` pin in `api/Concertable.Payment/Directory.Packages.props`.
- `IUserMapper` → deleted outright. No dependencies, no Mapperly needed, and the `Task.FromResult` wrapper goes
  with it.

**Gate:** build; `Concertable.Payment.UnitTests` (`TransactionMapperTests` rewritten against the static
mapper); Payment integration suite.

## Explicitly out of scope

- **The remaining 11 `*.Domain → *.Contracts` references.** Same template, own item; doing all twelve up front
  turns a contained change into a multi-week one.
- **Moving vocabulary into `Concertable.Contracts`.** It is a published package, so it is a breaking
  published-contract change requiring its own expand/contract plan and multiple merges.
- **Moving `fee > 0` and the other guards to the API boundary.** Only needed for a bidirectional DTO↔entity
  mapper, which this plan rejects. Keeping them removes the `OpportunityService` / `DealModule.Validate`
  cross-module change from the blast radius entirely.
- **Renaming the two genuine-DI `ApplicationMapper`s.** Both are correctly DI'd and misnamed; own item.

## Completion conditions

- No B2B `*.Domain` project references a `*.Contracts` project except the 11 on the declared allowlist.
- `Deal.Domain` cannot reference `DealDto`; the build proves it.
- `IDealMapper`, `IDealUpdater`, `IPaymentAmountMapper`, `ITransactionMapper` and `IUserMapper` no longer exist
  as DI-registered families.
- Every surviving mapper is static, total and dependency-free.
- Exhaustiveness over `DealType` is asserted by test everywhere `RequireAll` previously guaranteed it.
