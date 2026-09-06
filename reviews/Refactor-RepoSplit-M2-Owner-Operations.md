# Code review — Refactor/RepoSplit-M2-Owner-Operations

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `a2115afc5c061edfdc00cb5cf3b55d2e0307eda5`  `(2026-09-06)`
**Security-reviewed up to commit:** `a2115afc5c061edfdc00cb5cf3b55d2e0307eda5`  `(2026-09-06)`
**Judgment:** `changes-requested`

## Review pass — 2026-09-06 — full

**Candidate base:** `b0be763edaf36026b8a28a8acc28475900737e4c`
**Candidate head:** `a2115afc5c061edfdc00cb5cf3b55d2e0307eda5`
**Candidate branch:** `Refactor/RepoSplit-M2-Owner-Operations`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:7d2c6c393f61fd3e15ce301a1ac56a3626a17d83f583e087480de6871cf7b3a1` `(36 paths)`
**Candidate bundle:** `C:\Users\tommy\AppData\Local\Temp\concertable-review-m2-81ea7682898e4fc6bae1f195ffb721e0`
**Candidate bundle identity:** `sha256:ede65da33dafcfe0b62d5d683008f7f99e3b2bc4b7407621aa2388899f637148`
**Work-order path:** `reviews/Refactor-RepoSplit-M2-Owner-Operations.md`
**Work-order mode:** `new`
**Pass judgment:** `changes-requested`

### Findings

- [x] **M2-001 — MEDIUM — portability** — `scripts/test-owner-operations.ps1:90`
  The offline ownership gate uses `[IO.Path]::GetRelativePath`, which is unavailable in the repository's
  Windows PowerShell 5.1 fallback environment. Replace it with a compatible relative-path helper or make
  PowerShell 7 an explicit, enforced prerequisite so the advertised gate runs rather than failing at startup.
  Resolved with an explicit PowerShell 7 prerequisite, a runtime-compatible URI-based relative-path helper,
  and a module-independent SHA-256 parity implementation. The complete owner-operation gate passes under
  PowerShell 7.
- [x] **M2-002 — HIGH — validation coverage** — `.github/workflows/test.yml`
  The new owner-tool parity, rollback, containment, bootstrap-idempotency, and evaluated-reference checks are
  not invoked by any mandatory test entrypoint or workflow. Wire both owner-operation scripts into mandatory
  CI so these repository-split safety contracts cannot regress while normal CI remains green.
  Resolved by running both scripts in the always-gating `workflow-tests` job after installing the repository's
  .NET 10 toolchain. Both scripts pass locally under PowerShell 7 and the edited workflow parses as YAML.
- [x] **M2-003 — HIGH — filesystem safety** — `scripts/test-owner-operations.ps1:152`
  The reparse-point guard protects destructive migration moves and removals, but the suite tests only lexical
  `..` escape. Add a junction/symlink case that is rejected before mutation and prove an external marker is
  left untouched.
  Resolved with a platform-appropriate junction/symbolic-link fixture. The gate proves traversal is rejected
  and the external marker remains byte-for-byte unchanged before removing the link itself.
- [x] **M2-004 — HIGH — extraction correctness** — `eng/repository-split/map.yaml`
  The System extraction currently claims all of `scripts/`, including the new monorepo-only owner router,
  tooling synchronizer, and aggregate owner-operation test. Those commands assume sibling
  `api/Concertable.*` trees that do not exist after extraction. Give them an explicit dissolve/exclusion
  disposition and validate that the carved System tree receives only System-owned operation tooling.
  Resolved by excluding and dissolving the three monorepo-only commands while retaining the System-local
  module, bootstrap, and bootstrap test. The extraction-map validator now has a focused semantic gate for
  this ownership contract; it passes with zero errors and is mandatory in `split-inventory`.

Security lens: no additional findings. Destructive paths are rooted through `Resolve-OwnerPath`, reject
lexical escapes and reparse traversal, and restore the caller's process environment. The fixed local-only
service-auth value preserves the pre-existing localhost bootstrap contract and is stored through .NET user
secrets rather than in tracked runtime configuration.
