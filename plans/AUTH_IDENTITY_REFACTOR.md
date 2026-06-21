# Auth Identity Refactor

> **Goal:** Make `Concertable.Auth` a self-contained identity service with no compile-time dependency on any application's user module.
>
> **Status:** Phase 1 ✅ complete (2026-05-22). Phase 2 ✅ complete (2026-05-22). Phase 3 ✅ structurally
> complete in code — Auth owns `CredentialEntity` + `AuthDbContext`, B2B's User module is now an event-driven
> projection consuming `CredentialRegisteredEvent`, and the remote claims provider exists (the tenant-scoping
> work landed the cut; the step-by-step below was never ticked off line-by-line). The identity-only north star
> is finished by `USER_MODEL_PLAN.md` Phases 5 & 7, which delete the transitional `owner` then `role` claims.
>
> **Why:** Auth is shared infrastructure. Hard-wiring it to B2B's `IUserModule` means any consumer (Customer, future services) would need the same coupling. The `Role` enum and credential storage belong to B2B and Auth respectively — not both in one place.

---

## Phase 1 — Dissolve the Authorization module into Kernel ✅ (2026-05-22)

> `Concertable.Authorization.*` was a 2-project module inside B2B holding `ICurrentUser`, `CurrentUserAccessor`, and `AddAuthorizationModule()`. Cross-cutting current-user identity belongs in Kernel — every project already references it and nothing about `ICurrentUser` is B2B-specific.

1. ✅ **Move identity types into `Concertable.Kernel/Identity/`.**
   - `ICurrentUser`, `CurrentUserExtensions`, `ClaimsPrincipalExtensions` → `Concertable.Shared.Infrastructure.Identity`
   - `CurrentUserAccessor` (internal) → same namespace
   - `AddCurrentUser()` added to Kernel's `ServiceCollectionExtensions` (replaces `AddAuthorizationModule()`)

2. ✅ **Update all consumers.**
   - 15 `GlobalUsings.cs` files: `Concertable.Authorization.Contracts` → `Concertable.Shared.Infrastructure.Identity`
   - 6 host/composition files: remove `using Concertable.Authorization.Infrastructure.Extensions`, `AddAuthorizationModule()` → `AddCurrentUser()`
   - 4 direct `using Concertable.Authorization.Contracts` statements in concrete files updated
   - `Notification.Infrastructure.csproj`: Authorization.Contracts ref replaced with direct Kernel ref
   - `DataAccess.Infrastructure.csproj`, `Ticket.Infrastructure.csproj`, `Review.Infrastructure.csproj`: Authorization.Contracts refs removed (Kernel already reachable transitively)
   - Stale `InternalsVisibleTo("Concertable.Authorization.Infrastructure")` removed from `User.Application/AssemblyInfo.cs`

3. ✅ **Delete the Authorization module.**
   - `api/Concertable.B2B/Modules/Authorization/` deleted
   - Both projects removed from `Concertable.slnx` and `Concertable.B2B.slnx`

**Exit criteria met:** Zero `Concertable.Authorization` references in the solution. Full build green (0 errors).

---

## Phase 2 — Claims-provider seam ✅ (2026-05-22)

> `ProfileService` currently reads `IUser.Role` from `Concertable.User.Contracts` to build the `"role"` claim. This is the remaining Auth→B2B coupling at the claim level. Replace it with an abstract `IProfileClaimsProvider` so Auth is claim-agnostic.

4. ✅ **Create `Concertable.Auth.Contracts` project.**
   - Must be a separate project — the interface cannot live in the Auth executable (circular reference once B2B implements it).
   - Holds `IProfileClaimsProvider`: `Task<IEnumerable<Claim>> GetClaimsAsync(Guid subjectId)`.
   - Add to `Concertable.slnx` under a `/Auth/` folder.

5. ✅ **Refactor `ProfileService` to aggregate providers.**
   - Inject `IEnumerable<IProfileClaimsProvider>` instead of `IUserModule`.
   - `GetProfileDataAsync` iterates providers, collects claims, calls `context.AddRequestedClaims(...)`.
   - `IsActiveAsync` still uses `IUserModule.GetCredentialsByIdAsync` for now (that goes away in Phase 3).

