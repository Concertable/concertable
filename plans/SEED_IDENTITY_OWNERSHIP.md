# Seed identity ownership — dissolve `Concertable.Seed.Identity`

**Branch:** `Refactor/Microservices`.

Replaces the deleted `SEED_DATA_REDESIGN.md`. That doc's *SeedData-as-composition-root* work is done and is **not** revisited here. This plan undoes the one thing that doc deliberately introduced: a neutral shared identity-data library.

## The problem

`api/Shared/Seed/Concertable.Seed.Identity` is a neutral, everyone-reaches-into-it data library. Two distinct concerns are conflated inside it:

1. **Identity catalog** — `SeedUsers` (artist/venue managers + admin) and `SeedCustomers` (customers): deterministic `Guid` + email tuples.
2. **`EntityReflectionExtensions.With()`** — a generic reflection helper (`api/Shared/Seed/Concertable.Seed.Identity/Extensions/EntityReflectionExtensions.cs`) used by Auth's `CredentialFactory` and **every** B2B/Customer seed factory to force deterministic IDs over domain `Create()` methods. This is generic seeding infra, not identity.

Two anti-patterns result:

- **Shared *data* across data services.** B2B and Customer both consume the same identity catalog from `api/Shared/`. That couples two services that must never depend on each other (`api/ARCHITECTURE.md` line 60: *"Why `Concertable.Customer.Seed` doesn't know B2B-owned IDs"*). Shared *ports* are fine; shared *data* is the smell.
- **Auth knows domain roles.** `AuthDevSeeder` enumerates `ArtistManager` / `VenueManager` / `Customer` and their cardinality. Production Auth is role-agnostic — `CredentialRegisteredEvent(UserId, Email, ClientId)` carries no role; consumers map `ClientId → role` in their own `CredentialRegisteredHandler`. The dev seeder is the one place that knowledge leaks back into Auth.

## The decision (Model A — Auth owns identity, keyed by `client_id`)

Auth is the identity producer: in production it mints the `Guid` (`CredentialEntity.Create → Guid.NewGuid()`) and emits `CredentialRegisteredEvent`; consumers project from it. The deterministic seed identities are therefore **Auth's producer seed contract**, exactly mirroring the existing `Concertable.B2B.Seed.Contracts` pattern (`api/ARCHITECTURE.md` lines 66–80), with one rule: the catalog is expressed in **Auth's own vocabulary — OAuth `client_id`, never domain roles.** The `client_id → role` mapping stays in the consumers, where it already lives.

Rejected alternative (Model B — each service owns its identities and registers through a new Auth seed-registration endpoint): inverts production (consumers would mint GUIDs and push them to Auth), needs new S2S plumbing + cross-service seed-time orchestration. More cost than the purity is worth.

### Why this is consistent with "adapters own no seed catalog"

`api/ARCHITECTURE.md` lines 89–95 say an *agnostic* adapter (Payment) owns no seed catalog, because purchase semantics live in its consumers. Auth is different: it is a **producer adapter** — it genuinely produces identity (mints GUIDs, emits the credential event that consumers project from). Owning an identity seed contract is the producer-seed-contract pattern, not a violation. The doc update below must draw this distinction explicitly (agnostic adapter = no catalog; producer adapter = owns its producer seed contract).

## End state

- New project `Concertable.Auth.Seed.Contracts` owns the deterministic identity catalog, keyed by `client_id`. Referenced by Auth (producer) and downward by every consumer.
- `EntityReflectionExtensions` lives in the existing neutral `Concertable.Seed.Shared`.
- `api/Shared/Seed/Concertable.Seed.Identity` is **deleted**.
- Auth contains zero domain-role vocabulary; consumers keep the `client_id → role` mapping.

## Step 1 — Move the reflection helper to `Concertable.Seed.Shared`

Generic infra, no identity coupling. Do this first so it's independent of the identity move.

- Move `EntityReflectionExtensions.cs` → `api/Shared/Seed/Concertable.Seed.Shared/`, namespace `Concertable.Seed.Shared` (or a `.Reflection` sub-namespace — pick one and be consistent).
- Update every `using static Concertable.Seed.Identity.Extensions.EntityReflectionExtensions;`:
  - `api/Concertable.Auth/Data/Factories/CredentialFactory.cs`
  - `api/Concertable.B2B/Seed/Concertable.B2B.Seed.Infrastructure/Factories/{Venue,Artist,Contract,Concert,Booking,Application,Opportunity}Factory.cs`
  - Any Customer-side seed factories with the same `using static` (grep `EntityReflectionExtensions` to confirm the full set).
- Ensure each consuming csproj references `Concertable.Seed.Shared` (most already do — it hosts `IDevSeeder`).
- Build green before continuing.

