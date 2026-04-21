# Identity Module Extraction — Implementation Plan

## Why Identity Goes Before Artist

`ICurrentUser` is injected in 32+ files across the codebase. Every subsequent module (Artist, Venue, Concert) depends on it. If we extract Identity first:
- `ICurrentUser` lands in `Identity.Contracts` — every downstream module has a stable, clean reference
- `ArtistManagerEntity` + `VenueManagerEntity` move into Identity — when we extract Artist next, it references `Identity.Contracts` instead of `Concertable.Core`
- `Role` enum gets a canonical home in `Identity.Contracts`
- `OwnershipService` gets placed correctly before Artist/Venue extraction begins
- Auth logic (register, login, tokens, password flows) is coherent Identity domain logic — it all belongs together here

Without Identity first, the Artist module would still pull from `Concertable.Core` and `Concertable.Application` for user/auth concerns, defeating the modular boundary.

## What Auth means here

`Concertable.Auth` has been removed — auth is fully monolith. All auth + identity logic (registration, login, JWT tokens, email verification, password reset) lives together in `Concertable.Identity`. There is no separate IdentityServer to consider.

## Architecture

```
api/Modules/Identity/
  Concertable.Identity.Contracts/       ← public; Role enum, shared contracts
  Concertable.Identity.Domain/          ← internal; UserEntity hierarchy + token entities
  Concertable.Identity.Application/     ← internal; interfaces, DTOs, requests, validators, mappers
  Concertable.Identity.Infrastructure/  ← internal; implementations, repositories, DI
```

> **Note:** A 4th project (`Identity.Domain`) was added during implementation — domain entities
> belong in a Domain layer, not Application. The original plan said `Identity.Application/Domain/`;
> this was corrected mid-step.

## Architectural Decisions Made During Implementation

### `IBaseRepository<>` + `IGuidRepository<>` → `Concertable.Shared`
Originally in `Concertable.Application.Interfaces`. Moved to `Concertable.Shared` so that every
module's Application layer can extend them without depending on `Concertable.Application` (which
would create circular references). `IGuidEntity` was already in Shared — these belong there too.

### `PointMappers` → `Concertable.Shared`
`ToLatitude`/`ToLongitude` extension methods on NTS `Point`. Pure geometry utilities with no
application logic. Shared already has the NTS dependency, so this is the natural home.

### JWT bearer configuration stays in `Concertable.Web.AddAuth()`
`AddJwtBearer(...)` is middleware configuration — a host/web concern. `AddIdentityModule()`
in Identity.Infrastructure only registers service instances (token service, password hasher, etc.).
Web's `AddAuth()` calls `AddIdentityModule()` then configures the bearer scheme.

---

## Files to Move

### From `Concertable.Core/Entities` → `Identity.Application/Domain/`
- `UserEntity.cs` (contains `UserEntity`, `ManagerEntity`, `VenueManagerEntity`, `ArtistManagerEntity`, `CustomerEntity`, `AdminEntity` — TPH hierarchy, keep intact)
- `RefreshTokenEntity.cs`
- `EmailVerificationTokenEntity.cs`
- `PasswordResetTokenEntity.cs`

> **Note:** These entities must remain `public` (not `internal`) — other modules need them for EF navigation properties (e.g. Artist references `ArtistManagerEntity`).

### From `Concertable.Application/Interfaces` → `Identity.Application/Interfaces/`
- `IAuthService.cs`
- `ICurrentUser.cs` → also re-exported via `Identity.Contracts` (see below)
- `IUserService.cs`
- `IUserRepository.cs`
- `IOwnershipService.cs`
- `IUser.cs`
- `IUserMapper.cs`
- `IUserValidator.cs`
- `IUserPreferenceService.cs`

### From `Concertable.Application/Interfaces/Auth/` → `Identity.Application/Interfaces/Auth/`
- `IAuthUriService.cs`
- `IPasswordHasher.cs`
- `ITokenService.cs`
- `IUserLoader.cs`
- `IUserRegister.cs`

### From `Concertable.Infrastructure/Services/` → `Identity.Infrastructure/Services/`
- `AuthService.cs`
- `UserService.cs`
- `CurrentUser.cs`
- `CurrentUserAccessor.cs`
- `OwnershipService.cs`

