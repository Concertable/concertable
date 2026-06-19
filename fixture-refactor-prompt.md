# Refactor: move module-specific DB read-back off the shared `ApiFixture` into per-module fixture subclasses

## Goal

A B2B integration-test fixture (`ApiFixture`, xUnit + `WebApplicationFactory` + a SQL Testcontainer) is **shared by every module's** integration tests. It currently exposes **Concert-module** (and Payment-module) database read-back directly on the base, so *any* module's tests can read another module's rows. That's pollution and wrong layering. Move each module's read-back onto a **per-module fixture subclass** so the base stays generic and a test can only read back its **own** module's data.

This touches several files — **investigate and find them all**; don't assume the list below is complete.

## Background you need

- The shared fixture: `api/Concertable.B2B/Tests/Concertable.B2B.IntegrationTests.Fixtures/ApiFixture.cs`. It owns the generic infra: `WebApplicationFactory<Program>`, the SQL fixture, seeding, `ResetAsync()` (Respawn + re-seed + creates a DI `scope`), `CreateClient(...)`, mocks.
- Each module's integration-test project has its **own** collection definition, e.g.:
  ```csharp
  // api/Concertable.B2B/Modules/Concert/Tests/.../IntegrationCollection.cs
  [CollectionDefinition("Integration")]
  public sealed class IntegrationCollection : ICollectionFixture<ApiFixture>;
  ```
  Tests are `[Collection("Integration")]` and inject `ApiFixture` via constructor.
- A previous step **deleted** the old cross-module `IReadDbContext`/`ReadDbContext` (a catch-all unfiltered read context) — correct. But its replacement was bolted onto the **base** `ApiFixture` as per-DbSet read-back, which is the thing to fix now.

## Current (undesired) state — on the BASE `ApiFixture`

```csharp
private PublicConcertDbContext concertReads = null!;
...
public IQueryable<EscrowEntity> Escrows => paymentDbContext.Escrows.AsNoTracking();        // Payment-module read-back
public IQueryable<ApplicationEntity> Applications => concertReads.Set<ApplicationEntity>().AsNoTracking();  // Concert
public IQueryable<BookingEntity>     Bookings     => concertReads.Set<BookingEntity>().AsNoTracking();      // Concert
public IQueryable<ConcertEntity>     Concerts     => concertReads.Concerts.AsNoTracking();                 // Concert
...
// in ResetAsync():
concertReads     = scope.ServiceProvider.GetRequiredService<PublicConcertDbContext>();
paymentDbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
```

**Consumers found:** only **Concert** integration tests read `Applications`/`Bookings`/`Concerts` (the `Application*ApiTests`, `Concert*ApiTests`, `TenantScopingTests` under `Modules/Concert/Tests/...`). `Escrows` is Payment read-back with the same smell — audit its consumers too.

`PublicConcertDbContext` is the **unfiltered, read-only** "public stance" Concert context — used deliberately so assertions can read **all tenants'** rows (the tenant-filtered `ConcertDbContext` would hide cross-tenant rows). Keep using the unfiltered context for read-back.

## Desired design — inheritance

- **Base `ApiFixture`** stays generic: no `Applications`/`Bookings`/`Concerts`/`Escrows`, no `concertReads`/`paymentDbContext` module fields. Provide a **hook** for subclasses to resolve their own contexts during reset — e.g. a `protected virtual Task OnResetAsync(IServiceScope scope)` called at the end of `ResetAsync()`, or expose the created `IServiceScope`/`Services` so a subclass can resolve from it. (The base creates the scope in `ResetAsync`; the subclass needs access to it.)
- **`ConcertApiFixture : ApiFixture`** (place it in the Concert integration-test project, or the shared Fixtures project if other Concert-adjacent projects need it) resolves `PublicConcertDbContext` and exposes the read-back. The user dislikes the per-DbSet `IQueryable` properties — prefer exposing the **context** (e.g. a single `ConcertDbContext`/`ConcertReads` property returning the `PublicConcertDbContext`) and let tests do `.Set<ApplicationEntity>().AsNoTracking()`, **or** give the Concert fixture thin typed accessors. Pick the cleaner option and justify it. (Note: the property name shouldn't imply the filtered context — it's the unfiltered public read stance.)
- **Concert's `IntegrationCollection`** → `ICollectionFixture<ConcertApiFixture>`; Concert tests inject `ConcertApiFixture`.
- Decide whether **Payment's `Escrows`** gets the same treatment (a `PaymentApiFixture`, or fold into whichever module's tests use it) — audit consumers first.

## Constraints

- Read-back must stay on the **unfiltered** `PublicConcertDbContext` (cross-tenant assertions).
- Follow repo conventions in `api/docs/CODE_CONVENTIONS.md` (notably: **no primary constructors** for non-DbContext types — explicit ctor + `private readonly`; field naming).
- Other modules' fixtures/collections must keep compiling and pass unchanged.
- `dotnet build` green; if Docker is available, the Concert integration tests should still pass.

## What to deliver

1. The base-fixture cleanup + reset hook.
2. `ConcertApiFixture` (and `PaymentApiFixture` if warranted) exposing only that module's read-back.
3. Updated collection definitions + Concert (and Payment) test constructors.
4. A short list of every file changed and why.

Investigate first (find all consumers and collection definitions), propose the concrete shape, then implement.
