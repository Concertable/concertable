# Code review — Refactor/RepoSplit-M2-Owner-Operations

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `7a561adbe1d5f32a0e6a562323859fa4039117a8`  `(2026-09-06)`
**Security-reviewed up to commit:** `7a561adbe1d5f32a0e6a562323859fa4039117a8`  `(2026-09-06)`
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

## Review pass — 2026-09-06 — full after upstream restack and remediation

**Candidate base:** `ad4ad986f4f61f328ec9aae14a5fec1ccde364db`
**Candidate head:** `7a561adbe1d5f32a0e6a562323859fa4039117a8`
**Candidate branch:** `Refactor/RepoSplit-M2-Owner-Operations`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:95f5d610e302b8ba1f889ed79cebe2f030f4c7b4c962f66c3e39b231f807f2a5` `(41 paths)`
**Candidate bundle:** `C:\Users\tommy\AppData\Local\Temp\concertable-review-m2-final-d266c107aa7246afb4c46a2c430aa1de`
**Candidate bundle identity:** `sha256:daff06c88343d2c3e126021e41ee72d6148bc840fc71408b35991cc10fbceda8`
**Work-order path:** `reviews/Refactor-RepoSplit-M2-Owner-Operations.md`
**Work-order mode:** `append`
**Pass judgment:** `changes-requested`

### Findings

- [x] **M2-005 — MEDIUM — extraction correctness** — `eng/repository-split/map.yaml:95`
  The module and local-development documentation declare its canonical destination as root-level
  `tools/OwnerOperations.psm1` in `platform-dotnet`, but the extraction map's generic `api/` rename places it
  at `src/Concertable.Shared/tools/OwnerOperations.psm1`. Align and validate the actual rename destination.
  Resolved with an exact path rename before the generic `api/` rule and a validator assertion for the final
  root-level destination. The same touched map now uses the authoritative `platform-frontend` target name.
- [x] **M2-006 — LOW — plan resumability** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_FOUNDATION_PROGRESS.md:22`
  The checkpoint describes a three-commit head and tells the next owner to commit review repairs already
  present in this candidate. Record the actual fixing head and make final review/publication the resume action.
  Resolved by recording the five-commit stack through `7a561adbe`, distinguishing the two current final-pass
  corrections, and making their incremental review plus draft publication the sole resume action.

This is a new full pass because the exact #633 restack rewrote the earlier watermark's ancestry; the prior
candidate descriptor, findings, severities, and dispositions remain preserved above. Security lens: no new
findings in the CI, extraction-map, test, or documentation remediation delta.
