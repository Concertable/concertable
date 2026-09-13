# Auth identity contract shapes plan

## Outcome

Roadmap item `auth-identity-model/extension-block-syntax`. Two related fixes to
`Concertable.Auth.Contracts`'s typed identity model, both breaking-published-package changes:

- `InteractiveClientInfo` (struct → `sealed record class`), `InteractiveClients` deleted and folded into it
  (`Get`/`GetOrDefault`/`All` as static members on the record), `MobileScheme`/`IsMobile` removed (zero
  external consumers), `Find` renamed `GetOrDefault`.
- `AuthScopes`/`AuthResources` converted from legacy `this`-parameter extension methods to C# 14
  `extension()` blocks — `.Id()`/`.Audience()`/`.AcceptedScopes()`/`.IncludedClaims()` become parenless
  properties.

Full design rationale: `plans/auth-identity-model/AUTH_IDENTITY_MODEL_ROADMAP.md`, item
`auth-identity-model/extension-block-syntax`.

`ServiceClient`/`ServiceClientInfo`/`ServiceClients` (in `Concertable.Auth`, never published) get the same
treatment but are unpublished — no breaking-package constraint, folded into the consumer-sync phase since
they live in the same files being touched there anyway.

## Phases

### Phase 1 — producer: reshape `Concertable.Auth.Contracts` (this PR)

Branch `Refactor/AuthIdentityContractShapes`. Scoped entirely to `api/Concertable.Auth.Contracts/` —
consumers bind the published package and cannot see this shape until it republishes.

**Verification gate:** `Concertable.Auth.Contracts` builds 0 warnings; its own unit tests green.
**Status: done, verified** — 32/32 tests green, 0 warnings, diff confirmed scoped to
`api/Concertable.Auth.Contracts/` only.

### Phase 2 — consumers (after Phase 1 publishes)

Fresh worktree off post-publish `origin/main`. Adds/bumps `ConcertableAuthVersion` in every consumer's
`Directory.Packages.props` to the version this Phase 1 publishes.

- Every `.Info()`/`.Find()` call site → `InteractiveClientInfo.Get(...)`/`.GetOrDefault(...)`.
- Every `.Id()`/`.Audience()`/`.AcceptedScopes()`/`.IncludedClaims()` call site → parenless property.
- `Config.cs`'s `MobileClient` helper: `MobileScheme` lookup removed, scheme goes back to being a plain
  literal parameter at each `Config.XMobileClient()` call site (as it was before `ClientIds` existed).
- `ServiceClient`/`ServiceClientInfo`/`ServiceClients` in `Concertable.Auth` get the same struct→class /
  fold-catalog-in / `Find`→`GetOrDefault` treatment — unpublished, no sequencing constraint, do it here.
- Every `[InlineData(...)]` test site referencing these types across all six consumers.

**Verification gate:** full affected build; every locally-runnable suite this branch touches;
`grep -rn "InteractiveClients\.\|ServiceClients\.\|\.Info()\.\|\.Find(" api --include='*.cs'` → zero
(outside historical/allowlisted references).

## Out of scope

- Any change to `AuthParty`/business classification — already resolved and closed in Phase 2 of the parent
  `auth-identity-model` epic.
- `Concertable.Auth`'s own `Config.cs`/`AuthHostExtensions.cs`/`AuthDevSeeder.cs` internal logic beyond the
  mechanical call-site rename — no behavior change intended.
