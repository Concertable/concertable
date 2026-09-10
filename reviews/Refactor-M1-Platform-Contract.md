# Code review — Refactor/M1-Platform-Contract

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `a479ca4a6602ee8491623718c81dc31d196010f7`  `(2026-09-08)`
**Security-reviewed up to commit:** `a479ca4a6602ee8491623718c81dc31d196010f7`  `(2026-09-08)`
**Judgment:** `approved`

## Review pass — 2026-09-08 — full

**Candidate base:** `fec92fd552de85f9a28833bd8dd0f1a04704a191`
**Candidate head:** `a479ca4a6602ee8491623718c81dc31d196010f7`
**Candidate branch:** `Refactor/M1-Platform-Contract`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:d953c51241178cef2a986bf75df3706cef234de1b951659fbf4f186364a29160` `(7 paths)`
**Work-order path:** `reviews/Refactor-M1-Platform-Contract.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

M1 stage P4, the terminal stage. Base is `fec92fd55`, the merge of AppHost Sync PR #944. The candidate is
24 insertions and 288 deletions: retirement of the platform frontend-hosting contracts, plus two repairs
found during this pass.

Lenses applied: completeness of the retirement, whether any consumer still binds a deleted symbol, whether
every host survives losing the default roster, and published-package shape.

### Findings

- [x] **P4-1 — HIGH — correctness** — `api/Concertable.Auth/src/Concertable.Auth.AppHost/AppHost.cs:13`
  `Concertable.Auth.AppHost` called `AddAuth<Projects.Concertable_Auth>` but declared no SPA roster, so it
  depended on the `WithLocalSpaClient` loop this candidate deletes. After the deletion it would have
  registered **zero** SPA clients silently, and nothing covered it: the per-host roster assertions added at
  AppHost Sync cover B2B, Customer, Payment and Search, not Auth's own host. It hosts no SPA resources, so
  an explicit empty roster is the honest declaration, matching Payment and Search. Fixed by `2bddce4f9`,
  which adds `auth.WithSpaClients([])` and a graph test that boots the real AppHost and asserts the
  restriction is on with no client keys.

  This was missed earlier in the stack by a `git grep "AddAuth("` that did not match the generic
  `AddAuth<Projects.Concertable_Auth>(` form. The exhaustive check is `find api -name AppHost.cs` with a
  pattern accepting both call forms; there are six hosts, not five.

- [x] **P4-2 — MEDIUM — correctness** — `api/tests/Concertable.AppHost.ArchitectureTests/AppHostArchitectureTests.cs:132`
  `Build_MobileUrls_ResolveThroughOwnedTunnel`, added at AppHost Sync, enumerated live Aspire annotation
  collections after `BuildAsync()`, so the application lifecycle could mutate them mid-enumeration and throw
  `Collection was modified; enumeration operation may not execute`. It is a race, not a deterministic
  failure: #944's own merge group passed and the fault then surfaced in the merge group for sync PR #956,
  which the queue builds on top of the entries ahead of it. Left unrepaired it would eject unrelated PRs at
  random. Fixed by `a479ca4a6`, which materialises the three enumerations that run after the application is
  built, matching the `.ToArray()` already used by `GetRawEnvironmentAsync` a few lines below. The two
  enumerations that run before `BuildAsync()` are deliberately untouched.

### Verified clean

- **The retirement is complete and no consumer is stranded.** Verified by exact-symbol search rather than a
  prefix match: `LocalSpaSurfaces`, `LocalSpaSurface`, `WithLocalSpaClient` and `WithLocalSpaCorsOrigins` all
  have zero surviving references outside `obj/`. An earlier loose pattern appeared to show survivors, but
  every hit was an owner roster — `B2BLocalSpaSurfaces`, `CustomerLocalSpaSurfaces`,
  `SystemLocalSpaSurfaces` — which merely end in the same word.
- **`Concertable.Frontend.Hosting` survives as a product-neutral package, which is the point of M1.** The
  candidate deletes its `AppHostExtensions` (224 lines of product knowledge: which SPAs exist, which auth
  client each uses, which ports) and keeps `FrontendResourcesExtensions`, `FrontendWorkspacePathResolver`
  and `MobileServiceEndpoint` — the generic surface and tunnel primitives the owners call. The surviving
  `PackageReference`s are therefore correct rather than leftovers. Without this split the eventual
  `platform-frontend` repository would inherit every product's SPA roster on day one.
- **Every host now declares its own roster.** Enumerated exhaustively across all six `AppHost.cs` files:
  root gets the union, B2B gets Venue/Artist/Admin, Customer gets Customer, and Auth, Payment and Search
  each declare an explicit empty roster.
- **Deleting published API is safe here because the breakage would be a compile failure.** These symbols are
  public in published packages, so a surviving consumer fails `build` and the `carve-*` package-clean gates
  rather than failing at runtime.

### E2E tier

`skip-e2e-ui`, and unlike AppHost Sync this is the mechanically correct answer rather than an accepted risk.

The live behaviour change — narrowing which OIDC clients each host registers — happened at AppHost Sync and
has already landed. This candidate only removes code that no longer has a caller. Its two failure modes are
both caught earlier and more cheaply than by a browser suite: a stranded consumer of a deleted public symbol
is a compile error caught by `build` and the carve gates, and a host left without a roster is caught by the
per-host graph tests plus the one added here for Auth. API E2E is retained because the queue is the only
place the composed graph boots against live main.

### Security pass

`.github/workflows/` is untouched, and the Auth paths in this candidate delete a registration helper rather
than add one. The security-relevant question is whether removing the default roster can widen access: it
cannot, because the removal can only reduce a host's client set, and the one host that would have been left
with an implicitly empty set is now explicit. Fail-closed direction throughout. Scopes, PKCE, client
secrets, token lifetimes, the `ServiceClient` secrets and the E2E `concertable-test` gate are untouched.

### Validation

- `Concertable.Auth.ArchitectureTests` and `Concertable.AppHost.ArchitectureTests` both build clean with the
  two repairs.
- Exact-head PR CI and the merge queue, including API E2E against live main, are the authoritative gates.
