# Auth identity model — roadmap

Replace the flat `ClientIds` / `ApiScopeIds` string-constant classes in `Concertable.Auth.Contracts` with a
typed model — `AuthParty`, `InteractiveClient` / `InteractiveClients`, `AuthScope` / `AuthScopes`,
`AuthResource` / `AuthResources` — so every OIDC client id and API scope has one home, the wire strings live
in exactly one catalog each, and the three cross-service handlers that classify a registration client id
switch on `AuthParty` instead of matching hand-maintained string sets.

## Items

- [ ] `auth-identity-model/typed-identity-contract` — add the typed model to the published
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
  `ConcertableAuthVersion` pin moves past it. Attempted inside Phase 2 alongside consumer migration and
  reverted — see `AUTH_IDENTITY_MODEL_PROGRESS.md`'s "extension-block conversion was attempted and reverted"
  entry for why the combined shape cannot build.