### From `Concertable.Infrastructure/Services/Auth/` → `Identity.Infrastructure/Services/Auth/`
All 13 services:
- `AdminLoader.cs`, `AdminRegister.cs`
- `ArtistManagerLoader.cs`, `ArtistManagerRegister.cs`
- `CustomerLoader.cs`, `CustomerRegister.cs`
- `VenueManagerLoader.cs`, `VenueManagerRegister.cs`
- `AuthUriService.cs`
- `BCryptPasswordHasher.cs`
- `JwtTokenService.cs`
- `UserLoader.cs`
- `UserRegister.cs`

### From `Concertable.Infrastructure/Repositories/` → `Identity.Infrastructure/Repositories/`
- `UserRepository.cs`

### Web Controllers — stay in `Concertable.Web`
`AuthController`, `UserController`, `FakeAuthController` stay in Web — they will reference `IIdentityModule` (or auth service interfaces exposed through Contracts).

### EF Configuration
Any EF entity type configurations for User/token entities move to `Identity.Infrastructure/Data/`.
The Identity module gets its own `IdentityDbContext : ApplicationDbContext`.

## Identity.Contracts — What Is Public

```
Concertable.Identity.Contracts/
  ICurrentUser.cs          ← re-exported here; all modules reference this
  IUser.cs                 ← principal interface, needed by ICurrentUser
  Role.cs                  ← moved from Concertable.Core.Enums
  IIdentityModule.cs       ← facade for module-to-module calls (if needed)
```

`ICurrentUser` is claims-only — reads JWT claims via `IHttpContextAccessor`, no DB access:
```csharp
public interface ICurrentUser
{
    Guid? Id { get; }
    Role? Role { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}
```
Extension methods `GetId()` / `GetRole()` also live in Contracts and throw `UnauthorizedException` if null. `GetEntity()` / `Get()` / `GetOrDefault()` are **removed entirely**.

`IIdentityModule` is the cross-module facade for DB lookups other modules need:
```csharp
public interface IIdentityModule
{
    Task<UserDto?> GetUserAsync(Guid userId);       // Id, Email, Avatar
    Task<ManagerDto?> GetManagerAsync(Guid userId); // + StripeAccountId
    Task<CustomerDto?> GetCustomerAsync(Guid userId); // + StripeCustomerId
}
```
Not generic — concrete methods per return type so callers don't leak Identity's type hierarchy.

## `Role` enum migration

`Role` moves from `Concertable.Core.Enums` to `Identity.Contracts`. All references update. Since `Concertable.Core` stays in the solution (other entities still there), a type alias or a global using in Core pointing to the new location can smooth the transition — but prefer direct reference updates.

## `OwnershipService` — defer the cross-module fix

`OwnershipService` currently injects `IArtistService` + `IVenueService`. After Identity extraction it moves to `Identity.Infrastructure` still injecting those interfaces from `Concertable.Application`. **Do not fix this yet.** It will be resolved during Artist extraction (replaced with `IArtistModule`) and Venue extraction. The fix is documented here so it isn't forgotten.

## Integration Tests

Auth tests: `api/Tests/Concertable.Web.IntegrationTests/Controllers/Auth/AuthApiTests.cs`
- 17 tests covering register, login, send/verify email, forgot/reset/change password

**Run before starting:**
```
dotnet test --filter "FullyQualifiedName~AuthApiTests"
```
All 17 must pass. Re-run after each step.

Also run the full suite to catch regressions:
```
dotnet test
```
Baseline after Stage A: **236 tests passing** (18 Core + 3 Workers + 74 Infrastructure + 129 Integration + 12 E2E).

## Known Friction Points

### 1. `ICurrentUser` used in 32+ files across `Concertable.Infrastructure` and `Concertable.Application`
Once `ICurrentUser` moves to `Identity.Contracts`, every project that uses it must add a project reference to `Identity.Contracts`. For now this means `Concertable.Infrastructure` and `Concertable.Web` add the reference. Later, when Artist/Venue/Concert extract, those module projects also reference it.

### 2. `UserEntity` in EF `ApplicationDbContext`
EF configurations for User/token entities currently live in `Concertable.Infrastructure/Data/`. They move to `Identity.Infrastructure/Data/IdentityDbContext`. The shared `ApplicationDbContext` must stop configuring these entities once Identity owns them.

### 3. `Role` enum referenced everywhere
Moving `Role` from `Concertable.Core.Enums` to `Identity.Contracts` requires updating `using` statements across the solution. Most of these are in `Concertable.Web` (auth controller, middleware) and `Concertable.Infrastructure` (auth services).

