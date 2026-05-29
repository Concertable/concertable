# Finish the SeedData redesign on `Refactor/Microservices`

**Branch:** `Refactor/Microservices`. Most of the 403 work is done; see `memory/project_403_fix_state.md` for full state. The one thing left is **manager `User` rows missing from the DB during E2E** because the test suite respawns `[user].[Users]` between tests and nothing re-creates them. Fix that *and* clean up the `SeedData` design at the same time.

## End state we want

1. **`SeedData` is a singleton built entirely from compile-time-deterministic inputs in its parameterless constructor.** All properties are `{ get; }` — no `null!`, no `[]`, no setter, no later reassignment by anyone.
2. **`UserEventSeeder` is deleted** in both B2B and Customer E2E projects. `AuthDevSeeder` already publishes `CredentialRegisteredEvent`s for every seeded credential via its domain event → outbox → ASB pipeline; that's the single source of "managers exist in DB". E2E reuses dev's flow.
3. **`[user].[Users]`, `[user].[ArtistManagerProfiles]`, `[user].[VenueManagerProfiles]`, `[user].[AdminProfiles]`** go back into the `DbFixture`'s `TablesToIgnore` — User rows are reference data; tests don't mutate them, so don't wipe them. Same for the Customer side's `[user].[Users]`.
4. **Per-aggregate `XFactory.Seed` static** mirroring `api/Concertable.Auth/Data/Factories/CredentialFactory.cs` — one per entity that SeedData constructs.
5. **Identity formula promoted to the shared lib** (`Concertable.Seeding.Identity`) so Auth, B2B SeedData, and Customer SeedData all consume the same `SeedUsers` helpers.

## Why the redesign

Today's `SeedData`:

- Has ~60 properties all defaulted to `null!` or `[]`.
- Gets populated by `IDevSeeder.SeedAsync` calls that do `seedData.X = ...; context.X.Add(seedData.X)`.
- Doesn't work for manager users because they come via `CredentialRegisteredEvent` from `AuthDevSeeder`, not via a `SeedAsync` call — so `seedData.ArtistManager1` is never assigned anywhere, and tests NRE.

The fix is to recognise that **everything `SeedData` exposes is deterministic at compile time**. We pick the IDs, the emails, the role, the geometry, the relationships. Nothing depends on DB-generated state. So `SeedData` constructs every entity in its ctor; seeders shrink to "persist what `SeedData` already built"; the event flow (Auth → handler) writes the same rows the handler-side path would, with the same IDs, so the in-memory mirror and the DB row are consistent by construction.

## Promote `SeedUsers` to the shared lib

Create `api/Seeding/Concertable.Seeding.Identity/SeedUsers.cs`:

```csharp
namespace Concertable.Seeding.Identity;

public static class SeedUsers
{
    public const int ManagerCount = 35;

    public static Guid   ArtistManagerId(int n)    => new($"a1000000-0000-0000-0000-{n:D12}");
    public static string ArtistManagerEmail(int n) => $"artistmanager{n}@test.com";
    public static Guid   VenueManagerId(int n)     => new($"b1000000-0000-0000-0000-{n:D12}");
    public static string VenueManagerEmail(int n)  => $"venuemanager{n}@test.com";

    public static readonly Guid Admin = new("a0000000-0000-0000-0000-000000000001");
}
```

Same lib gets `SeedCustomers` (promote from `Concertable.Customer.Seeding/SeedCustomers.cs`).

Delete `api/Seeding/Concertable.B2B.Seeding/SeedUsers.cs`. Delete the `internal static class SeedIds` block at the top of `AuthDevSeeder.cs` and `UserDevSeeder.cs` — replace usages with `SeedUsers`.

If the `Concertable.Seeding.Identity` csproj doesn't exist yet, create it. Reference it from `Concertable.Auth.csproj`, `Concertable.B2B.Seeding.csproj`, `Concertable.Customer.Seeding.csproj`, and any module Infrastructure projects whose IDevSeeders/ITestSeeders reference `SeedIds`.

## Factory ID assignment — use `.With(...)` reflection helper

ID generation is disabled for seeding (via `.UseSeedingSupport(sp)` on the DbContext registration). Most entities have private `Id` setters and either domain-assigned IDs (`Guid.NewGuid()` inside the factory `Create`) or DB-identity IDs. To overwrite with a deterministic seed ID, use the existing reflection extension at `api/Seeding/Concertable.B2B.Seeding/Extensions/EntityReflectionExtensions.cs`:

