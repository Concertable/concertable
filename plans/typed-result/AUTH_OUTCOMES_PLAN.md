# Auth expected-outcome migration

> Next steps live in @plans/typed-result/AUTH_OUTCOMES_PROGRESS.md -> `## Next Steps`.

**Status:** The semantic migration, published Reunion conversion, domain-ownership correction,
alpha.3 package reconciliation, and final producer-terminal audit are locally implemented. Auth has
no Payment, B2B, or Customer runtime/package dependency.

## Outcome

Replace ambiguous `IAuthService` null, boolean, enum, and completion outcomes with the smallest
Reunion-backed in-process contract that preserves each caller's real decisions. Ordinary absence
becomes `Option<T>`. Expected refusals that a Razor or Duende caller must act on become
operation-owned typed Results. Completion-only operations remain `Task` when exposing a distinction
would disclose account state or give the caller no useful action.

The migration ends at Auth's edges. Razor Pages keep their framework-facing page state and
`IActionResult` shapes, Duende keeps `GrantValidationResult` and logout-context shapes, and OAuth/OIDC
wire behavior does not carry `Result` or `Option` values.

## Definition of done

- Every `IAuthService` outcome has been classified from all of its callers, not from its current CLR
  return type.
- `LoginAsync` and `LogoutAsync` expose ordinary absence with `Option<T>`.
- Registration, password change, email verification, and password reset expose one operation-owned
  `UnitResult<TError>` refusal each, with exact definition contract tests.
- Verification-email and password-reset-email requests remain completion-only where absence is an
  intentional no-op and no caller can safely act on it.
- Invalid credentials, an unknown account, an unverified account, and privacy-sensitive reset/email
  no-ops remain externally indistinguishable wherever they are indistinguishable today.
- EF query nullability remains inside `AuthService`; no persistence contract returns `Option<T>`.
- Database, Duende, email/outbox, cancellation, and invariant failures remain exceptions.
- `CredentialEntity` owns password verification and mutation decisions. Verification/reset token
  entities own token-expiry refusal and successful mutation; `AuthService` only maps missing rows and
  coordinates persistence.
- Expected domain refusals use operation-owned Results. A token paired with the wrong credential is
  an invariant defect and remains a `DomainException`; no application pre-check duplicates it.
- Error definitions use the current direct `ErrorDefinition.<Kind><TCase>(...)` API. No
  `ErrorDefinition.For<TError>()` call remains.
- Every used Reunion package resolves exactly `0.1.0-alpha.3`; Auth does not add the unused Validation
  or AspNetCore packages.
- No Result/Option/error carrier crosses Razor, HTTP, OAuth/OIDC, Duende, event, or persistence wire
  shapes.
- Auth builds from its own published package closure, including the Reunion-backed Kernel package at
  the service's integrated platform pin, and passes the standalone carve.
- The new Auth unit and integration suites cover the migrated contracts and the previously uncovered
  edge behavior.
- The final PR passes review, build, unit/integration tests, Auth carve, full merge-queue API and UI
  E2E, merge, publication, and the generated platform-sync gate.
- The PR #470 audit proves no duplicated application pre-check/domain throw, no invariant exception
  converted to an expected outcome, and no `DomainException`-to-HTTP behavior changed by this branch.

## Ownership and invariants

- Auth remains credential-only: email, password hash, verification state, IdentityServer, sign-in/
  sign-up pages, token issuance, and identity claims. No role, tenant, customer, or downstream user
  projection concept enters the service.
- This work changes Auth runtime and Auth-owned tests. Its only cross-area change aligns the Shared.Api
  typed-result architecture guard with the merged exhaustive-switch convention; no shared or sibling
  service runtime changes. Payment, B2B, Customer, and Search runtime code is not an integration
  surface for this migration.
- Existing client IDs, claims, cookies, authorization contexts, return URLs, logout prompts, token
  endpoint errors, page messages, and redirects remain framework-owned edge behavior.
- Auth projects whose source uses functional carriers or errors own direct `Reunion` / `Reunion.Errors`
  references. Auth HTTP edges own `Reunion.AspNetCore` only where they map those carriers. No shared
  Kernel API, compatibility shim, or alternative carrier is added.

