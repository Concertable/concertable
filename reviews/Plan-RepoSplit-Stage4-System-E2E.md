# Code review — Plan/RepoSplit-Stage4-System-E2E

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `fef64122e676a7dd78b89a48c36b6c02dd7fe970`  `(2026-08-31)`
**Security-reviewed up to commit:** `fef64122e676a7dd78b89a48c36b6c02dd7fe970`
**Judgment:** `approved`

## Review pass — 2026-08-31 — full

**Candidate base:** `cf0da4c9b3e678a198a57968e9ad2bfc80b74e46`
**Candidate head:** `83c97871a944f7edbb1e4002d7adfc5e02ca6419`
**Candidate branch:** `Plan/RepoSplit-Stage4-Fleet-E2E`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:6d42eeb1bcdfc98c8a40d6dd0139c8de8f1f1046981d0f356472202fdf303d02` `(8 paths)`
**Candidate bundle:** `C:\Users\tommy\AppData\Local\Temp\concertable-review-stage4-fleet-83c97871a`
**Candidate bundle identity:** `sha256:6a2e969e460a7175a210d9479f3fa8d2421a4fe255a2bd2035d0f5ebbb53532e`
**Work-order path:** `reviews/Plan-RepoSplit-Stage4-Fleet-E2E.md`
**Work-order mode:** `new`
**Pass judgment:** `changes-requested`

### Findings

- [x] **FLEET-001 — LOW — language/framework conventions** — `api/Concertable.Search/tests/E2ETests/Concertable.Search.E2ETests.Helpers/SearchServiceExtensions.cs:22`
  This edited extension container still declares `AddSearchService` with legacy `this` syntax. The routed C# style rule requires every ordinary member in a touched extension container to use the C# 14 `extension(...)` form. Migrate the single method without changing its behavior.
  Resolved by migrating the container's single method to `extension(IDistributedApplicationTestingBuilder builder)`; the fleet source-provider build passed with zero errors.
  The later metadata-routing fix generalized the receiver to `IDistributedApplicationBuilder` while retaining the required C# 14 extension-block form.

- [x] **FLEET-002 — MEDIUM — changed-behaviour test impact** — `api/Concertable.Search/tests/E2ETests/Concertable.Search.E2ETests.Helpers/SearchServiceExtensions.cs:39`
  The new Web and Workers metadata values have the same `IProjectMetadata` type and are forwarded positionally, but no model-level assertion proves each reaches the matching named Aspire resource. A swap still compiles and passes the current build/carve gates, then launches the wrong executable during E2E startup. Add a source-free unit test with distinct sentinel metadata that asserts `search-web` receives the Web sentinel and `search-workers` receives the Workers sentinel; also assert the source provider exposes the matching generated Search project paths.
  Resolved with a source-free Search helper model test (1/1) and fleet source-provider path tests (2/2). The regenerated package-only carve includes and passes the Search model test while all service `src` folders and `Concertable.Fleet.E2E.Source` remain absent.

## Review pass — 2026-08-31 — incremental

**Candidate base:** `83c97871a944f7edbb1e4002d7adfc5e02ca6419`
**Candidate head:** `80f6da637b9fca909404f01741d74908062d8744`
**Candidate branch:** `Plan/RepoSplit-Stage4-Fleet-E2E`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:4e14d24336641983fc96287a11c8678b301242e0d434b02107c8117985e1f1a1` `(13 paths)`
**Candidate bundle:** `C:\Users\tommy\AppData\Local\Temp\concertable-review-stage4-fleet-inc-80f6da637`
**Candidate bundle identity:** `sha256:5e9fc1d4ee028e7899793426b7014fef100ecc9850acc8be5561fb9cf19c683a`
**Work-order path:** `reviews/Plan-RepoSplit-Stage4-Fleet-E2E.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No new findings. Native/general review and the parent testing/conventions check accepted the fixing delta; the separately dispatched testing lens timed out and contributed no evidence.

## Review pass — 2026-08-31 — incremental

**Candidate base:** `80f6da637b9fca909404f01741d74908062d8744`
**Candidate head:** `79629476fd18d7eab478404d8ed128a91758adb0`
**Candidate branch:** `Plan/RepoSplit-Stage4-Fleet-E2E`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:fdb1cde7a230f1b20e432ac614d2f87fed06ba68ff6975c3e3ae179214fe7bc1` `(2 paths)`
**Candidate bundle:** `C:\Users\tommy\AppData\Local\Temp\concertable-review-stage4-fleet-final-79629476f`
**Candidate bundle identity:** `sha256:55d25560b984cad3741d66ce7d84c871b3854c69d64b90b34f838e3416bc469c`
**Work-order path:** `reviews/Plan-RepoSplit-Stage4-Fleet-E2E.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No new findings. The parent consistency review accepted the ledger and review-work-order checkpoint; the separately dispatched native review timed out and contributed no evidence.

## Review pass — 2026-08-31 — incremental

**Candidate base:** `79629476fd18d7eab478404d8ed128a91758adb0`
**Candidate head:** `9449f9bd4c950794e5695bf13f878ccbe9510a0`
**Candidate branch:** `Plan/RepoSplit-Stage4-Fleet-E2E`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:c8045bc89d637f0ca58e1d4ea237cb6ae38cca1e4c36f30b30dcf0ca25a10636` `(17 paths)`
**Candidate bundle:** `C:\Users\tommy\AppData\Local\Temp\concertable-review-stage4-fleet-final-9449f9bd4`
**Candidate bundle identity:** `sha256:9f3f36e554fa6453f02f07f838419ae781a85290ea88f34bcf58e9fe631fce70`
**Work-order path:** `reviews/Plan-RepoSplit-Stage4-Fleet-E2E.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No new findings. Independent native/general and security/boundary lenses validated the immutable bundle. The
16 DataAccess, Payment, and review paths are inherited unchanged from current `origin/main`; the only
post-merge Stage 4 change is the accurate fleet-ledger reconciliation, and no boundary or integration
interaction was introduced.

## Review pass — 2026-08-31 — incremental

**Candidate base:** `9449f9bd4c950794e5695bf13f878ccbe9510a0`
**Candidate head:** `da10657b351c7526647b13c0c0516fa66d857510`
**Candidate branch:** `Plan/RepoSplit-Stage4-Fleet-E2E`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:4223088f7490b5b573aa2a2742115ed1ad2a7d2555f1a1ded7bb76ca82927298` `(3 paths)`
**Candidate bundle:** `C:\Users\tommy\AppData\Local\Temp\concertable-review-stage4-fleet-final-da10657b3`
**Candidate bundle identity:** `sha256:524811e5d4298f1f5b7a0784251e32578955594f23f0a4d7ced726066721ebcb`
**Work-order path:** `reviews/Plan-RepoSplit-Stage4-Fleet-E2E.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No new findings. Independent native/general and documentation/boundary lenses validated the immutable
bundle. The two excluded progress ledgers exactly match incoming `origin/main`; the sole branch-authored
change updates only the Stage 4 ledger's reconciliation SHA, and the Search fleet slice remains unchanged.

## Review pass — 2026-08-31 — incremental

**Candidate base:** `da10657b351c7526647b13c0c0516fa66d857510`
**Candidate head:** `2984f59aac5ebb91e161c59251d81edfe36a6e89`
**Candidate branch:** `Plan/RepoSplit-Stage4-Fleet-E2E`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:a8b6a4e791a64c7b5b82834927821be463a33219fa3d3267a4b01a654df3fa70` `(4 paths)`
**Candidate bundle:** `C:\Users\tommy\AppData\Local\Temp\concertable-review-stage4-fleet-rename-2984f59aa`
**Candidate bundle identity:** `sha256:074de2f08a3c33230f5603417796c53a9680591d27d2c05cc39a729cec97de21`
**Work-order path:** `reviews/Plan-RepoSplit-Stage4-Fleet-E2E.md`
**Work-order mode:** `append`
**Pass judgment:** `changes-requested`