### 4. `FakeAuthController` — test-only bypass
`FakeAuthController` allows integration tests to authenticate without real JWT. It references `UserEntity` and `ICurrentUser`. Once those move it still works — just add the `Identity.Contracts` reference to `Concertable.Web`.

---

## Suggested Extraction Order

1. ✅ **Run integration tests** — baseline: 209 tests passing
2. ✅ **Create project scaffolding** — 4 `.csproj` files: Contracts, Domain, Application, Infrastructure
3. ✅ **Move domain entities** — `UserEntity.cs` + token entities → `Identity.Domain/`. Cross-domain nav props removed (`Venue`, `Artist`, `Preference`, `Tickets`, `Messages`). EF configs for those relationships flipped to start from the owning side.
4. ✅ **Move `Role` enum** → `Identity.Contracts`. Global using added everywhere.
5. ✅ **Move application interfaces** — all `IAuth*`, `IUser*`, `ICurrentUser`, `IOwnershipService`, DTOs, Requests, Validators, Mappers → `Identity.Application/`
6. ✅ **Move infrastructure implementations** — all services, repos → `Identity.Infrastructure/`. `AddIdentityServices()` registered in Identity.Infrastructure; JWT bearer setup kept in Web. Test count: **236 tests passing**.

---

## Stage B — Prerequisite: Denormalize Profile Fields onto Artist/Venue

**Why this is required before `IdentityDbContext` can be properly wired:**

`ArtistEntity` and `VenueEntity` currently have no own `Location`, `Avatar`, `Address`, or `Email`
fields. They borrow all of these via the `User` navigation property. When `UserRepository` moves
to `IdentityDbContext`, the `UserEntity` is tracked in a separate EF context from
`ArtistEntity`/`VenueEntity` (which are in `ApplicationDbContext`). EF cannot fix up cross-context
navigation properties — `artist.User` becomes null after save, causing `ArtistMappers.ToDto` to
throw.

**Domain event infrastructure is already scaffolded in `Concertable.Shared`** (`IDomainEvent`,
`IEventRaiser`, `IDomainEventDispatcher`, `EventRaiser`) but no concrete events or handlers exist
yet. The denormalization introduces the first use of this pattern.

**Fields to add to `ArtistEntity` and `VenueEntity`:**
- `Location` (NTS `Point?`) — for geo-search, currently via `User.Location`
- `Address` (`Address` value object — County, Town) — currently via `User.Address`
- `Avatar` (`string?`) — currently via `User.Avatar`
- `Email` (`string`) — currently via `User.Email`; denormalize now, sync via domain event later

**Changes required:**

### B-pre Step 1: Add fields to entities + EF migration
- Add `Location`, `Address` (owned VO), `Avatar`, `Email` to `ArtistEntity` and `VenueEntity`
- Update `LocationExpression` on both entities: `a.User.Location` → `a.Location`
- EF migration — new nullable columns; seed data must populate them

### B-pre Step 2: Update ArtistService and VenueService
- `CreateAsync`/`UpdateAsync`: write directly to `artist.Location`, `artist.Address`, `artist.Avatar`
- Replace `userService.UpdateLocationAsync(user, lat, lng)` with direct geocoding + set on entity
- Remove `userRepository.Update(user)` calls — no longer saving user data from these services
- Remove `IUserRepository` and `IUserService` injections from `ArtistService` and `VenueService`

### B-pre Step 3: Update mappers
- `ArtistMappers.ToDto` / `ToSummaryDto`: `artist.User.Avatar` → `artist.Avatar`, etc.
- `VenueMappers.ToDto` / `ToSummaryDto`: same
- `LocationExpression` in both entities already handled by Step 1

### B-pre Step 4: Domain event for future sync
- When Identity later updates `UserEntity.Avatar` or `UserEntity.Email`, it should raise a domain event
  (e.g. `UserAvatarUpdated`, `UserEmailChanged`) that Artist/Venue modules handle to sync their copy
- **Defer the handler implementation** — define the event shape in Shared when needed, implement
  handlers in Artist/Venue modules during their extraction
- For now: Artist/Venue fields are the source of truth for their own profile data

### B-pre Step 5: Run tests — target 236 still passing

---

## Stage B Steps (Revised)

**DbContext infrastructure prerequisite (completed this session):**
- Created `Concertable.Data.Application` — `IReadDbContext` with typed `IQueryable<T>` for every
  entity across all modules. No EF dependency. References `Concertable.Core`.