## Caller and outcome audit

| Operation | Current outcomes and callers | Caller decision | Target contract and edge mapping |
|---|---|---|---|
| `LoginAsync` | `ClaimsPrincipal?`; consumed by `LoginModel` and `ResourceOwnerPasswordValidator` | Both callers need a principal or one deliberately indistinguishable authentication miss. Unknown email, wrong password, and unverified email must not become distinct. | `Task<Option<ClaimsPrincipal>>`. Razor maps `None` to the existing `Invalid email or password.` page response. Duende maps `None` to the existing `invalid_grant` / `Invalid credentials`; `Some` retains cookie or subject issuance. |
| `LogoutAsync` | `string?`; consumed only by `LogoutModel` after cookie sign-out | The redirect exists or it does not; absence is ordinary and needs no explanation. | `Task<Option<string>>`. The page maps `Some` to that redirect and `None` to `/`. Duende's `ShowSignoutPrompt` check remains outside this operation. |
| `RegisterAsync` | `RegisterResult.Success` or `EmailAlreadyExists`; consumed only by `RegisterModel` | Duplicate email is an expected, caller-actionable refusal and is intentionally disclosed by the current registration page. | `Task<UnitResult<RegisterError>>`. Success sets `Submitted`; failure renders the owned safe message. Remove `RegisterResult`. |
| `ChangePasswordAsync` | `bool`; consumed only by authorized `ChangePasswordModel` | Missing credential and incorrect current password deliberately lead to the same corrective action and message. | `Task<UnitResult<ChangePasswordError>>`. Both refusal sources collapse to `CurrentPasswordIncorrect`; success keeps the existing page state. A malformed/missing `sub` remains a Razor-edge identity failure before the call. |
| `SendEmailVerificationAsync` | completion-only; called by registration; missing credential is a silent no-op | There is no meaningful distinction for the caller to act on, and a result would manufacture vocabulary. | Remain `Task`. Keep the missing-credential no-op and exception propagation. Do not wrap it in `Result`. |
| `VerifyEmailAsync` | `bool`; consumed only by `VerifyEmailModel` | Unknown, expired, and orphaned tokens all render the same safe failure; success marks the credential verified and consumes the token. | `Task<UnitResult<VerifyEmailError>>`. Razor maps success/failure to its existing `Success` page state; no typed carrier enters the page or URL. |
| `SendPasswordResetAsync` | completion-only; consumed only by `ForgotPasswordModel`; unknown email is a silent no-op | The page must always claim submission so an account cannot be discovered. The caller has no safe failure branch. | Remain `Task`. Preserve the same submitted response for known and unknown email, and propagate real infrastructure/cancellation failures. |
| `ResetPasswordAsync` | `bool`; consumed only by `ResetPasswordModel` | Unknown, expired, and orphaned tokens all render the same safe failure; success changes the hash and consumes the token. | `Task<UnitResult<ResetPasswordError>>`. Razor keeps its existing success state and `Invalid or expired reset link.` response. |

No current `IAuthService` boolean is a complete capability predicate. Page-model booleans such as
`Success` and `Submitted` are framework-facing rendering state, not operation contracts, and remain
boolean.

## Operation-owned errors

All alternatives are payload-free and callers do not need runtime case discrimination. Use
operation-owned Dunet unions with one natural case per outcome, one exhaustive `Definition` switch,
and direct case construction. Do not add wrapper factories or static error catalogs.

| Type/value | Definition | Preserved caller-safe message |
|---|---|---|
| `RegisterError.EmailAlreadyExists` | `Conflict`, code `register.email_already_exists` | `An account with that email already exists.` |
| `ChangePasswordError.CurrentPasswordIncorrect` | `Unauthenticated`, code `change.password_current_password_incorrect` | `Current password is incorrect.` |
| `VerifyEmailError.InvalidOrExpiredToken` | `Invalid`, code `verify.email_invalid_or_expired_token` | `This verification link is invalid or has expired.` |
| `ResetPasswordError.InvalidOrExpiredToken` | `Invalid`, code `reset.password_invalid_or_expired_token` | `Invalid or expired reset link.` |

Keep the definitions beside the Auth operation contract. Unit tests hard-code each value, code,
message, and `ErrorKind`; expected values must not be calculated with production helpers.

