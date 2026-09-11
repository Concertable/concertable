# Code review — Refactor/AuthIdentityModel

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `7fb9b2550b9dea7b955855ab37a312743a7b8624`  `(2026-09-10)`
**Security-reviewed up to commit:** `7fb9b2550b9dea7b955855ab37a312743a7b8624`  `(2026-09-10)`
**Judgment:** `approved`

## Review pass — 2026-09-10 — full

**Candidate base:** `4db65f56dc4a3734eb502de7ff2d7562087096a5`
**Candidate head:** `7fb9b2550b9dea7b955855ab37a312743a7b8624`
**Candidate branch:** `Refactor/AuthIdentityModel`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:4c8db6aa37a49ff426da3eb5bcd221c3b469b58a4ea68d77a69aae8a9dc8c2ec` `(26 paths)`
**Candidate bundle:** `n/a — parent-run review over the frozen head in the worktree; findings addressed on-branch under --fix authorization`
**Candidate bundle identity:** `n/a`
**Work-order path:** `reviews/Refactor-AuthIdentityModel.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

Phase 1 of `plans/auth-identity-model` — additive typed identity model in the published
`Concertable.Auth.Contracts` package (`AuthParty`, `InteractiveClient`/`InteractiveClients`,
`AuthScope`/`AuthScopes`, `AuthResource`/`AuthResources`), `[Obsolete]` on the old `ClientIds` /
`ApiScopeIds`, plus a co-located unit-test project and carve wiring. Reviewed over three iterations as the
fixes landed (findings A–D below); this pass is stamped at the head that carries all of them.

Every catalog value was traced against the live `Config.cs` (`ApiScopes` / `ApiResources` / `TestClient` /
web + mobile clients) and the four resource-server host extensions — all wire client ids, scope strings,
audiences, mobile schemes, accepted scopes and included user claims match exactly. No
`TreatWarningsAsErrors` anywhere in the repo, so the new `[Obsolete]` attributes warn but do not break
consumers. Carve wiring (`test.yml` project list, `.slnx`, regenerated `inventory.json`, glob-exclude of
`tests/**` in the flat package csproj, co-located `ProjectReference` that resolves in the `carve-auth`
tree) verified; `split-inventory --check` reports no cross-repository test edge.

### Findings

- [x] **A — HIGH — correctness** — `api/Concertable.Auth.Contracts/InteractiveClients.cs:38`
  `InteractiveClients.Find` threw `ArgumentNullException` on a null client id, contradicting its own doc
  ("returns null rather than throwing"). A `CredentialRegisteredEvent` with an absent/null `ClientId`
  would fault the Phase 2 handler and dead-letter the message instead of skipping it. Fixed: `Find(string?)`
  guards null and returns null; test covers `null` / `""` / unknown. *(commit 7fb9b2550)*
- [x] **B — MEDIUM — conventions** — `api/Concertable.Auth.Contracts/Directory.Build.targets:5`
  Imported only `TestConventions.targets`, omitting the `PlatformSourcePackages.targets` import every
  sibling carve root carries; a later platform `PackageReference` on the test project would silently miss
  the in-repo source swap. Fixed: added the import. *(commit 7fb9b2550)*
- [x] **C — MEDIUM — correctness** — `api/Concertable.Auth.Contracts/AuthResources.cs`
  Catalog reverse-lookups (`AuthResources.TryGet`, `AuthScopes.TryGet`, `InteractiveClients.TryGet`)
  handed back a `default(struct)` on a miss whose `ImmutableArray` members throw `NullReferenceException`
  when read — a trap in a published contract. Fixed: dropped every reverse lookup with no consumer;
  `InteractiveClients.Find` returns `InteractiveClientInfo?`; `AuthResourceInfo` collapsed into extension
  methods on `AuthResource` so no contract struct carries collections. *(commit 3bc6f5b3a)*
- [x] **D — LOW — conventions** — `api/Concertable.Auth.Contracts/tests/Concertable.Auth.Contracts.UnitTests/`
  Redundant `using Concertable.Auth.Contracts;` in the three test files (the test namespace is nested
  inside it). Fixed: removed. *(commit 3bc6f5b3a)*

### Verified, no finding

- **Correctness.** Catalog construction is a `FrozenDictionary` over compile-time literals; a duplicate
  key throws at first access, and the tests exercise `.All` on every catalog plus completeness, value,
  round-trip and `IsB2b`/`IsMobile` assertions. `InteractiveClientInfo.Party` is `AuthParty?` (null only
  for `E2ETest`); `IsB2b` reads `Party is Venue or Artist or Admin` — correct against the live
  `ManagerClientIds` set and `TenantTypeByClientId` dict this replaces.
- **Wire compatibility.** Every wire string is byte-identical to the value in the `ClientIds` /
  `ApiScopeIds` classes being obsoleted; enum member names (`CustomerBrowser` → `"customer-web"`) are
  deliberately decoupled and the catalog is the only bridge. `CredentialRegisteredEvent.ClientId` stays
  `string` on the wire.
- **Additive-only.** Nothing outside `Concertable.Auth.Contracts.csproj` and the new test project changes
  runtime behaviour; consumers still bind `ClientIds`/`ApiScopeIds`. The breaking removal + consumer
  migration is Phase 2, gated on this republishing.
- **`csharp-style` / `csharp-naming` re-checked against every changed `.cs`:** file-scoped namespaces,
  no underscore prefixes, sealed test classes, `[Fact]`/`[Theory]` with expected-last, AAA blank-line
  separation, one-line doc comments — all conform.

### Security layer

Qualifying paths: `api/Concertable.Auth.Contracts/**` (matches `(^|/)Concertable\.Auth` and
`\.Contracts(/|\.)` in `.agents/merge-gate.json`), `.github/workflows/test.yml` (generic CI-workflow
pattern in the hook).

**Findings:** none.

- **No secrets, credentials, keys or hashes** in the diff. Every value is a public OIDC client identifier
  or OAuth scope name — already published in discovery metadata and already present in the classes being
  obsoleted. Client *secrets* remain in `AuthHostExtensions` / configuration, untouched, and are
  deliberately kept out of this published contract (`ServiceClient` stays in `Concertable.Auth` — Phase 2).
- **No auth decision logic.** No token issuance, validation, claims transformation or authorization check
  is added or changed; Duende still owns all of it. The catalogs are inert data + lookup.
- **Auth-semantic risk (catalog values diverging from the Duende registration)** is verified exact by the
  code pass and does not activate until Phase 2's separately-reviewed consumer migration — this PR changes
  no running code.
- **`.github/workflows/test.yml`** — one line adding a project path to the `carve-auth` in-repo slnx list.
  No permissions, secrets, triggers or actions changed.
