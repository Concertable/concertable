# Code review — Plan/RepoSplit-Stage4-Fleet-E2E

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `83c97871a944f7edbb1e4002d7adfc5e02ca6419`  `(2026-08-31)`
**Judgment:** `changes-requested`

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

- [ ] **FLEET-002 — MEDIUM — changed-behaviour test impact** — `api/Concertable.Search/tests/E2ETests/Concertable.Search.E2ETests.Helpers/SearchServiceExtensions.cs:39`
  The new Web and Workers metadata values have the same `IProjectMetadata` type and are forwarded positionally, but no model-level assertion proves each reaches the matching named Aspire resource. A swap still compiles and passes the current build/carve gates, then launches the wrong executable during E2E startup. Add a source-free unit test with distinct sentinel metadata that asserts `search-web` receives the Web sentinel and `search-workers` receives the Workers sentinel; also assert the source provider exposes the matching generated Search project paths.
