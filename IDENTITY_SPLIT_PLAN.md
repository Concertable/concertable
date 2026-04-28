# Identity Module Split Plan

Carve `Concertable.Identity.*` into three modules with cleaner modular-monolith
boundaries: `Auth`, `User`, and `Authorization`.

## Why

`Concertable.Identity.*` currently bundles six unrelated responsibilities:

1. **Authentication** — register / login / logout / refresh, password hashing,
   JWT issuance.
2. **Account recovery** — email verification + password reset tokens.
3. **Session/token lifecycle** — refresh-token storage, revoke/rotate.
4. **Authorization context** — `ICurrentUser`, `Role` enum, claim extraction
   from `HttpContext`. Cross-cutting; injected by every module.
5. **User identity / profile** — `UserEntity` TPH hierarchy, `UserRepository`,
   user lookup facades, location PUT.
6. **Inbound profile sync** — `VenueManagerSyncHandler` /
   `ArtistManagerSyncHandler` listening to Venue/Artist events.

Three of those (auth, account recovery, token lifecycle) are about
**credentials and security**. Two (user aggregate + manager TPH) are about
**who the user is**. One (authorization context) is **a cross-cutting concern
every module consumes**. They don't belong in one module.

## Target shape

### A. `Concertable.Auth.*` — credentials & tokens

Stateless security module. Owns everything about logging in and proving
identity.

- **`Auth.Api`** — `AuthController` (register, login, logout, refresh,
  verify-email, send-verification, forgot-password, reset-password,
  change-password).
- **`Auth.Application`** — `AuthService`, `IPasswordHasher`, `ITokenService`,
  `IAuthUriService`, request validators.
- **`Auth.Infrastructure`** — `JwtTokenService`, `BCryptPasswordHasher`,
  `AuthUriService`, `RefreshTokenRepository`,
  `EmailVerificationTokenRepository`, `PasswordResetTokenRepository`,
  `AuthDbContext`.
- **`Auth.Domain`** — `RefreshTokenEntity`, `EmailVerificationTokenEntity`,
  `PasswordResetTokenEntity`.
- **`Auth.Contracts`** — `IAuthModule`, `LoginResponse`, login/register/reset
  request records.

Auth depends on **`User.Contracts`** (calls `IUserModule.GetCredentialsAsync`,
`SetEmailVerifiedAsync`, `SetPasswordHashAsync`) and **`Authorization.Contracts`**
(reads `Role` to stamp into the JWT).

### B. `Concertable.User.*` — user aggregate

The `Identity` module renamed and slimmed.

- **`User.Api`** — `UsersController` (profile, location PUT).
- **`User.Application`** — `UserService`, `IUserService`, `IUserRepository`,
  role-keyed `IUserRegister` / `IUserLoader` polymorphism contracts,
  validators, mappers.
- **`User.Infrastructure`** — `UserRepository`, `UserDbContext`, role-keyed
  `VenueManagerRegister` / `ArtistManagerRegister` / `CustomerRegister` /
  `AdminRegister` + matching loaders, manager-sync event handlers.
- **`User.Domain`** — `UserEntity` (abstract base), `ManagerEntity`
  (abstract intermediate), `VenueManagerEntity`, `ArtistManagerEntity`,
  `CustomerEntity`, `AdminEntity`, `Address` value object,
  `UserCreatedDomainEvent`.
- **`User.Contracts`** — `IUserModule` (consolidates today's `IIdentityModule`
  + `IManagerModule`), `IUser` DTO, `UserRegisteredEvent`.

`IUserModule` surface:
- `GetByIdAsync(Guid id)` / `GetByIdsAsync(IEnumerable<Guid> ids)`
- `GetByEmailAsync(string email)`
- `GetCredentialsAsync(string email)` — returns hash + role + isVerified
- `CreateAsync(...)`
- `SetEmailVerifiedAsync(Guid userId)`
- `SetPasswordHashAsync(Guid userId, string newHash)`
- `UpdateLocationAsync(Guid userId, ...)`
- manager lookup (folded in from `IManagerModule`)

### C. `Concertable.Authorization.*` — cross-cutting context

No DbContext, no entities, no API surface. Pure context provider every other
module references.

- **`Authorization.Contracts`** — `ICurrentUser`,
  `ClaimsPrincipalExtensions`, `CurrentUserExtensions`, future policy-name
  constants.
- **`Authorization.Infrastructure`** — `CurrentUserAccessor` (HttpContext →
  `ICurrentUser`), `CurrentUserResolver` (DB-backed variant),
  `AddAuthorizationModule()` DI extension.

`Authorization.Contracts` references **`User.Contracts`** for the `Role`
enum type — one-way dependency.

### Roles vs Authorization

The `Role` enum stays in `User.Contracts` because it's **data on the user**
(the TPH discriminator column on `UserEntity`). The User module is the source
of truth for "what role does this user have?"

Authorization is the **decision layer** — `[Authorize(Roles = "Admin")]`,
policy handlers, claim mapping into the JWT, future per-permission RBAC.
That's not user-aggregate concern.

