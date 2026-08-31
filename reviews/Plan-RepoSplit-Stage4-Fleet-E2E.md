# Code review — Plan/RepoSplit-Stage4-Fleet-E2E

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `80f6da637b9fca909404f01741d74908062d8744`  `(2026-08-31)`
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
