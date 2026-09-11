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
