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
- [ ] `auth-identity-model/extension-block-syntax` — two related fixes to the typed model's shape, both
  blocked on the same publish-first constraint below. Scope revised 2026-09-11 evening (a design discussion
  found more than the original extension-block gap):
  - `AuthScope`/`AuthScopes` and `AuthResource`/`AuthResources` — convert the legacy `this`-parameter
    extension methods to C# 14 `extension()` blocks (`csharp-style` requirement, missed by both Phase 1
    review rounds). These two have no public row struct (nothing needs the whole row, every call site wants
    one derived fact — `.Id()`, `.Audience()`), so they keep the extension-method shape, just modernised.
  - `InteractiveClient`/`InteractiveClientInfo` and `ServiceClient`/`ServiceClientInfo` — a genuine redesign,
    not just a syntax swap:
    - `InteractiveClientInfo`/`ServiceClientInfo` become `sealed record class`, not `readonly record struct`.
      Reason: their all-zero default is not a harmless empty value (`Client = CustomerBrowser` — enum `0` —
      with `Id = null` looks like real data for the wrong client), so a struct's silent `default` is a real
      footgun here; a class's `null` is an honest, un-mistakable "nothing".
    - `InteractiveClients`/`ServiceClients` (the separate static catalog classes) are deleted. `Get`/
      `GetOrDefault`/`All` become static members directly on the record itself — a struct/class can hold its
      own statics without needing a companion extension-method class; only the removed `client.Info()`
      fluent-sugar actually required one, and that sugar is the price paid for one fewer type.
    - `MobileScheme`/`IsMobile` come off `InteractiveClientInfo` entirely — grepped every consumer: only
      `Concertable.Auth`'s own `Config.cs` ever reads them, so publishing them on the *shared* contract
      coupled every external consumer's package to an Auth-internal reconstruction detail nothing else
      needs. The scheme goes back to being a plain literal parameter at each `Config.XMobileClient()` call
      site, exactly as it was before `ClientIds` existed — it never needed a lookup table, local or shared.
    - `Find` → `GetOrDefault` (BCL's `GetValueOrDefault` naming, now accurate since the class makes `null`
      the real "not found" value — it wasn't accurate for the struct, where `GetValueOrDefault` would have
      implied a non-nullable, always-safe return). `Of` was considered and rejected (invents a word); a
      `TryGetValue(out)` shape was considered and rejected even for the class (doesn't compose with this
      codebase's existing `is { } x` / `is not { } x` pattern-matching style at every call site).
    - `All` drops to `internal` on the `InteractiveClient` side (`InternalsVisibleTo` for its own test
      project) — grepped every consumer: nothing outside its own unit tests calls it, not even Auth's own
      `Config.cs`, which builds its client list by hand. `ServiceClients.All` stays as-is — it has a real
      production consumer (`AuthHostExtensions.cs`) and never leaves the service since it's unpublished.

  **This work belongs in `Concertable/auth`, not this monorepo.** PR #1016 ("Stop the monorepo publishing
  Auth's package ids", merged 2026-09-11 `c778b6adb`) moved Auth to `PROMOTED_TARGETS` in
  `.github/scripts/package_ownership.py` — Auth's source still physically lives here
  (`api/Concertable.Auth.Contracts/`; removing it is a later, separate step, "10F"), and the monorepo still
  builds/packs it, but `publish-packages.yml`'s `filter` step now unlinks Auth's artifacts before the push,
  so nothing built from this copy ever reaches the feed. Auth publishes canonically from its own
  `Concertable/auth` repository now (first service to do so). Editing the monorepo copy is dead work with no
  publish path. Blocked on `Concertable/auth`'s own polyrepo cutover finishing — hold until that's confirmed
  stable.

  **Do not attempt the producer and consumer halves in one PR — it was tried and cannot build**, and the
  same constraint applies once this moves to `Concertable/auth`: every consumer restores
  `Concertable.Auth.Contracts` as a `PackageReference`, never a `ProjectReference`, so a consumer only sees a
  new shape after it publishes and the consumer's pin moves past it. Converting the four containers and
  updating every call site in one PR left `Concertable.Auth.Contracts` building with its own tests green and
  `Concertable.Auth` failing with 31 `CS0119`/`CS1503` errors at every `.Id`/`.Info`/`.Audience`/
  `.AcceptedScopes`/`.IncludedClaims` site — the `dotnet:package-cutover` "expand merge, structural red"
  case, which requires expand and sync as separate merges regardless of which repo owns the package.