- Created `Concertable.Data.Infrastructure` — `DbContextBase` (abstract, domain event dispatch +
  audit on `SaveChangesAsync`), `ReadDbContext : DbContextBase` (implements `IReadDbContext`,
  no-tracking, throws on save), `AuditInterceptor`, `DomainEventDispatcher`. These were moved out
  of `Concertable.Infrastructure`.
- `ApplicationDbContext` updated to inherit `DbContextBase` (removed duplicate `SaveChangesAsync`).
- Both projects added to the solution under the `api` solution folder.

7. ✅ **Create `IdentityDbContext`** — `IdentityDbContext : DbContextBase` (not `ApplicationDbContext`
   — that anti-pattern is now fixed). `OnModelCreating` configures only Identity-owned entities
   (User hierarchy, RefreshToken, EmailVerificationToken, PasswordResetToken). Registered in
   `AddIdentityModule()` with `AuditInterceptor`.

8. ✅ **Switch `UserRepository` to `IdentityDbContext`** — `UserRepository` is now self-contained
   (no longer inherits `GuidRepository<ApplicationDbContext>`). Injects `IdentityDbContext` for all
   owned operations. Still injects `ApplicationDbContext` **transitionally** for the four
   cross-module queries (`GetByApplicationIdAsync`, `GetByConcertIdAsync`,
   `GetIdByApplicationIdAsync`, `GetIdByConcertIdAsync`) — these are flagged for removal once
   steps 10b/10c are complete and `IReadDbContext` queries replace them.

9. ✅ **Create `IIdentityModule` facade** — complete. Collisions resolved by keeping only what's needed:
   - `ManagerDto` in Contracts: `Id`, `Email`, `Avatar`, `StripeAccountId`
   - `IIdentityModule` exposes only `GetManagerAsync(Guid)` — `GetUserAsync`/`GetCustomerAsync` dropped
     because `UserDto`/`CustomerDto` names clashed with existing types in `Concertable.Application.DTOs`
     and `Identity.Application.DTOs` respectively; nothing external called them anyway
   - `IdentityModule` (internal) implements `GetManagerAsync` via `IUserRepository`

10. ✅ **Redesign `ICurrentUser`** — complete:
    - `ICurrentUser` is claims-only in `Identity.Contracts`: `Id?`, `Role?`, `Email?`, `IsAuthenticated`
    - `CurrentUserAccessor` reads `IHttpContextAccessor` JWT claims directly (no DB)
    - Extension methods `GetId()`/`GetRole()` in Contracts — throw `UnauthorizedAccessException` (BCL) if null
    - `GlobalExceptionHandler` maps `UnauthorizedAccessException` → 401
    - `CurrentUserMiddleware` removed — no longer needed
    - All `GetEntity()`, `Get()`, `GetOrDefault()` removed entirely
    - `Concertable.Infrastructure.csproj` has direct `<ProjectReference>` to `Identity.Contracts`

10b. **Remove nav props from `ArtistEntity` and `VenueEntity`** ← **NEXT STEP** (10d was done first; 10b/10c were deferred):
    - Remove `ArtistEntity.User` (type `ArtistManagerEntity`)
    - Remove `VenueEntity.User` (type `VenueManagerEntity`)
    - Only `Guid UserId` FK remains on both
    - Update EF config in `ApplicationDbContext`:
      ```csharp
      entity.HasOne<ArtistManagerEntity>().WithOne().HasForeignKey<ArtistEntity>(a => a.UserId);
      entity.HasOne<VenueManagerEntity>().WithOne().HasForeignKey<VenueEntity>(v => v.UserId);
      ```
    - No nav prop on either side — EF enforces FK constraint only

10c. **Update all queryable mappers** (`.User.*` → direct field access):
    - `QueryableArtistMappers`: `a.User.Avatar` → `a.Avatar`, `a.User.Address` → `a.Address`, `a.User.Email` → `a.Email`
    - `QueryableVenueMappers`: same + `v.User.Location` → `v.Location`
    - `ConcertMappers`: `artist.User.Avatar` → `artist.Avatar`, `venue.User.*` → `venue.*`
    - `QueryableArtistHeaderMappers` (Search): `a.User.*` → `a.*`
    - `QueryableVenueHeaderMappers` (Search): `v.User.*` → `v.*`
    - `QueryableConcertHeaderMappers` (Search): chain through venue/artist
    - `QueryableConcertMappers`: chain through venue/artist
    - `ConcertEntity.LocationExpression`: `c.Booking.Application.Opportunity.Venue.User.Location` → `...Venue.Location`
    - `ConcertService` line ~130: same expression
    - `OpportunityApplicationService` line ~142: `artist.User.Email!` → `artist.Email!`