6. ✅ **Add interim in-process B2B provider.**
   - `UserProfileClaimsProvider : IProfileClaimsProvider` in `Concertable.User.Infrastructure`.
   - Fetches `IUser` via `IUserModule.GetByIdAsync`, returns `email`, `email_verified`, `role` claims.
   - Registered in `AddUserModule()`.
   - This keeps Phase 2 independently shippable and the token shape unchanged. It is replaced in Phase 3 by the remote provider.
   - Auth registers an Auth-local provider for `email` / `email_verified` only after Phase 3 when it owns the credential store.

**Exit criteria met:** Web login → token still contains `email`, `email_verified`, `role`. Auth no longer has a direct `IUser.Role` reference in `ProfileService`.

---

## Phase 3 — The cut: Auth owns credentials, User module goes event-driven

> This is the structural separation. Auth gets its own credential store. B2B's User module sheds credentials and becomes an event-driven projection consuming Auth's registration events.

### 3a. Auth gets its own store

7. **Add `AuthDbContext` and credential entities.**
   - Schema `auth`, new `AuthDb` connection string.
   - `CredentialEntity`: `Id` (Guid, `sub`), `Email`, `PasswordHash`, `IsEmailVerified`.
   - `EmailVerificationTokenEntity`, `PasswordResetTokenEntity`: move the shape from `Concertable.User.Infrastructure/Data/`.
   - EF configs in `Concertable.Auth` (no cross-context FKs, no nav chains into B2B).

8. **Wire `AuthDb` in AppHost.**
   - Add SQL Server resource `auth-db` / connection name `AuthDb`.
   - Point `Concertable.Auth` at it.

9. **Add outbox and ASB transport to Auth.**
   - `AddOutbox(AuthDb, runDispatcher: true)` + `AddAzureServiceBusTransport` (publish-only).
   - Auth currently has neither; it was borrowing B2B's outbox with `runDispatcher: false`.

### 3b. Move credential/token logic into Auth

10. **Re-home `AuthService` onto `AuthDbContext`.**
    - Registration, credential lookup, email-verify, password-reset now read/write `CredentialEntity` and the token entities directly. No `IUserModule` calls.
    - BCrypt hashing stays in Auth (`IPasswordHasher` → `BCryptPasswordHasher` — already there).
    - Token generation (`RandomNumberGenerator.GetBytes(32)`, URL-safe Base64) moves from `User.Infrastructure` into Auth.
    - Email uniqueness: one credential per email (global, not per-role). Single `EmailExistsAsync` check replaces the three per-role checks.

11. **Raise registration events from domain events.**
    - Add `IPreCommitDomainEventHandler` on `CredentialEntity` that fires `UserRegisteredDomainEvent`.
    - Handler publishes the appropriate `*RegisteredEvent` to the outbox via `IBus`.
    - No service-layer `eventBus.PublishAsync` — canonical pattern only.

### 3c. Registration events move to `Concertable.Auth.Contracts`

12. **Move the four registration events.**
    - `CustomerRegisteredEvent`, `VenueManagerRegisteredEvent`, `ArtistManagerRegisteredEvent` from `Concertable.User.Contracts/Events/` → `Concertable.Auth.Contracts`.
    - `AdminRegisteredEvent`: no consumer (admins are seeded) — drop it.
    - Update all consumers: B2B User module (new — step 13), Customer `CustomerProfileCreationHandler`, Payment `CustomerRegisteredHandler` / `ManagerRegisteredHandler`.
    - `Concertable.Auth.Contracts` added as project reference in Consumer projects.

### 3d. B2B User module becomes a downstream projection

13. **User module consumes registration events.**
    - New inbox-idempotent handlers: `CustomerRegisteredHandler`, `VenueManagerRegisteredHandler`, `ArtistManagerRegisteredHandler` each create the `UserEntity` + matching profile row (`VenueManagerProfileEntity` etc.) in one transaction.
    - This preserves the atomicity that `CreateVenueManagerAsync` / `CreateArtistManagerAsync` previously had in one DB call — it's just now within the inbox consumer's transaction instead of the registration call.
    - B2B gains `AddInbox` + `AddAzureServiceBusTransport` subscription wiring if not already fully present.

