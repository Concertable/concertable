# Code Patterns

Recurring design patterns this codebase commits to. When a change fits one of these shapes, use the
pattern — don't invent a local variant. Sibling of [`CODE_CONVENTIONS.md`](./CODE_CONVENTIONS.md)
(naming/style); this file is about *structure*.

## Tenancy is composed, never subtracted

Visibility comes from **what a context is built from**, not from disabling rules after the fact.
Per-query `IgnoreQueryFilters` calls are banned — "add a global rule, then remove it for half the
callers" hides the stance at every call site and is unauditable. This is **compiler-enforced**:
`IgnoreQueryFilters` is a banned API (`BannedApiAnalyzers`, `RS0030` = error — see `api/BannedSymbols.txt`),
so the codebase has zero calls; the building blocks (all in `B2B.DataAccess.Infrastructure`):

- **The module's `XConfigurationProvider` is the anemic core** — pure table mappings, zero tenancy.
  Both stances below compose it; neither modifies it. Both stances are **per module** — a cross-module
  "sees everything" context would break module isolation (every module reads only its own model).
- **`VenueArtistTenantScopedDbContext`** (abstract, `B2B.DataAccess.Infrastructure`) — the tenant-filtered
  stance. Ctor-injects the module's configuration provider + `ITenantContext` (it implements
  `IHasTenantContext`); its sealed `OnModelCreating` composes the anemic core first, then the module's
  filter declarations via the abstract `ApplyTenantFilters` hook
  (`modelBuilder.ApplyVenueArtist<TEntity>(this)` per entity). Filters are declared per entity, never
  auto-derived from the `IVenueArtistTenantScoped` marker: marked ≠ filtered is a per-entity product
  decision (Concert carries the pair but stays public). Example: `ConcertDbContext`.
- **`TenantScopedDbContext`** (abstract, same seam) — the single-owner counterpart to the above: same
  shape, but `ApplyTenantFilters` declares per-entity single-owner filters
  (`modelBuilder.ApplySingleOwner<TEntity>(this)`, `TenantId == current`). Examples:
  `VenueDbContext` (filters `Venue`/`VenueImage`) and `ArtistDbContext`.
- **`XReadDbContext`** (one concrete read context per split module) — the tenant-independent read stance. It derives
  the shared DataAccess `ReadDbContext`, which composes the module's configuration provider with no
  tenancy on top. It is read-only by construction — `SaveChanges` throws — so the write-side
  `TenantInterceptor` guard can never be bypassed through it. Example: `ConcertReadDbContext`.
- **`UnscopedDbContext`** (abstract, same seam) — the unscoped-but-writable stance: composes the provider
  with no tenancy, but **writable** (unlike `XReadDbContext`), so a cross-tenant operator can act on rows
  it doesn't own; the `TenantInterceptor` write-guard no-ops for a tenant-less write. One subclass per
  module that has an admin write flow, e.g. `VenueAdminDbContext` (venue approval).