10d. ✅ **Wire Stripe services through `IIdentityModule`/`IUserRepository`**:
    - `StripeAccountController` + `StripeAccountValidator` — inject `IIdentityModule`, call `GetManagerAsync`
    - `StripeCustomerValidator`, `ArtistTicketPaymentService`, `VenueTicketPaymentService` — inject `IUserRepository` directly (need full entity, not just Contracts DTO)
    - `ArtistService` + `VenueService` — inject `IUserRepository` for Avatar at create time
    - `MessageService` stale `user.Id` → `userId` fixed
    - `DoorSplitConcertWorkflow` + `ManagerPaymentService` using `IManagerRepository<ManagerEntity>` — **deferred to Concert extraction**

10e. ✅ **Introduce `ICurrentUserResolver`** — complete:
    - `ICurrentUserResolver` in `Identity.Application.Interfaces`: `Task<UserEntity> ResolveAsync(ct)`
    - `CurrentUserResolver` (internal) in `Identity.Infrastructure.Services`: injects `ICurrentUser` + `IUserRepository`
    - Registered in `AddIdentityModule()` as scoped
    - `ArtistTicketPaymentService`, `VenueTicketPaymentService`, `StripeCustomerValidator`: dropped both, inject resolver only
    - `ArtistService`, `VenueService`, `StripeAccountController`: dropped `IUserRepository`, keep `ICurrentUser` for ID-only ops, add resolver for entity lookups
    - `UserService`: left unchanged — `IUserRepository` is needed for many non-currentUser operations there

10f. **Move `IManagerRepository` to `Identity.Application.Interfaces`**:
    - Currently in `Concertable.Application.Interfaces/IManagerRepository.cs`
    - Belongs in Identity — `ManagerEntity` is an Identity type
    - Used by `DoorSplitConcertWorkflow` + `ManagerPaymentService` — those stay in Infrastructure for now, referencing Identity.Application

11. ✅ **Rename `AddIdentityServices()` → `AddIdentityModule()`** — done. JWT bearer config stays
    in Web's `AddAuth()` which calls `AddIdentityModule()` internally.

12. ✅ **ReadDbContext barrier resolved** — `IEntityTypeConfiguration<T>` pattern introduced in
    `Concertable.Data.Infrastructure/Data/Configurations/`. `ReadDbContext.OnModelCreating` and
    `ApplicationDbContext.OnModelCreating` both call `ApplyConfigurationsFromAssembly(typeof(ReadDbContext).Assembly)`.
    `IdentityDbContext` applies only its 3 user-hierarchy configs explicitly.
    - 18/18 Search tests ✅, 129/129 integration tests ✅, 95/95 unit tests ✅
    - **Current: 234/236** — 2 `ConcertDraftTests` E2E failures pre-existing, unrelated

13. **Re-run full test suite at 236** — the 2 E2E `ConcertDraftTests` failures need separate investigation
    ⬜ Investigate `ConcertDraftTests` root cause (DoorSplit + Versus application acceptance workflows)

**Step 10e** — `ICurrentUserResolver` interface + implementation (deferred but documented):
- Lives in `Identity.Application.Interfaces`
- Replaces dual `ICurrentUser` + `IUserRepository` injection in services that need both

---

## Up Next: Artist Module

Once Identity is complete, Artist extraction begins. The payoff is immediate:
- `ArtistManagerEntity` is in `Identity.Application` — Artist module references it cleanly
- `ICurrentUser` is in `Identity.Contracts` — `ArtistService` injects it from the right place
- `Role` is in `Identity.Contracts` — no more `Concertable.Core` dependency for role checks
- `ArtistEntity` already owns its profile fields — no cross-module field borrowing to untangle

See `ARTIST_MODULE_REFACTOR.md` for the Artist plan. One addition vs. the original plan: `ReviewIdSelector` is **scrapped**. Each module owns its own review query logic inline — no cross-domain expression needed.

## Reference
- Search module extraction (complete) — `Concertable.Search.Contracts`, `.Application`, `.Infrastructure`
- Kamil Grzybek modular monolith series: `modular-monolith-with-ddd` on GitHub