14. **Shed credentials from `UserEntity` and `IUserModule`.**
    - `UserEntity.PasswordHash` removed. `UserEntity` keeps `Id` (`sub`), `Email` (denormalized copy), `Role`, `Address`, `Location`, `Avatar`.
    - `IUserModule` loses: `*EmailExistsAsync`, `Create*Async`, `GetCredentials*Async`, `SetEmailVerifiedAsync`, `SetPasswordHashAsync`, and all four token methods.
    - `IUserModule` keeps: `GetByIdAsync`, `GetByIdsAsync`, `GetManagerByIdAsync`.
    - Authorization policy handlers (`VenueManagerProfileHandler` etc.) unchanged — they query profile rows by `sub` claim.
    - Decide at implementation: keep or drop `IUser.IsEmailVerified` (denormalized copy synced via event vs. removed if no B2B consumer uses it).

### 3e. Remote claims provider

15. **B2B exposes an internal S2S claims endpoint.**
    - `GET /internal/users/{sub}/claims` in `Concertable.User.Api`, returns `role` (and any future B2B) claims.
    - Protected by client-credentials S2S auth. Add a new scope (e.g. `user:claims`) + Duende client for Auth in `Config.cs`.

16. **Auth registers an HTTP-backed `IProfileClaimsProvider`.**
    - Calls the B2B endpoint during token issuance, acquiring an S2S token via `ITokenService` (already in Kernel).
    - Short-TTL in-memory cache keyed on `sub` — the claim does not change between requests.
    - Delete the interim in-process `UserProfileClaimsProvider` from Phase 2.
    - Auth also registers its own Auth-local provider for `email` / `email_verified` from `CredentialEntity`.

### 3f. Drop the B2B coupling from Auth

17. **Remove `Concertable.User.Infrastructure` reference from Auth.**
    - Remove `<ProjectReference>` and `AddUserModule(...)` from `Concertable.Auth`.
    - `using Concertable.User.Contracts;` disappears from `ProfileService` / `AuthService`.

18. **Split seeders.**
    - Auth's `IDevSeeder` / `ITestSeeder` seed `CredentialEntity` rows.
    - B2B's User seeder seeds `UserEntity` / profile rows — or, preferably, publishes registration events so the projection path is exercised in dev.
    - Admin still seeded directly (no `AdminRegisteredEvent`).

**Exit criteria:** `Concertable.Auth.csproj` has zero project references to `Concertable.User.*`. Register each role → credential row in `AuthDb` → event on bus → `UserEntity` + profile row in B2B via inbox. Login issues a token with `role` claim via the remote provider. Email-verify and password-reset end-to-end against `AuthDb`.

---

## Phase 4 — Re-scaffold migrations + end-to-end verification

19. **Re-scaffold all `InitialCreate` migrations.**
    - `./initial-migrations.ps1` from `api/` for all existing module DbContexts (User module model changed).
    - Scaffold `AuthDbContext`'s `InitialCreate` separately.
    - No additive migrations — nuke and recreate databases (no live data).

20. **End-to-end verification.**
    - AppHost starts cleanly: B2B.Web, B2B.Workers, Auth, Customer.Web, Customer.Workers, Payment.Web, Payment.Workers.
    - Registration → projection → login → token flow works for all three manager roles + customer.
    - B2B authorization policies (`VenueManager`, `ArtistManager`, `Admin`) still gate their endpoints.
    - Existing auth E2E (web login → token contains `role` claim) passes unchanged.

---

## What this does NOT change

- Authorization policy handlers in B2B (`VenueManagerProfileHandler` etc.) — untouched, they use `sub` not `role`.
- The `role` claim's purpose in the SPA (`guards.ts`, `useRole`) — unchanged.
- Customer service — it already has no `IUserModule`/`Role` dependency; only updates needed are the event namespace in step 12.
- Payment service — same: only event namespace update in step 12.