## Step 2 — Create `Concertable.Auth.Seed.Contracts`

Location: `api/Concertable.Auth/Seed/Concertable.Auth.Seed.Contracts/` (mirrors `api/Concertable.B2B/Seed/...`). References `Concertable.Auth.Contracts` only (for `ClientIds`). POCO project — no EF, no framework refs. Add to `api/Concertable.Auth/Concertable.Auth.slnx` and `api/Concertable.slnx`.

Port the content of `SeedUsers` + `SeedCustomers` into a catalog keyed by `client_id`. Identifiers must be role-free (Auth's vocabulary); email *strings* stay verbatim to avoid breaking dev login and existing E2E references.

```csharp
namespace Concertable.Auth.Seed.Contracts;

public sealed record SeedCredential(Guid Id, string Email);

public static class AuthSeedCatalog
{
    public const int ArtistWebCount = 35;
    public const int VenueWebCount  = 35;
    public const int CustomerWebCount = 3;

    public static readonly SeedCredential Admin =
        new(new("a0000000-0000-0000-0000-000000000001"), "admin@test.com");

    // Ordered; index n-1 == old SeedUsers.ArtistManagerId(n)
    public static IReadOnlyList<SeedCredential> For(string clientId) => clientId switch
    {
        ClientIds.Admin       => [Admin],
        ClientIds.ArtistWeb   => ArtistWeb,
        ClientIds.VenueWeb    => VenueWeb,
        ClientIds.CustomerWeb => CustomerWeb,
        _ => [],
    };

    public static int TotalCount => 1 + ArtistWebCount + VenueWebCount + CustomerWebCount;

    private static readonly IReadOnlyList<SeedCredential> ArtistWeb =
        Build(ArtistWebCount, n => new($"a1000000-0000-0000-0000-{n:D12}"), n => $"artistmanager{n}@test.com");
    private static readonly IReadOnlyList<SeedCredential> VenueWeb =
        Build(VenueWebCount,  n => new($"b1000000-0000-0000-0000-{n:D12}"), n => $"venuemanager{n}@test.com");
    private static readonly IReadOnlyList<SeedCredential> CustomerWeb =
        Build(CustomerWebCount, n => new($"c0000000-0000-0000-0000-{n:D12}"), n => $"customer{n}@test.com");

    private static IReadOnlyList<SeedCredential> Build(int count, Func<int, Guid> id, Func<int, string> email) =>
        [.. Enumerable.Range(1, count).Select(n => new SeedCredential(id(n), email(n)))];
}
```

GUID formulas are copied verbatim from the existing `SeedUsers`/`SeedCustomers` so every seeded ID is unchanged — no migration/data churn. Keep the `1-based n` formulas exactly.

## Step 3 — Repoint Auth

- `AuthDevSeeder.cs`: replace the `SeedUsers`/`SeedCustomers` enumeration with iteration over `AuthSeedCatalog.For(clientId)` per client. Still uses `CredentialFactory.Create(cred.Id, cred.Email, passwordHash, clientId)`. No role words appear.
- `Concertable.Auth.csproj`: drop the `Concertable.Seed.Identity` reference; add `Concertable.Auth.Seed.Contracts`. (Keep `Concertable.Seed.Shared` for the reflection helper.)
- Build Auth.

## Step 4 — Repoint B2B (consumer, downward ref)

B2B owns the `client_id → role` interpretation. Optionally add a thin B2B-side helper to preserve ergonomics, e.g. in `Concertable.B2B.Seed.Contracts`:

```csharp
internal static class B2BSeedIdentities
{
    public static SeedCredential ArtistManager(int n) => AuthSeedCatalog.For(ClientIds.ArtistWeb)[n - 1];
    public static SeedCredential VenueManager(int n)  => AuthSeedCatalog.For(ClientIds.VenueWeb)[n - 1];
    public static SeedCredential Admin               => AuthSeedCatalog.Admin;
    public static int ManagerCount => AuthSeedCatalog.ArtistWebCount; // == VenueWebCount
}
```

Repoint:
- `SeedCatalog.Artists.cs` / `SeedCatalog.Venues.cs`: `SeedUsers.ArtistManagerId(n)` → `B2BSeedIdentities.ArtistManager(n).Id` (likewise venue).
- `SeedState.cs`: build manager `UserEntity`s from `B2BSeedIdentities` (B2B still supplies `Role.ArtistManager` / `Role.VenueManager` / `Role.Admin` and admin's `Point`/`Address`/avatar).
- `UserHealthCheck.cs`: `SeedUsers.TotalCount` → `AuthSeedCatalog.TotalCount` (or `B2BSeedIdentities`-derived count if B2B should only assert its own users — decide which population the health check gates and use that count).
- csprojs: `Concertable.B2B.Seed.Contracts`, `Concertable.B2B.Seed.Infrastructure`, `Concertable.B2B.User.Infrastructure` — drop `Concertable.Seed.Identity`, add `Concertable.Auth.Seed.Contracts`.
- Build B2B.

## Step 5 — Repoint Customer (consumer, downward ref)

- `Customer SeedState.cs`: `SeedCustomers.CustomerId(n)` / `CustomerEmail(n)` → `AuthSeedCatalog.For(ClientIds.CustomerWeb)[n-1]` (a thin Customer-side helper analogous to B2B's is fine).
- `Concertable.Customer.Seed.Infrastructure.csproj`: drop `Concertable.Seed.Identity`, add `Concertable.Auth.Seed.Contracts`.
- Build Customer.

## Step 6 — Repoint Payment, Search tests, E2E hooks

All downward refs to `Concertable.Auth.Seed.Contracts`:
- `api/Concertable.Payment/Concertable.Payment.Infrastructure/Data/Seeders/PaymentTestSeeder.cs` (+ csproj).
- `api/Concertable.Search/Tests/Concertable.Search.IntegrationTests.Fixtures/{SeedState,ApiFixture}.cs` (+ csproj).
- `api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests.Ui/Hooks/StripeHooks.cs` (+ csproj).
- `api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests/Payments/TicketPurchaseTests.cs` (+ csproj).
- Build each.

## Step 7 — Prune stale references

These csprojs reference `Concertable.Seed.Identity` but have **no source usage** (verified — only stale binary artifacts). Remove the reference:
- `Concertable.B2B.Venue.Domain.csproj`
- `Concertable.B2B.Artist.Domain.csproj`
- `Concertable.Customer.User.Domain.csproj`

## Step 8 — Delete the shared lib

- Delete `api/Shared/Seed/Concertable.Seed.Identity/` (folder + project).
- Remove it from `api/Concertable.slnx` and any per-service `.slnx` that lists it.
- Confirm no remaining `Concertable.Seed.Identity` references anywhere (grep both `using` and `<ProjectReference`).

## Step 9 — Docs

- `api/ARCHITECTURE.md` "Producer seed libraries point downward only": add that the **identity producer adapter (Auth)** owns `Concertable.Auth.Seed.Contracts`, keyed by `client_id` and role-free, referenced downward by consumers. Distinguish it from the **agnostic adapter (Payment)**, which still owns no catalog. Keep the existing Payment paragraph intact.
- `api/docs/SEEDING_CONVENTIONS.md`: record that deterministic seed identities are owned by Auth's seed contract; consumers reference downward and never hardcode another service's identity GUIDs; the `client_id → role` mapping is consumer-owned.

## Step 10 — Build & verify

- `dotnet build api/Concertable.slnx` (use the **PowerShell** tool, single-line command).
- Integration suite first (fast): exercises `UserTestSeeder` persisting `SeedState.Users` and the handler-written rows — confirms every seeded ID still lines up across Auth credential / consumer user row / SeedState mirror.
- Then targeted E2E: B2B "Artist pays hire fee upfront to book venue" (manager auth path) and a Customer ticket-purchase scenario (customer auth path).

## Hard rules (from memory — do not violate)

- **No comments narrating fixes/changes**; **no comments in infra/DI/config**.
- **No `Co-Authored-By: Claude` / `🤖 Generated with Claude Code` trailer** on commits.
- Services/factories: explicit ctor + `private readonly` fields, `this.field` (no `_field`), no primary constructors. `AuthSeedCatalog` is a static data holder — fine as-is.
- `is not null` over `is { }`; no unnecessary braces on single-statement `if`/`else`.
- New per-service solution edits use `.slnx`; `Concertable.sln` is legacy.
- **PowerShell** tool for `dotnet build`/`dotnet test` (not Bash); solution is `api/Concertable.slnx`.
- **Show the staged diff and wait for explicit approval before committing.**

## Order of work (avoid mid-flight broken builds)

1. Move `EntityReflectionExtensions` → `Concertable.Seed.Shared`; fix `using static`s; build.
2. Create `Concertable.Auth.Seed.Contracts` + `AuthSeedCatalog`; add to solutions.
3. Repoint Auth; build Auth.
4. Repoint B2B (catalog, SeedState, health check, csprojs); build B2B.
5. Repoint Customer; build.
6. Repoint Payment / Search tests / E2E hooks; build.
7. Prune stale Domain refs.
8. Delete `Concertable.Seed.Identity`; remove from solutions; grep clean.
9. Update `ARCHITECTURE.md` + `SEEDING_CONVENTIONS.md`.
10. Full build; integration suite; targeted E2E.
11. Show diff; commit on approval.