```csharp
public static T With<T>(this T entity, string propertyName, object? value) where T : class
```

The canonical pattern is `CredentialFactory.Seed` (`api/Concertable.Auth/Data/Factories/CredentialFactory.cs:8-14`):

```csharp
public static CredentialEntity Seed(Guid id, string email, string passwordHash, string clientId)
{
    var credential = CredentialEntity.Create(email, passwordHash, clientId)
        .With(nameof(CredentialEntity.Id), id);
    credential.VerifyEmail();
    return credential;
}
```

Every new `XFactory.Seed` static follows this shape:

```csharp
// api/Concertable.B2B/Modules/Artist/Concertable.B2B.Artist.Domain/Factories/ArtistFactory.cs
using static Concertable.B2B.Seeding.Extensions.EntityReflectionExtensions;

public static class ArtistFactory
{
    public static ArtistEntity Seed(int id, Guid managerId, string name, /*…*/)
        => ArtistEntity.Create(managerId, name, /*…*/).With(nameof(ArtistEntity.Id), id);
}
```

The one exception is `UserFactory.Seed` — `UserEntity.FromRegistration(id, email, role)` already accepts an explicit `Guid` id, so `UserFactory` just delegates and doesn't need `.With(...)`. Same for the Customer-side `UserFactory`.

**If `EntityReflectionExtensions` namespace doesn't reach Domain projects from `Concertable.B2B.Seeding`,** move the extensions class up to a project Domain can reference (e.g., promote it to `Concertable.Seeding.Identity` alongside `SeedUsers`, or to a generic `Concertable.Seeding.Reflection`). Don't duplicate it.

## Create the entity factories

Mirror `CredentialFactory.cs` for every aggregate `SeedData` constructs:

**B2B side** (`api/Concertable.B2B/Modules/<X>/Concertable.B2B.<X>.Domain/Factories/<X>Factory.cs`):

- `UserFactory.Seed` (two overloads — basic, and with `Point/Address/avatar` for Admin)
- `ArtistFactory.Seed`
- `VenueFactory.Seed`
- `FlatFeeContractFactory.Seed`
- `VersusContractFactory.Seed`
- `DoorSplitContractFactory.Seed`
- `VenueHireContractFactory.Seed`
- `OpportunityFactory.Seed`
- `ApplicationFactory.Seed`
- `BookingFactory.Seed`

**Customer side** (`api/Concertable.Customer/Modules/<X>/Concertable.Customer.<X>.Domain/Factories/<X>Factory.cs`):