## Boundary mapping rules

- Keep `FirstOrDefaultAsync` / `FindAsync` nullable results in `AuthService`, then create `Option` or a
  typed failure at the service return boundary.
- Use `TryGetValue`, `TryGetError`, `Match`, `ValueOr`, or the alpha.3 construction surface. A
  target-typed raw payload is valid only where success/error intent is unambiguous; use exact named
  cases where payload types overlap or branch intent matters. Do not add throwing payload accessors or
  local unwrap/conversion helpers.
- Do not catch EF, Duende, outbox/email, hashing, token-generation, or cancellation exceptions to turn
  them into expected outcomes. Tests must prove representative infrastructure and cancellation faults
  still escape.
- Do not distinguish unknown credentials from bad passwords or unverified credentials in either the
  Razor login page or password-grant response.
- Do not distinguish an unknown reset email from a known one in the forgot-password page response.
- Do not distinguish missing, expired, or orphaned verification/reset tokens at the Razor edge.
- Keep `ClaimsPrincipal`, redirect strings, page booleans/messages, `IActionResult`,
  `GrantValidationResult`, cookies, and protocol error strings as edge-owned values.

## Existing coverage inventory

| Layer | What exists | Material gaps before this plan |
|---|---|---|
| Auth unit | No `api/Concertable.Auth/tests/` tree and no Auth unit project. Kernel tests prove carrier behavior but do not exercise Auth contracts or adapters. Downstream unit tests only consume `CredentialRegisteredEvent`. | No definition contracts, caller mapping tests, exception-path assertions, or direct coverage of any `IAuthService` outcome. |
| Auth integration | No Auth fixture or integration project. B2B, Customer, and Search integration suites replace JWT validation and do not boot Auth. The shared `MockEmailSender` is test infrastructure, not Auth behavior coverage. | No real Auth persistence/token-expiry tests, Razor response tests, Duende protocol tests, privacy/no-disclosure tests, or logout/password coverage. |
| API E2E | B2B and Customer API fixtures use `Concertable.Testing.E2E.TestTokenMinter` against Auth's `/connect/token` with seeded, verified credentials. This repeatedly covers the successful password-grant login path. | No assertion for unknown/wrong/unverified credentials or the exact `invalid_grant` response; no logout, registration, change/reset, invalid token, or disclosure coverage. |
| UI E2E | Passing Customer and B2B login scenarios cover successful OIDC login. Passing Customer, Venue, and Artist sign-up scenarios cover registration success, fake-email auto-verification, and subsequent sign-in. | No invalid/unknown/unverified login, duplicate registration, logout, change password, forgot/reset password, invalid/expired verification/reset link, or unknown-email indistinguishability scenario. |

The new Auth-owned tests close the behavioral gaps at unit/integration level. Existing B2B and Customer
API/UI E2E remain unchanged and run in full in the merge queue as cross-surface compatibility coverage;
this branch does not turn either service's test tree into Auth's owner or add their runtime as an Auth
dependency.

## Target test structure

- Add `api/Concertable.Auth/tests/Concertable.Auth.UnitTests/` for exact error-definition contracts and
  focused Razor/Duende adapter tests that use in-memory doubles only.
- Add `api/Concertable.Auth/tests/Concertable.Auth.IntegrationTests.Fixtures/` as Auth-owned
  `WebApplicationFactory`/Testcontainers SQL infrastructure. It may reference service-agnostic shared
  testing primitives, but Auth-specific configuration, seed helpers, clocks, token generators, and
  doubles stay here.
- Add `api/Concertable.Auth/tests/Concertable.Auth.IntegrationTests/` for real HTTP/Razor/Duende flows
  against the real Auth `Program` and database. Add all three projects to `api/Concertable.slnx`; name
  the test project `*.IntegrationTests.csproj` so CI discovers it.
- Characterize the externally observable behavior before changing signatures, then keep those tests
  unchanged through the contract migration. Use targeted unit tests only where a framework adapter's
  owned-value mapping cannot be isolated cleanly over HTTP.
- Cover successful and refused outcomes, token consumption, state mutation/no-mutation, exact safe
  messages/protocol errors, known-vs-unknown disclosure parity, and representative exception and
  cancellation propagation.