### Findings

- [x] **FLEET-003 — MEDIUM — verification accuracy** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_STAGE4_FLEET_PROGRESS.md:57`
  The ledger describes the local package feed and package-only test evidence as exact-head, but those artifacts were generated before the extension-container rename at `2984f59aa`. Regenerate the local package feed and package-only carve at this head, rerun the focused tests, and update the recorded package version and results.
  Resolved in `fef64122e`: generated 55 packages as `0.1.0-local.1788198957447`, rebuilt the package-only carve with zero errors, and passed Search 1/1, Payment 6/6, and fleet source-provider 2/2.

## Review pass — 2026-08-31 — incremental

**Candidate base:** `2984f59aac5ebb91e161c59251d81edfe36a6e89`
**Candidate head:** `fef64122e676a7dd78b89a48c36b6c02dd7fe970`
**Candidate branch:** `Plan/RepoSplit-Stage4-Fleet-E2E`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:7b68f580cf7f575050038c1b9145cfd40c536e0112c13ab24d30452bbb66d04a` `(1 paths)`
**Candidate bundle:** `C:\Users\tommy\AppData\Local\Temp\concertable-review-stage4-fleet-remediation-fef64122e`
**Candidate bundle identity:** `sha256:e91ba098d7e621de0e0dd71270fec2dd810225b3f4a02cae0c2b5da64dbb0da4`
**Work-order path:** `reviews/Plan-RepoSplit-Stage4-Fleet-E2E.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No new findings. Independent native/general and verification-accuracy lenses validated the immutable
remediation bundle and confirmed that the regenerated package, carve, and focused test evidence fully
resolves FLEET-003.