If per-permission RBAC grows later (claims beyond Role, policy → permission
mapping, `Permissions` table), it lives in `Authorization`, not `User`.

## TPH hierarchy stays intact

`VenueManagerEntity` / `ArtistManagerEntity` / `CustomerEntity` /
`AdminEntity` stay as TPH subtypes of `UserEntity` **inside the User module**.
They aren't Venue/Artist domain data — they're user accounts that happen to
manage a venue/artist. The actual `VenueEntity` / `ArtistEntity` already live
in their own modules with a `ManagerUserId` column pointing at `Users.Id`.

Splitting the TPH across modules would shred the discriminator (one table,
one type-hierarchy) and force cross-context inheritance, which EF can't model.

The role-keyed register/loader polymorphism stays internal to
`User.Infrastructure` — it's a registration-shape detail, not a cross-module
contract.

`CustomerEntity` stays a TPH subtype here even though the `Customer` *module*
owns Preferences — same pattern already locked in
(`project_future_modules.md`: "CustomerEntity (Identity TPH user-subtype)
stays in Identity"). Just rename mentally: Identity → User.

## DbContexts

| Context | Schema | Tables | Notes |
| --- | --- | --- | --- |
| `AuthDbContext` | `auth` | `RefreshTokens`, `EmailVerificationTokens`, `PasswordResetTokens` | `UserId : Guid` columns only — no nav, no SQL FK to `Users` (cross-context). Default `__EFMigrationsHistory`. |
| `UserDbContext` | `user` | `Users` | TPH discriminator on `Role`. Owns `Address` + NTS `Point` config. Inherits `DbContextBase`. |
| `Authorization` | — | — | No DbContext. |

Migration order in `initial-migrations.ps1`:
**Shared → User → Artist → Venue → Concert → Contract → Auth → AppDb.**

User runs early because Artist / Venue / Concert / Customer carry
`ManagerUserId` / `CustomerUserId` columns referencing `Users.Id`. Auth runs
late because nothing else FKs to its tables.

## Cross-module references after the split

- `Venue.Domain.VenueEntity.ManagerUserId : Guid` → `Users.Id`
  (no FK constraint, cross-context).
- `Artist.Domain.ArtistEntity.ManagerUserId : Guid` → `Users.Id` (same).
- Concert / Payment / Customer / Messaging / etc. consume:
  - `ICurrentUser` from `Authorization.Contracts`
  - `IUserModule` from `User.Contracts` for lookups
- Auth references `User.Contracts` + `Authorization.Contracts`. No reverse
  edges.

## What does NOT move in this split

The inbound profile sync (`VenueManagerSyncHandler`,
`ArtistManagerSyncHandler` copying avatar/location from Venue/Artist events
onto the manager user) is an inverse-coupling smell already tracked in
`project_location_refactor.md`. It stays inside the new User module for now;
fix it as part of the location refactor, not this split.

## Step order

1. **Stand up `Authorization` module.** Move `ICurrentUser`, `Role` enum,
   `ClaimsPrincipalExtensions`, `CurrentUserExtensions`,
   `CurrentUserAccessor`, `CurrentUserResolver` into the new project.
   Repoint every existing consumer's project ref from `Identity.Contracts` →
   `Authorization.Contracts` (+ `User.Contracts` once `Role` lands there;
   for step 1 the `Role` enum can temporarily live in
   `Authorization.Contracts` and migrate to `User.Contracts` in step 3).

2. **Stand up `Auth` module.** Move token entities, `AuthDbContext`,
   `AuthController`, `AuthService`, `JwtTokenService`,
   `BCryptPasswordHasher`, `AuthUriService`, request validators.
   Auth still calls into the existing Identity for user lookup via a
   temporary internal seam.

3. **Rename `Identity` → `User`.** Drop `IAuthModule` + token entities from
   the project. Rename `IIdentityModule` → `IUserModule`, fold
   `IManagerModule` in. Move `Role` enum from `Authorization.Contracts` to
   `User.Contracts`; update the one-way reference. Expose
   `GetCredentialsAsync` / `SetEmailVerifiedAsync` / `SetPasswordHashAsync`
   so Auth no longer reaches into `UserEntity`.

4. **Re-scaffold migrations** per `CLAUDE.md` — run
   `./initial-migrations.ps1` from `api/` to nuke and regenerate every
   module's `InitialCreate` in dependency order.

5. **Verify integration tests** — `Concertable.Web.IntegrationTests` boots
   all modules + runs Respawner between tests. Confirm the migration order
   and the cross-context `UserId` columns work end-to-end.

## Out of scope

- `VenueManagerSyncHandler` / `ArtistManagerSyncHandler` redesign — defer to
  the location refactor.
- Per-permission RBAC, policy expansion, claim transformations — future work
  inside `Authorization`.
- 2FA, social login (Google/Apple), account linking — future work inside
  `Auth`.
- Application/Booking rename (`APPLICATION_RENAME_PLAN.md`) — independent.
