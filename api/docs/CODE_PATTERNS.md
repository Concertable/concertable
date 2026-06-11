# Code Patterns

Recurring design patterns this codebase commits to. When a change fits one of these shapes, use the
pattern — don't invent a local variant. Sibling of [`CODE_CONVENTIONS.md`](./CODE_CONVENTIONS.md)
(naming/style); this file is about *structure*.

## Tenancy is composed, never subtracted

Visibility comes from **what a context is built from**, not from disabling rules after the fact.
Per-query `IgnoreQueryFilters` calls are banned — "add a global rule, then remove it for half the
callers" hides the stance at every call site and is unauditable. The codebase has zero
`IgnoreQueryFilters` calls; the building blocks (all in `B2B.DataAccess.Infrastructure`):

- **The module's `XConfigurationProvider` is the anemic core** — pure table mappings, zero tenancy.
  Both stances below compose it; neither modifies it. Both stances are **per module** — a cross-module
  "sees everything" context would break module isolation (every module reads only its own model).
- **`VenueArtistTenantDbContext`** (abstract, `B2B.DataAccess.Infrastructure`) — the tenant-filtered
  stance. Ctor-injects the module's configuration provider + `ITenantContext` (it implements
  `IHasTenantContext`); its sealed `OnModelCreating` composes the anemic core first, then the module's
  filter declarations via the abstract `ApplyTenantFilters` hook
  (`modelBuilder.ApplyVenueArtist<TEntity>(this)` per entity). Filters are declared per entity, never
  auto-derived from the `IVenueArtistTenantScoped` marker: marked ≠ filtered is a per-entity product
  decision (Concert carries the pair but stays public). Example: `ConcertDbContext`.
- **`PublicDbContext`** (abstract, same seam) — the public stance. Composes the module's own
  configuration provider with no tenancy on top: public by construction, nothing is lifted because
  nothing was applied. Read-only by construction — `SaveChanges` throws — so the write-side
  `TenantInterceptor` guard can never be bypassed through it. One concrete subclass per module,
  e.g. `PublicConcertDbContext`.

Query classes then split by **visibility stance**, one stance per class (mixing them in one class is
the LSP violation — callers can't know which contract a method honors):

- **`XRepository`** — party/host reads on the module's filtered context. The default.
- **`PublicXRepository`** — the public marketplace surface (anonymous browse: details pages,
  listings) on `IPublicDbContext`. Never returns private contents. Examples:
  `PublicOpportunityRepository`, `PublicConcertRepository` (Concert module).
- **Cross-tenant *facts* that aren't browse** (e.g. "is this slot taken?") get their own named
  abstraction returning only booleans/scalars on `IPublicDbContext` — e.g. `IConcertAvailability` —
  so the name carries the why and nothing needs an apologetic comment.

The injection site is then self-documenting: a service holding `repository` + `publicRepository`
(the codebase convention when a service injects both stances of its own aggregate) states exactly
which queries see what.

## Keyed strategy resolver

**When a rule varies by a closed key** (typically `ContractType`): one facade class implements the
public interface, constructor-injects the concrete strategies, maps key → strategy in a
`FrozenDictionary`, and delegates. Consumers inject the interface and call it — they never branch on
the key, never see the map, never touch keyed DI.

Canonical example — `ContractMapper`
(`Modules/Contract/Concertable.B2B.Contract.Application/Mappers/ContractMapper.cs`):

```csharp
internal sealed class ContractMapper : IContractMapper
{
    private readonly FrozenDictionary<ContractType, IContractMapper> mappers;

    public ContractMapper(
        FlatFeeContractMapper flatFee,
        DoorSplitContractMapper doorSplit,
        VersusContractMapper versus,
        VenueHireContractMapper venueHire)
    {
        mappers = new Dictionary<ContractType, IContractMapper>
        {
            [ContractType.FlatFee] = flatFee,
            [ContractType.DoorSplit] = doorSplit,
            [ContractType.Versus] = versus,
            [ContractType.VenueHire] = venueHire,
        }.ToFrozenDictionary();
    }

    public IContract ToContract(ContractEntity entity) =>
        mappers[entity.ContractType].ToContract(entity);
}
```

Other instances: `PayeeResolver` (Concert module — which party receives a concert's ticket revenue).

Rules of the shape:

- The facade and the strategies implement the **same interface**; the facade is the only DI-default
  registration (`AddSingleton<IXResolver, XResolver>()`), strategies register as their concrete types.
- Strategies are injected as **concrete constructor parameters** — not `IServiceProvider`, not
  `GetRequiredKeyedService`, not `IEnumerable<IX>` scanning. The dictionary in the constructor IS the
  rule, written once, readable at a glance.
- Methods return **existing domain types or scalars** — don't mint a one-use DTO just to bundle a
  resolver's outputs; add a second method instead.
- An unmapped key throws (`KeyNotFoundException`) — a new enum member fails loudly rather than
  silently defaulting.

### The anti-patterns this replaces — never do these

- **Branching on the key in agnostic components.** A `ContractType == VenueHire ? … : …` ternary (or
  switch) inside a handler/service/mapper that is otherwise contract-agnostic plants a business rule
  where nobody will look for it, and it WILL get copy-pasted (that's how it spreads). The rule lives
  in exactly one resolver.
- **Service location at the consumer.** `GetRequiredKeyedService<T>(key)` in a handler or step leaks
  the dispatch mechanism into business code. Keyed/dynamic resolution, if ever needed, stays inside
  the facade next to the composition root.
- **Enum + switch as an API.** Returning an enum that every caller must re-interpret with its own
  switch just multiplies the branch across the codebase. Return the resolved *value*, not a label.
- **Throwaway result records.** A `record Xyz(Guid A, Guid B)` created only to carry one resolver's
  return values is noise — prefer separate methods or an existing entity/read model.
- **Discard-tuple calls.** `var (thing, _) = await GetPairAsync(...)` means the API is the wrong
  shape for the caller — add the single-value method to the interface instead.