- `UserFactory.Seed` (Customer's `UserEntity.FromRegistration` is `(id, email)`, no role)
- Any other aggregates Customer's `SeedData` references

Each `Seed` static takes an explicit `id` plus the domain inputs the entity needs. Uses the existing `Create`/`Apply`/`Book` domain factory and chains `.With(nameof(X.Id), id)` if the domain factory doesn't accept an explicit Id.

`UserFactory` shape (this one is special — no `.With(...)` needed):

```csharp
public static class UserFactory
{
    public static UserEntity Seed(Guid id, string email, Role role) =>
        UserEntity.FromRegistration(id, email, role);

    public static UserEntity Seed(Guid id, string email, Role role, Point location, Address address, string avatar)
    {
        var user = UserEntity.FromRegistration(id, email, role);
        user.UpdateLocation(location, address);
        user.UpdateAvatar(avatar);
        return user;
    }
}
```

## Rewrite `Concertable.B2B.Seeding/SeedData.cs`

Parameterless ctor, all `{ get; }`, no `null!`, no `[]`. Named refs declared first; lists derived to include them at known slots:

```csharp
public sealed class SeedData
{
    public const string TestPassword = "Password11!";

    public UserEntity ArtistManager1        { get; }
    public UserEntity ArtistManagerNoArtist { get; }
    public UserEntity VenueManager1         { get; }
    public UserEntity VenueManager2         { get; }
    public UserEntity Admin                 { get; }

    public IReadOnlyList<UserEntity> ArtistManagers { get; }
    public IReadOnlyList<UserEntity> VenueManagers  { get; }
    public IReadOnlyList<UserEntity> Users          { get; }

    public ArtistEntity Artist { get; }
    public VenueEntity  Venue  { get; }
    public FlatFeeContractEntity FlatFeeAppContract { get; }
    public ApplicationEntity     ConfirmedApp       { get; }
    public BookingEntity         ConfirmedBooking   { get; }
    // … every other field that's `null!` today becomes { get; } and is assigned in the ctor

    public SeedData()
    {
        ArtistManager1 = UserFactory.Seed(
            SeedUsers.ArtistManagerId(1), SeedUsers.ArtistManagerEmail(1), Role.ArtistManager);
        ArtistManagerNoArtist = UserFactory.Seed(
            SeedUsers.ArtistManagerId(SeedUsers.ManagerCount),
            SeedUsers.ArtistManagerEmail(SeedUsers.ManagerCount),
            Role.ArtistManager);
        VenueManager1 = UserFactory.Seed(
            SeedUsers.VenueManagerId(1), SeedUsers.VenueManagerEmail(1), Role.VenueManager);
        VenueManager2 = UserFactory.Seed(
            SeedUsers.VenueManagerId(2), SeedUsers.VenueManagerEmail(2), Role.VenueManager);

        ArtistManagers =
        [
            ArtistManager1,
            .. Enumerable.Range(2, SeedUsers.ManagerCount - 2).Select(i => UserFactory.Seed(
                SeedUsers.ArtistManagerId(i), SeedUsers.ArtistManagerEmail(i), Role.ArtistManager)),
            ArtistManagerNoArtist,
        ];

        VenueManagers =
        [
            VenueManager1,
            VenueManager2,
            .. Enumerable.Range(3, SeedUsers.ManagerCount - 2).Select(i => UserFactory.Seed(
                SeedUsers.VenueManagerId(i), SeedUsers.VenueManagerEmail(i), Role.VenueManager)),
        ];

        Admin = UserFactory.Seed(SeedUsers.Admin, "admin@test.com", Role.Admin,
            new Point(-0.5, 51.0) { SRID = 4326 },
            new Address("Leicestershire", "Loughborough"),
            "avatar.jpg");

        Users = [Admin, .. ArtistManagers, .. VenueManagers];

        Artist = ArtistFactory.Seed(id: 1, managerId: ArtistManager1.Id, /*…*/);
        Venue  = VenueFactory .Seed(id: 1, managerId: VenueManager1.Id,  /*…*/);

        FlatFeeAppContract = FlatFeeContractFactory.Seed(/*id*/ 1, Artist.Id, Venue.Id, /*…*/);
        ConfirmedApp       = ApplicationFactory   .Seed(/*id*/ 1, Artist.Id, Venue.Id, FlatFeeAppContract.Id, /*…*/);
        ConfirmedBooking   = BookingFactory       .Seed(/*id*/ 1, ConfirmedApp, /*…*/);
        // … every other entity, in dependency order
    }
}
```

Notes:

- `Point` is constructed directly via `new Point(lon, lat) { SRID = 4326 }`; no `IGeometryProvider` injection needed, so the ctor stays parameterless.
- Field initializers in C# **cannot** reference other instance fields, so cross-aggregate refs (`Artist` needs `ArtistManager1.Id`) must live in the ctor body, not field initializers. Don't try to make these field initializers — it won't compile.
- Look at every `null!` and `[]` default in the existing file (~60 lines) and replace each with a `{ get; }` property assigned in the ctor.
- Use the existing entity factory `Create`/`Apply`/`Book` methods inside your new `XFactory.Seed` statics — don't bypass the domain.
- Named refs FIRST, then lists derived to slot them in by position. No `[^1]` "trust me" indexing.

## Same treatment for `Concertable.Customer.Seeding/SeedData.cs`

It's thinner (`Customer` and `CustomerIds`) but apply the same rules — `{ get; }`, ctor-built, no `null!`, no settable properties.

## Rewrite `AuthDevSeeder.cs`

Drop the internal `SeedIds`. Use `SeedUsers`:

```csharp
context.Credentials.Add(CredentialFactory.Seed(SeedUsers.Admin, "admin@test.com", passwordHash, ClientIds.Admin));

for (int i = 1; i <= SeedUsers.ManagerCount; i++)
    context.Credentials.Add(CredentialFactory.Seed(
        SeedUsers.ArtistManagerId(i), SeedUsers.ArtistManagerEmail(i), passwordHash, ClientIds.ArtistWeb));

for (int i = 1; i <= SeedUsers.ManagerCount; i++)
    context.Credentials.Add(CredentialFactory.Seed(
        SeedUsers.VenueManagerId(i), SeedUsers.VenueManagerEmail(i), passwordHash, ClientIds.VenueWeb));

foreach (var customer in SeedCustomers.All)
    context.Credentials.Add(CredentialFactory.Seed(
        customer.Id, customer.Email, passwordHash, ClientIds.CustomerWeb));
```

## Rewrite `UserDevSeeder.cs` (B2B)

Only needs to persist `seedData.Admin` (admins don't come from events). Drop the in-DB lookup for `seedData.Admin` — just use `seedData.Admin` directly:

```csharp
public async Task SeedAsync(CancellationToken ct)
{
    if (!await context.Users.AnyAsync(u => u.Id == seedData.Admin.Id, ct))
    {
        context.Users.Add(seedData.Admin);
        context.AdminProfiles.Add(new AdminProfileEntity(seedData.Admin.Id));
        await context.SaveChangesAsync(ct);
    }
}
```

No `seedData.Admin = ...` assignment, no `seedData.Users = await context.Users.ToListAsync(ct)` assignment — those properties are `{ get; }` now and pre-populated by SeedData's ctor.

## Rewrite `UserTestSeeder.cs` (B2B integration tests)

Pure persistence, no assignment:

```csharp
public async Task SeedAsync(CancellationToken ct)
{
    if (await context.Users.AnyAsync(ct)) return;

    context.Users.AddRange(seedData.Users);
    context.ArtistManagerProfiles.AddRange(
        seedData.ArtistManagers.Select(u => new ArtistManagerProfileEntity(u.Id)));
    context.VenueManagerProfiles.AddRange(
        seedData.VenueManagers.Select(u => new VenueManagerProfileEntity(u.Id)));
    context.AdminProfiles.Add(new AdminProfileEntity(seedData.Admin.Id));

    await context.SaveChangesAsync(ct);
}
```

## Rewrite the other B2B IDevSeeders

`ArtistDevSeeder`, `VenueDevSeeder`, `ContractDevSeeder`, `ConcertDevSeeder`, `ConversationsDevSeeder`, `BlobDevSeeder` — they all currently do `seedData.X = ...; context.X.Add(seedData.X)` style. Change them to **read** from SeedData and persist:

```csharp
public async Task SeedAsync(CancellationToken ct)
{
    if (await context.Artists.AnyAsync(ct)) return;
    context.Artists.AddRange(seedData.Artists);
    await context.SaveChangesAsync(ct);
}
```

`ConversationsDevSeeder` used `seedData.ArtistManagerIds[i]` — replace with `seedData.ArtistManagers[i].Id`. The `ArtistManagerIds` / `VenueManagerIds` / `ArtistManagerEmails` / `VenueManagerEmails` properties on SeedData go away entirely; callers project from the `ArtistManagers` / `VenueManagers` `IReadOnlyList<UserEntity>` instead.

## Delete `UserEventSeeder.cs` and unwire it

- Delete `api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests/UserEventSeeder.cs`.
- Delete `api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests/UserEventSeeder.cs`.
- In both `AppFixture.cs`, remove the `await new UserEventSeeder(host, Polling).SeedAsync();` calls from `InitializeAsync` and `ResetAsync`. Also remove the seeder host's `AddAzureServiceBusTransport` call if nothing else in the seeder host needs to publish via ASB (check first — if outbox still needs it, keep but with empty subscription registration).

## Restore Respawner ignore list

In `api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests/DbFixture.cs` and `api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests/DbFixture.cs`, add to `TablesToIgnore`:

- B2B: `[user].[Users]`, `[user].[ArtistManagerProfiles]`, `[user].[VenueManagerProfiles]`, `[user].[AdminProfiles]`
- Customer: `[user].[Users]`

Also check whether `Inbox` is respawned. If yes and Users is *not* respawned, the handler will believe re-published messages haven't been processed and try to re-insert User rows on every reset, blowing up on PK conflict. Two options:

- Ignore `Inbox` too (manager rows persist, inbox dedup persists, handler skips re-published messages cleanly).
- Or leave Inbox respawned and trust the handler's `if (await context.Users.AnyAsync(u => u.Id == e.UserId, ct)) return;` guard to catch the duplicate.

Pick whichever matches what `AuthDevSeeder` actually does — read both `DbFixture.cs` files first and decide.

## Update `SEEDING_CONVENTIONS.md`

The "SeedData ref assignment pattern" section needs rewriting. The new convention is:

> SeedData properties are `{ get; }` and constructed in SeedData's parameterless ctor using per-aggregate `XFactory.Seed` statics (which use `EntityReflectionExtensions.With(nameof(X.Id), id)` to assign deterministic IDs over the domain's `Create` method). No seeder ever assigns to a SeedData property. Seeders read from SeedData and persist via `context.X.Add(seedData.X)`.

Drop the "never load back from DB" example since it's no longer relevant — there's no assignment to discuss.

## Hard rules (from memory; do not violate)

- **No comments narrating fixes/changes** (`feedback_no_fix_comments`, `feedback_no_comments_in_infra_config`). Don't add `// new pattern` or `// reverted from X` comments.
- **No `Co-Authored-By: Claude` or `🤖 Generated with Claude Code` trailer** on commits (`feedback_no_claude_trailer`).
- **`is not null`** over `is { }` (`feedback_null_check_style`).
- **No primary constructors for services** (`feedback_no_primary_constructors`) — explicit ctor + `private readonly` fields. `SeedData` itself is a data holder and a parameterless ctor is fine.
- **Field naming `this.field` not `_field`** (`feedback_field_naming`).
- **No unnecessary braces on single-statement `if`/`else`** (`feedback_no_unnecessary_braces`).
- **Use `PowerShell` tool, not `Bash`, for `dotnet build` / `dotnet test`** (`feedback_use_powershell_tool_for_dotnet`). Solution file is `api/Concertable.slnx` (not `.sln`).
- **`UserClaimsController.cs`** already exists in both B2B and Customer (renamed from `InternalUserController` in the prior session) — leave it alone.
- **Show staged diff before committing** (`feedback_show_before_commit`) and wait for explicit approval.

## Order of work (to avoid mid-flight broken builds)

1. Create `Concertable.Seeding.Identity` lib + `SeedUsers.cs` + promoted `SeedCustomers.cs`, reference it from Auth, B2B.Seeding, Customer.Seeding, and any module Infrastructure projects that use `SeedIds`.
2. Decide where `EntityReflectionExtensions` lives so Domain projects can use it. Move if needed.
3. Create all `XFactory.Seed` statics (UserFactory, ArtistFactory, VenueFactory, all the contract factories, ApplicationFactory, BookingFactory, OpportunityFactory) — B2B side first, then Customer side.
4. Rewrite `B2B.Seeding/SeedData.cs` ctor.
5. Rewrite `Customer.Seeding/SeedData.cs` ctor.
6. Update every IDevSeeder + ITestSeeder to read-from-SeedData instead of assign-to-SeedData.
7. Update `AuthDevSeeder` to use `SeedUsers` + `SeedCustomers`.
8. Delete B2B `SeedUsers.cs` (now in the shared lib) and Customer `SeedCustomers.cs` (likewise).
9. Delete `UserEventSeeder.cs` in both E2E projects; remove calls in both `AppFixture`s.
10. Add tables back to `TablesToIgnore` in both `DbFixture`s.
11. Update `SEEDING_CONVENTIONS.md`.
12. Build: `dotnet build api/Concertable.slnx`. Fix any cross-project compile errors.
13. Show the user the staged diff before committing.

## Verification

After build is green, run the B2B UI E2E suite focused on **"Artist pays hire fee upfront to book venue"** via the `run-ui-e2e-tests` skill. That scenario was failing previously and exercises the manager auth path; if it passes, the SeedData redesign + ignore-list fix has resolved the underlying 403 issue. Also run the Customer E2E suite to confirm no regressions.

If the Reqnroll/Playwright scenario still fails on Stripe.js button-clickability, that's a separate in-progress issue tracked in `memory/project_venue_hire_e2e_debug.md` — diagnose only if the failure isn't auth-related.

## State of the working tree as of this prompt being written

The previous session reverted everything *except* the controller rename. Current state:

- `SeedData.cs` — back to original (`null!` / `[]` defaults)
- `SeedUsers.cs` — back to original (just the four ID/email helpers, no static lists)
- `CredentialRegisteredHandler.cs` — back to original (no `SeedData?` injection)
- `UserEventSeeder.cs` — back to original (publish + poll only) — will be deleted as part of this work
- `UserClaimsController.cs` (B2B + Customer) — new files, kept; `InternalUserController.cs` deletions kept

Start from that baseline.
