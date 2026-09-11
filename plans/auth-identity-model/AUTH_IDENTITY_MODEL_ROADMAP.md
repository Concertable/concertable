# Auth identity model — roadmap

Replace the flat `ClientIds` / `ApiScopeIds` string-constant classes in `Concertable.Auth.Contracts` with a
typed model — `AuthParty`, `InteractiveClient` / `InteractiveClients`, `AuthScope` / `AuthScopes`,
`AuthResource` / `AuthResources` — so every OIDC client id and API scope has one home, the wire strings live
in exactly one catalog each, and the three cross-service handlers that classify a registration client id
switch on `AuthParty` instead of matching hand-maintained string sets.

## Items

- [x] `auth-identity-model/typed-identity-contract` — add the typed model to the published
  `Concertable.Auth.Contracts` package (additive, `[Obsolete]` the old classes), then migrate every consumer
  (Auth `Config.cs` + host wiring, the B2B/Customer registration handlers, the four resource-server hosts,
  `TestTokenMinter`, tests) and delete the old classes. Breaking published-contract change: producer PR
  publishes first, consumer migration follows against the bumped pin.
- [ ] `auth-identity-model/extension-block-syntax` — convert `InteractiveClients`/`AuthScopes`/
  `AuthResources`/`ServiceClients` from legacy `this`-parameter extension methods to C# 14 `extension()`
  blocks (`csharp-style` requirement, missed by both Phase 1 review rounds). Two PRs, not one: a
  producer-only PR touching only `api/Concertable.Auth.Contracts/` that publishes independently, then a
  separate consumer-sync PR retargeting every `.Id()`/`.Info()`/`.Audience()`/`.AcceptedScopes()`/
  `.IncludedClaims()` call site to the parenless property form once that publish lands and every consumer's
  `ConcertableAuthVersion` pin moves past it.

  **This work no longer belongs in the monorepo.** The monorepo stopped publishing
  `Concertable.Auth.Contracts` and `Concertable.Auth.Hosting` (`PROMOTED_TARGETS` in
  `.github/scripts/package_ownership.py`), so the producer PR described above has nothing to publish from
  here. Do it in `Concertable/auth` once that repository publishes canonically, and bump each monorepo
  consumer's `ConcertableAuthVersion` only after that publish lands.

  **Do not attempt the producer and consumer halves in one PR — it was tried and cannot build.** Converting
  the four containers and updating every call site across the six consumers left
  `Concertable.Auth.Contracts` building with its own 50 tests green, and `Concertable.Auth` failing with 31
  `CS0119`/`CS1503` errors at every `.Id`/`.Info`/`.Audience`/`.AcceptedScopes`/`.IncludedClaims` site. The
  cause is structural, not a mistake in the conversion: every consumer restores `Concertable.Auth.Contracts`
  as a `PackageReference` — never a `ProjectReference`, and it is absent from
  `api/PlatformSourcePackages.targets`' locally source-swappable set — so consumers compile against the
  already-published legacy-method shape. `publish-packages.yml` publishes on push to `main` only, so no
  PR-time publish can exist to build a consumer against the new shape before merge. This is the
  `dotnet:package-cutover` "expand merge, structural red" case, which requires expand and sync as separate
  merges.