## Phases

Each phase is one local, independently shippable checkpoint. Finish its complete verification gate,
update this plan and the ledger in the same commit, commit without asking, and stop at the ledger's
next handoff. There is no package or deployment boundary between phases, so they stay on this branch
and ship in one PR.

### Phase 1 - Auth test foundation and owned error vocabulary (complete)

- [x] Add a direct `Concertable.Kernel` package reference and central package version to Auth's published
  package closure; use the existing `ConcertablePlatformVersion` and do not enable local-core mode.
- [x] Add the Auth unit, integration-fixture, and integration test projects described above, including
  only the package/tooling entries they require.
- [x] Add the four operation-owned error unions and exact unit contract tests without changing
  `IAuthService` signatures or runtime callers yet.
- [x] Add HTTP characterization coverage for the current successful and refused login, logout,
  registration, email verification, password change, forgot-password, and password-reset behavior,
  including account-disclosure parity and exception/cancellation paths.
- [x] Update `api/Concertable.slnx`. Do not change models or migrations.

### Phase 2 - Login and logout ordinary absence

- [x] Change `LoginAsync` to `Task<Option<ClaimsPrincipal>>` and convert nullable EF lookup state at the
  service boundary.
- [x] Map `Option` in `LoginModel` and `ResourceOwnerPasswordValidator` to the existing Razor cookie and
  Duende password-grant behavior.
- [x] Change `LogoutAsync` to `Task<Option<string>>` and map it in `LogoutModel` to the existing redirect or
  `/` fallback after the existing cookie sign-out and prompt rules.
- [x] Cover `Some`/`None` adapter behavior and prove unknown email, wrong password, and unverified email
  remain identical at both login edges.

### Phase 3 - Registration and email verification refusals (complete)

- [x] Replace `RegisterResult` with `UnitResult<RegisterError>` in the service and page caller; delete the
  enum and preserve the existing duplicate-account disclosure and registration success state.
- [x] Replace `VerifyEmailAsync`'s boolean with `UnitResult<VerifyEmailError>`; collapse missing, expired,
  and orphaned token rows to the one safe refusal and preserve token consumption on success.
- [x] Keep `SendEmailVerificationAsync` completion-only and preserve its missing-credential no-op.
- [x] Prove duplicate registration does not create a second credential or send a second verification,
  valid verification mutates once, invalid variants do not mutate, and infrastructure/cancellation
  faults are not converted to typed failures.

### Phase 4 - Password changes, reset privacy, and exhaustive cleanup (complete)

- [x] Replace `ChangePasswordAsync`'s boolean with `UnitResult<ChangePasswordError>`; preserve the same
  failure for missing credential and incorrect current password and the same success UI.
- [x] Keep `SendPasswordResetAsync` completion-only. Prove known and unknown emails return the identical
  forgot-password page while only the known account gains a reset token/email command.
- [x] Replace `ResetPasswordAsync`'s boolean with `UnitResult<ResetPasswordError>`; collapse missing,
  expired, and orphaned token rows to one safe refusal and preserve one-time token consumption.
- [x] Complete the caller inventory and require no surviving `RegisterResult`, nullable login/logout
  return, or command-success boolean in `IAuthService`; allow only the two intentional completion-only
  email operations and ordinary nullable input parameters.
- [x] Re-run every Auth test plus the unchanged cross-surface coverage inventory before entering review.

### Phase 5 - Domain ownership and current Reunion API (complete)

- [x] Move authentication capability and password-change/reset decisions into `CredentialEntity`
  behind an Auth-domain password-hasher port.
- [x] Move verification/reset token expiry decisions and successful credential mutation into the token
  entities; keep token/credential identity mismatch exceptional as a domain invariant.
- [x] Move domain-owned errors beside those rules and replace the removed
  `ErrorDefinition.For<TError>()` factory with the current direct generic factories.
- [x] Add focused entity tests for success, expected refusal without mutation, and invariant failure.
- [x] Re-run Auth unit/integration tests, the architecture slice, full Release solution build, fresh
  standalone Auth carve, and mechanical checks.

### Phase 6 - Reunion alpha.2 baseline and construction ergonomics (complete)

