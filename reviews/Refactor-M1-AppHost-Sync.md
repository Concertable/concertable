# Code review — Refactor/M1-AppHost-Sync

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `832bd0e1232b326245782a993b5d75ec825f34bd`  `(2026-09-08)`
**Security-reviewed up to commit:** `832bd0e1232b326245782a993b5d75ec825f34bd`  `(2026-09-08)`
**Judgment:** `approved`

## Review pass — 2026-09-08 — full

**Candidate base:** `ed5c0fce602fc6a2e9aaa65cfe74970c51dc7c90`
**Candidate head:** `832bd0e1232b326245782a993b5d75ec825f34bd`
**Candidate branch:** `Refactor/M1-AppHost-Sync`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:d79c169e37a6212bfc62e31051cce8f087a652771af20c5174a3ab9374a4d996` `(14 paths)`
**Work-order path:** `reviews/Refactor-M1-AppHost-Sync.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

M1 stage P3, delivered after P2 PR #943 landed at `e86a73b06` and published packages
`0.1.0-alpha.0.1332` plus Auth image `sha256:06a295ad`. Base is the post-platform-sync main `ed5c0fce6`.

Lenses applied: Auth client registration correctness per host, roster coverage across every AppHost,
callback ordering, digest pinning, and test coverage of the narrowing.

### Findings

No findings. The following were examined and are clean:

- **This is the stage where the allowlist becomes live, and every host declares one.** All five AppHosts
  call `WithSpaClients`: B2B gets Venue/Artist/Admin, Customer gets Customer, the root system host gets
  `SystemLocalSpaSurfaces.AuthClients` — the union, which reproduces base `LocalSpaSurfaces.Authenticated`
  exactly — and Payment and Search pass an explicit empty roster `WithSpaClients([])`. No host is left
  relying on an implicit default, which is what makes P4's deletion of that default safe rather than
  hopeful. Verified by enumerating every `AppHost.cs` that calls `AddAuth`, not by sampling.
- **Callback ordering is sound.** `AddAuth` registers its four `WithLocalSpaClient` callbacks first, then
  the host's `WithSpaClients` callback clears every inherited `Auth__SpaClients__` key and rewrites the
  restricted set. Aspire runs environment callbacks in registration order, and the clear plus rewrite are
  inside one callback, so no partially-applied roster is reachable.
- **The Auth.Hosting churn is mechanical.** The 99-line diff in `Concertable.Auth.Hosting/AppHostExtensions.cs`
  converts `this`-extension methods into a C# 14 `extension(IDistributedApplicationBuilder builder)` block;
  the container and project `AddAuth` overloads keep every reference, secret lookup, endpoint and
  `MobileLanIp` branch line-for-line.
- **Digest pinning is uniform and correct.** All four service AppHosts pin
  `sha256:06a295ad6fa01a223000682b0f6efbfba2d5436a8fb2ffaa2d2399526ff3ae69`, which is the Auth image tagged
  with P2's exact merge commit `e86a73b06c…`. The one surviving instance of the previous digest is a
  self-contained unit-test fixture in `ContainerImageResourceTests.cs` against a fake
  `ghcr.io/concertable/service` image, not a real pin.
- **Mobile public URL wiring replaces an implicit derivation with an explicit one.** Each host now does
  `if (builder.AddMobile…(…) is { } mobileTunnel) auth.WithMobilePublicUrl(mobileTunnel.GetEndpoint(auth, "https"))`,
  so `Auth__PublicUrl` comes from the tunnel that is actually created rather than from a scraped
  `services__auth__https__0` value, and is set only when the tunnel exists. Dev-only, gated on `RunMobile`.
- **Coverage targets the narrowing rather than the mechanism.** `AppHost_ProductionGraphAndStrictValidation_AreValid`
  runs on all four service hosts; `AppHost_MobileGraph_ContainsOnlyB2BSurfaces` and
  `…ContainsOnlyCustomerSurfaces` assert owner isolation; `AppHost_CustomerSpaOrigin_MatchesAuthRegistration`
  pins SPA origin against Auth registration; `Build_AllFrontendSurfaces_AreOwnedAndCollisionFree` proves the
  system union is owned and collision-free; and
  `FrontendWorkspaces_ExtractedAndMonorepoLayouts_ResolveEveryProductionCandidate` covers both layouts.

### E2E tier

`skip-e2e-ui`, decided by Tommy against the mechanical default. The analysis below is retained because it
argued the other way, and the accepted risk should be on record rather than reasoned away.

The mechanical rule has a positive trigger here: `api/tests/Concertable.E2E.Source` project-references
`Concertable.B2B.AppHost` and `Concertable.Customer.AppHost`, the two hosts this stage narrows, and the
browser suites authenticate through the SPA clients those hosts register. API E2E does not substitute for
that, because `TestTokenMinter` mints tokens with `grant_type=password` and `client_id=concertable-test` — a
dedicated test client untouched by the narrowing — so it never exercises `customer-web`, `venue-web`,
`artist-web` or `admin`.

**Accepted risk:** a login path broken by a wrong roster would not be caught by any gate that runs on this
candidate, and Platform Contract then deletes the fallback that would make it diagnosable.

**What reduces it to narrow:** each host's roster matches the flows its own suite drives; the roster shape is
asserted in the fast tier by `AppHost_CustomerSpaOrigin_MatchesAuthRegistration`,
`AppHost_MobileGraph_ContainsOnly*Surfaces` and `Build_AllFrontendSurfaces_AreOwnedAndCollisionFree`, all
green; and the one genuine defect in this stack — Auth's own AppHost silently losing every SPA client once
the default is retired — was found by enumerating hosts, not by any E2E suite, and is fixed on Platform
Contract with its own graph test.

### Security pass

Independent security review over `ed5c0fce6..832bd0e12` found **no HIGH or MEDIUM findings**. It verified
every host's client set is equal to or a strict subset of base, that the empty roster is fail-closed (empty
means none, not all), that callback ordering cannot invert so no stale-but-trusted registration survives, that
the Auth digest is uniform across all four container hosts, and that the production redirect URIs in
`appsettings.json` are structurally unreachable because any client whose environment keys were cleared is also
absent from `EnabledClients`. `Auth__PublicUrl` resolving to a dev tunnel under `RunMobile` changes the issuer
but not redirect validation or token issuance, and base already derived the same value from the
tunnel-rewritten service URL.

### Validation

- Exact-head PR CI green at `832bd0e12`: 82 pass, 5 skipping, 0 failures, including `build`, the
  `carve-auth`/`carve-b2b`/`carve-customer` package-clean gates, `container-images`, and the architecture,
  unit and integration matrices.
- The local compile floor through `scripts/local-platform.ps1 build api/Concertable.slnx --configuration Release`
  succeeded with 0 errors against 58 source-built platform packages.
- Merge-queue full E2E against live main is the authoritative remaining gate.