Query classes then split by **data-access stance**, one stance per class (mixing them in one class is
the LSP violation — callers can't know which contract a method honors):

- **`XRepository`** — the tenant-bound `XDbContext`, including whichever tenant filters that
  entity declares. The default.
- **`XReadRepository`** — read-only access through the module's tenant-independent `XReadDbContext`.
  Its contract controls which data leaves the module.
- **`XAdminRepository`** — privileged cross-tenant read/write (e.g. admin approval) on the writable
  `UnscopedDbContext`. Only where an admin write flow exists, e.g. `VenueAdminRepository`.
- **Domain facts that aren't naturally entity repositories** may get their own purpose-named
  abstraction on the module's `XReadDbContext`, e.g. `IConcertAvailability`, when they form a real,
  independently consumed capability. Do not wrap a single query already owned by an aggregate
  repository in a one-method interface; keep that query on the repository.

The injection site is then self-documenting: a service holding `repository` + `readRepository`
(the codebase convention when a service injects both stances of its own aggregate) states exactly
which queries see what.

**A stance class only exists when the entity has more than one stance.** A single-stance entity is a
plain `XRepository` — don't pre-qualify it `Public*`/`Admin*` with no sibling to disambiguate from;
rename it the day a second stance is actually born. The qualifier carries *which* contract; with
nothing to contrast, it's noise.

**Filter an entity only when its *reads* are tenant-private.** The marker (`ITenantScoped` /
`IVenueArtistTenantScoped`) means "carries the owner id," not "is filtered." If the entity's core flow
reads it *across* tenants, leave it unfiltered and let `TenantInterceptor` guard the writes — filtering
it fails those cross-tenant reads closed. Unfiltered by design today: **Opportunity** (the artist's
apply reads the venue's opportunity to stamp the deal), **Deal** (an applying artist reads the
venue's terms), **Concert** (public listing). Filtered: **Venue**, **Artist** (owner-private reads,
with browse split off to the public stance).

## Repository naming — independent dimensions

Repository qualifiers describe the contract that differs from a service's unqualified default; they
are not one vocabulary to impose across every service:

- **B2B data-access stance:** `XRepository` uses the normal tenant-bound `XDbContext`, `XReadRepository`
  uses the tenant-independent/read-only `XReadDbContext`, and `XAdminRepository` uses
  unfiltered/writable `UnscopedDbContext`. Purpose-named internal lookups and facts may share
  `XReadDbContext`. Name the
  composed contract, never the filtering mechanism: do not substitute `Unscoped` or `CrossTenant`.
- **Mutability:** the shared `Repository<...>` surface permits writes; `ReadRepository<...>` exposes
  queries only. Customer's event-synced replicas therefore use `XReadRepository` even without a
  writable sibling — `Read` states a capability, not an audience.
- **Projection shape:** Search's `XHeaderRepository` and `XAutocompleteRepository` names describe the
  projection they serve, not visibility or write capability.

These dimensions are independent. A B2B `XReadRepository` is both unfiltered and read-only because
of its context; a Customer `XReadRepository` is read-only over a context with no tenant filtering.
Keep the ordinary owned/scoped/writable repository unqualified. Audience belongs at the API contract,
not in persistence type names.

## One repository per entity — don't fold a satellite entity into another entity's repository

A repository's generic base (`Repository<TEntity>`) binds it to exactly one entity; give every entity
its own repository even when several live in the same module and DbContext, and even when one is
queried far more than another. Concert (`ApplicationRepository`, `BookingRepository`,
`ConcertRepository`, `ContractRepository`, `InvoiceRepository`, `OpportunityRepository`) and
Conversations (`MessageRepository`, `ContentReportRepository`) are the reference shape: six and two
sibling entities in one module, six and two repositories.

The tell that a repository has drifted into this anti-pattern: its interface mixes queries for two or
more unrelated entity types (e.g. `IUserRepository` returning both `UserEntity` and admin-invitation
rows), or it hand-writes a `GetXByIdAsync`/`AddX` pair that only re-implements what the generic base
already gives the *wrong* entity type bound as `TEntity`. Split it — one interface, one repository, one
entity — even if the caller (a single service) ends up injecting two repositories instead of one; that
is the service's job, not a reason to merge the persistence contracts.

`ITenantRepository` (Tenant module) is a known, pre-existing violation — logged in that module's
`TECH_DEBT.md`, not a pattern to copy.

## Module-local keyed strategy factory

**When behavior varies by a closed key** (typically `DealType`), declare every strategy family
vertically at the owning module's composition root. A module-local generic factory owns keyed DI;
operation-specific facades delegate through it. Consumers never branch on the key, see the registration
mechanism, or inject the generic factory merely to perform a business operation.

The factory's noun names what it returns. `DealType` is only the selection key:

```csharp
internal interface IDealStrategyFactory<TStrategy>
    where TStrategy : class
{
    TStrategy Create(DealType dealType);
}

internal sealed class DealStrategyFactory<TStrategy> : IDealStrategyFactory<TStrategy>
    where TStrategy : class
{
    private readonly IKeyedServiceProvider services;

    public DealStrategyFactory(IKeyedServiceProvider services)
    {
        this.services = services;
    }

    public TStrategy Create(DealType dealType) =>
        services.GetRequiredKeyedService<TStrategy>(dealType);
}
```

The builder records registrations before mutating `IServiceCollection`, then rejects duplicate keys,
undeclared or incomplete coverage, unexpected keys, and conflicting lifetimes. Every family declares
`RequireAll<T>()` or `RequireExactly<T>(...)`; adding an enum member therefore fails composition until
the new type is handled deliberately.

```csharp
services.AddDealStrategies(strategies =>
{
    strategies.For(DealType.FlatFee)
        .AddSingleton<IDealMapper, FlatFeeDealMapper>()
        .AddSingleton<IDealUpdater, FlatFeeDealUpdater>();

    // The other deal types follow in the same vertical block.

    strategies.RequireAll<IDealMapper>();
    strategies.RequireAll<IDealUpdater>();
});
```

Rules of the shape:

- **Factories return a selected component.** A resolver consumes a selected strategy and returns the
  final domain answer. Mappers, renderers, serializers, and calculators keep naming the operation they
  perform; sharing a selection mechanism never flattens those suffixes into `Strategy`.
- **Factories and builders are module-local.** Deal and Concert own separate implementations because
  their strategies are different runtime concerns. Do not create a cross-module registry or put the
  factory in a shared Contracts package.
- **Only the factory implementation performs keyed lookup.** Composition roots may register the scoped
  `IKeyedServiceProvider` adapter; application handlers, steps, services, and named facades never inject
  it or call `GetRequiredKeyedService`.
- **The factory is scoped.** A selected leaf may depend on scoped repositories or clients. Stateless
  leaves may remain singleton, but any unkeyed facade that captures the factory must also be scoped.
- **Named facades remain the business API.** `DealMapper`, `DealUpdater`, `DealTermsRenderer`, and
  `SettlementAmountResolver` implement their operation-specific interfaces and delegate selection to
  the module factory. `IConcertWorkflowFactory` remains a named factory because its caller genuinely
  needs the selected workflow instance.
- Methods return existing domain types or scalars. Do not mint a one-use DTO or return an enum that
  every caller must reinterpret; add a second operation-specific method when a caller needs another
  value.

### The anti-patterns this replaces — never do these

- **Branching on the key in agnostic components.** A `DealType == VenueHire ? … : …` ternary (or
  switch) inside a handler/service/mapper that is otherwise contract-agnostic plants a business rule
  where nobody will look for it, and it WILL get copy-pasted (that's how it spreads). The rule lives
  in exactly one resolver.
- **Service location outside the factory.** `IKeyedServiceProvider` or
  `GetRequiredKeyedService<T>(key)` in a handler, step, service, or named facade leaks the dispatch
  mechanism into business code.
- **Parallel hand-written maps.** A `FrozenDictionary<DealType, ...>` per facade duplicates coverage
  declarations and lets one family drift when a new deal type is added. Declare all families in the
  validated vertical builder instead.
- **Enum + switch as an API.** Returning an enum that every caller must re-interpret with its own
  switch just multiplies the branch across the codebase. Return the resolved *value*, not a label.
- **Throwaway result records.** A `record Xyz(Guid A, Guid B)` created only to carry one resolver's
  return values is noise — prefer separate methods or an existing entity/read model.
- **Discard-tuple calls.** `var (thing, _) = await GetPairAsync(...)` means the API is the wrong
  shape for the caller — add the single-value method to the interface instead.

## Dependency injection

- **Our services:** register the interface and implementation in the owning composition-root extension;
  use constructor injection. Don't use `IServiceProvider` or factory lambdas for an ordinary dependency graph.
- **Third-party SDKs:** use the vendor's DI extension or root client when available; otherwise directly
  register only consumed service types. Keep them behind Infrastructure adapters — never rebuild the
  vendor's service graph with `IServiceProvider` factory lambdas.

## Dependency-holders — public get-only auto-properties, not mirrored fields

When a type's whole job is to **surface its injected dependencies as public members** of an interface
it implements — it holds them, adds no behaviour of its own — assign the constructor params straight to
**public get-only auto-properties**. Don't declare a private backing field and then mirror it with an
expression-bodied property; `private readonly IX x;` + `public IX X => x;` is two members and a pointless
double-hop for one dependency.

Canonical example — the per-`DealType` `IConcertWorkflow` implementations
(`Modules/Concert/…/Services/Workflow/Workflows/`), which exist only to expose each workflow step
(`Apply`, `Accept`, `Book`, `Finish`, `Cancel`) as a public property:

```csharp
internal sealed class FlatFeeWorkflow : IConcertWorkflow, IAppliesSimple, IAcceptsCheckout, IAcceptsSimple
{
    public FlatFeeWorkflow(
        SimpleApplyStep apply,
        CaptureEscrowAcceptStep accept,
        CreateConcertDraftStep book,
        ReleaseEscrowFinishStep finish,
        RefundEscrowStep cancel)
    {
        this.Apply = apply;      // concrete param (what DI resolves) → interface-typed property
        this.Accept = accept;
        this.Book = book;
        this.Finish = finish;
        this.Cancel = cancel;
    }

    public ISimpleApplyStep Apply { get; }
    public ISimpleAcceptStep Accept { get; }
    public IBookStep Book { get; }
    public IFinishStep Finish { get; }
    public ICancelStep Cancel { get; }
}
```

The params stay **concrete** (so DI resolves the registered concrete step) while the properties are
**interface-typed** (the contract consumers see); the implicit concrete→interface conversion happens at
assignment. Assignments are `this.`-qualified like any constructor assignment (see
[`CODE_CONVENTIONS.md`](./CODE_CONVENTIONS.md)).

This is **not** a licence to drop `private readonly` fields everywhere. It applies only when the member
is a genuine public part of the type's contract that just passes the dependency through. A dependency a
type consumes *internally* (a repository a service calls, a client a step invokes) stays a
`private readonly` field — that's captured state, not a surfaced member, and the "no primary
constructors for captured state" rule in `CODE_CONVENTIONS.md` still governs it.

## Typed HTTP clients — Refit, not hand-rolled `HttpClient`

Every outbound HTTP call we *consume* gets a Refit interface — a `[Get]`/`[Post]`-annotated contract
registered with `AddRefitClient<T>()`, base address + any auth handler attached at registration. Don't
hand-roll `IHttpClientFactory.CreateClient()` + `PostAsync` + manual `JsonDocument` parsing when a
typed interface expresses the same call. *Which* protocol a hop uses is decided first in
[`MICROSERVICE_COMMUNICATION.md`](./MICROSERVICE_COMMUNICATION.md) (gRPC for our-own internal sync;
HTTP only at the forced boundaries — browser, third party, OAuth spec); this is the structural shape
*once that table has chosen HTTP*.

Current Refit clients, one interface per remote contract:

- **`IGoogleGeocodingApi`** — third-party REST (Google geocoding). External, we don't own the shape.
- **`IUserClaimsApi`** — the internal `/internal/users/{sub}/claims` hop. This is the transition-window
  exception to "our-own-internal is gRPC" — it stays Refit until that service has a gRPC surface.
- **`ITokenApi`** (`Concertable.Kernel.Auth`) — the OAuth2 `/connect/token` client-credentials POST
  behind `ClientCredentialsTokenService`. Form-encoded body via `[Body(BodySerializationMethod.UrlEncoded)]`,
  response shape pinned with `[JsonPropertyName]` (`access_token`/`expires_in`); the authority is the
  base address, set per host in `AddClientCredentials`. The token *cache* (scope-keyed
  `ConcurrentDictionary` + double-checked `SemaphoreSlim` so a stampede collapses to one fetch + an
  `expires_in − 30s` margin) lives in the service; Refit owns only the wire call underneath it.

`Concertable.Kernel` carries the `Refit.HttpClientFactory` package for `ITokenApi` — fine: Refit is a
small, already-in-repo library, and a typed contract is worth a package reference. (The "shared is the
intersection" rule in `api/CLAUDE.md` is about not bolting audience-specific *concepts* onto shared
*types* — it does not forbid a shared utility package.)

**One caveat specific to `ITokenApi`:** `ClientCredentialsTokenService` is a **singleton** (it owns the
shared token cache), so the Refit client it injects is a captive dependency — one `HttpMessageHandler`
pinned for the app's lifetime, no factory handler rotation. Accepted here: the authority is a stable
internal endpoint hit infrequently. Don't copy that singleton-captures-Refit shape onto a hot or
DNS-volatile client — those stay scoped/transient so the factory rotates handlers normally.

### The anti-patterns this replaces — never do these

- **Hand-rolled `HttpClient` + manual JSON/`JsonDocument` parsing** for a call a Refit interface could
  express. The typed contract is the readable source of truth; reach for raw `HttpClient` only when
  Refit genuinely can't model the call.
- **Refit against our own internal HTTP.** If both ends are ours it's gRPC (`AddGrpcClient<T>`) —
  Refit there means two contract surfaces for one service. The only standing exception is a service
  that doesn't have its gRPC surface yet (`IUserClaimsApi`).

## Unit of work — which one

Choose by the number of `SaveChanges` calls and `DbContext`s involved:

- **`IUnitOfWork<T>.SaveChangesAsync()`** — the default for one context and one flush. Stage every
  entity change, then save once; EF commits that save atomically (e.g. Payment's ledger: mutate the
  escrow/settlement, stage the ledger rows, then save `PaymentDbContext` once).
- **`IUnitOfWork<T>.ExecuteAsync(block)`** — one context when the operation genuinely needs several
  `SaveChanges` calls or requires its reads and writes to share one explicit transaction.
- **`IUnitOfWorkBehavior<T>.ExecuteAsync(block)`** — cross-module only. Wraps the block in an ambient
  `TransactionScope` so writes to several modules' contexts in one service all enlist in one transaction
  (e.g. `OpportunityService.CreateAsync`: `DealDbContext` + `ConcertDbContext` together). A single-context
  transaction can't span them.

Never share a transaction across **services** — a separate service owns its own database; coordinate those
with messages (outbox), never a unit of work.