- [x] Align Auth's existing direct `Reunion` and `Reunion.Errors` references to
  `0.1.0-alpha.2`; do not add `Reunion.Validation` or `Reunion.AspNetCore`.
- [x] Adopt target-typed raw payload or exact named-case conversions where they simplify the existing
  Auth-owned contracts without weakening branch intent or error-union ownership.
- [x] Rerun the Auth verification gate before incremental review and PR preflight.

### Phase 7 - Reunion alpha.3 published baseline (delivery in progress)

- [x] Align Auth's direct `Reunion` and `Reunion.Errors` references to `0.1.0-alpha.3` after the
  producer packages are indexed on NuGet.org.
- [x] Audit the additive flexible Option HTTP terminals. Auth still owns no Minimal API or MVC
  terminal surface, so `Reunion.AspNetCore` remains absent and no runtime call site changes.
- [x] Rerun the Auth verification gate and review the package update.
- [x] Push the exact candidate through the plan push protocol.
- [x] Require replacement checks to pass and return PR #517 to the full-E2E merge queue.
- [x] Replace the repeatedly failing GitHub release bootstrap with one pinned Stripe package installer,
  then verify and review it.
- [x] Push the bootstrap fix through the plan push protocol.
- [x] Require replacement checks and return PR #517 to the full-E2E merge queue.

## Verification gate for every phase

1. Run the affected Auth unit and integration projects through the `integration-debug` workflow;
   diagnose and drive any failure to green rather than reporting it.
2. Run `dotnet build api/Concertable.slnx --configuration Release` and require zero errors.
3. Reproduce `carve-auth` from a fresh temporary tree containing the phase checkpoint, then run
   `dotnet build src/Concertable.Auth/Concertable.Auth.csproj --configuration Release` from that carve
   with package restore only. Do not use `UseLocalCore=true`.
4. Run `git diff --check` and the phase-specific signature/legacy-carrier searches.
5. Do not run API or UI E2E locally before the PR. The merge queue is the E2E gate.

No phase changes the EF model, so `initial-migrations.ps1` is not expected. If implementation reveals
a genuine model change, stop and amend the plan before touching migrations.

## Review and delivery lifecycle

1. After the final package reconciliation, run incremental review from the
   existing review watermark; resolve every clear finding.
2. Reconcile with current `origin/main`, audit Auth's actual package/HTTP topology, replace old carrier
   imports and terminals with directly owned Reunion packages at their real edges, then rebuild,
   retest, re-carve, incrementally review, and run PR preflight. If topology proves no unpublished
   package dependency, Auth may deliver independently of Payment.
3. Push the single Auth branch and open a plain GitHub PR. Do not add `skip-e2e`, `skip-e2e-ui`, or a
   skip trailer: this changes user-facing Auth and protocol flows, so full merge-queue API and UI E2E
   are required.
4. Merge through the normal queue workflow. A genuine E2E failure is dispatched to the matching E2E
   debug workflow and is not blindly retried.
5. Because this changes `api/**`, follow package publication and the generated platform-sync PR to a
   terminal green merge. Fix a red sync in its own sync branch; never leave it behind.
6. Only after review, PR, merge, publication, and platform sync are terminal: record the final evidence
   in the ledger, update the permanent epic tracker's Auth item, then delete this plan and its ledger
   together in the following close-out change. The permanent tracker is never deleted.

## Out of scope

- Roles, user kinds, tenant/customer concepts, business profiles, downstream projections, or claims
  beyond Auth's existing credential identity responsibility.
- Payment, B2B, Customer, or Search runtime changes, or cross-service runtime references.
- New shared Kernel operations, alternative Result/Option carriers, implicit conversions, or local
  functional helpers.
- Result types for silent email/no-op operations or any other outcome that gives callers no meaningful
  distinction.
- Result/Option values in HTTP, Razor form models, Duende protocol models, events, persistence entities,
  or other wire contracts.
- Opportunistic Auth model, migration, validation, UI redesign, or account-policy changes.

If implementation proves a concrete Kernel operation is missing, stop this plan at that call site and
record a separate additive shared-foundation item with its own publish and platform-sync gate. Do not
expand Kernel on this branch.
