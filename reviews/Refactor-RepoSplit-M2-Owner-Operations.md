# Code review — Refactor/RepoSplit-M2-Owner-Operations

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `5c94815a0046a28d8b539b06e76ed04513039eac`  `(2026-09-08)`
**Security-reviewed up to commit:** `5c94815a0046a28d8b539b06e76ed04513039eac`  `(2026-09-08)`
**Judgment:** `approved`

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
  root-level destination. The touched map and generated inventory now use the authoritative
  `platform-frontend` target name.
- [x] **M2-006 — LOW — plan resumability** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_FOUNDATION_PROGRESS.md:22`
  The checkpoint describes a three-commit head and tells the next owner to commit review repairs already
  present in this candidate. Record the actual fixing head and make final review/publication the resume action.
  Resolved by recording the five-commit stack through `7a561adbe`, distinguishing the two current final-pass
  corrections, and making their incremental review plus draft publication the sole resume action.

This is a new full pass because the exact #633 restack rewrote the earlier watermark's ancestry; the prior
candidate descriptor, findings, severities, and dispositions remain preserved above. Security lens: no new
findings in the CI, extraction-map, test, or documentation remediation delta.

## Review pass — 2026-09-06 — incremental

**Candidate base:** `7a561adbe1d5f32a0e6a562323859fa4039117a8`
**Candidate head:** `d98622e69008cce293806095841ab28f482c4647`
**Candidate branch:** `Refactor/RepoSplit-M2-Owner-Operations`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:c81927bcc0309d726465c224759589549c1d95db76bb77c5503dcec936ae4a66` `(6 paths)`
**Candidate bundle:** `C:\Users\tommy\AppData\Local\Temp\concertable-review-m2-incremental-final2-65537ca2c85c40e4b0f443ff2e1bdf3f`
**Candidate bundle identity:** `sha256:1fabdc58f220e2062d8f2df9a8b70c3f2d7be9ba2a5ffbf50ca18e18d6c782f9`
**Work-order path:** `reviews/Refactor-RepoSplit-M2-Owner-Operations.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No findings. The exact platform-tool rename precedes and escapes the generic `api/` rename, the focused map
gate asserts the resulting root-level path, and the generated frontend inventory consistently uses the
authoritative `platform-frontend` target. The checkpoint now resumes at publication rather than replaying
already committed repairs. Security lens found no new risk in this metadata, validator, or documentation delta.

## Review pass — 2026-09-06 — incremental publication checkpoint

**Candidate base:** `d98622e69008cce293806095841ab28f482c4647`
**Candidate head:** `97b447bcfa679be55d4c6ec666b5909c287ceb5c`
**Candidate branch:** `Refactor/RepoSplit-M2-Owner-Operations`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:1d12273e008395251ba1d9f7f9b4fb4f226077a9e40fd4d5266ff59383441ab8` `(2 paths)`
**Candidate bundle:** `C:\Users\tommy\AppData\Local\Temp\concertable-review-m2-publication-eb80be6260a14bcd932a40dc6bf74872`
**Candidate bundle identity:** `sha256:e1e14c65ec562a9249ed2de23c4d92f9c4960d2474d88a7dfc9421a6e6a7540e`
**Work-order path:** `reviews/Refactor-RepoSplit-M2-Owner-Operations.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No findings. The tracked review transition and PR #947 checkpoint accurately record the published draft,
explicit #633 base, Docker resume condition, and completed M2 review state. Security lens found no risk in
the review-only and plan-only tail.

## Review pass — 2026-09-08 — incremental current-main merge resolution

**Candidate base:** `7ec03757b590ad593dab52009bf64902661ce2e4`
**Candidate head:** `5c94815a0046a28d8b539b06e76ed04513039eac`
**Candidate branch:** `Refactor/RepoSplit-M2-Owner-Operations`
**Candidate scope:** `eng/repository-split/map.yaml, plans/platform/POLYREPO_ROADMAP.md, plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_FOUNDATION_PROGRESS.md, plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md, scripts/test-owner-operations.ps1`
**Candidate path-set:** `sha256:c510a91f0a258e49fb1b7b0a4f8b8a5c4907e34ceb72f15cddccd6b9658555b5` `(5 paths)`
**Candidate bundle:** `C:/Users/TOMMYS~1/AppData/Local/Temp/claude/C--Users-TommySeery-source-repos-Concertable--worktrees-Refactor-RepoSplit-M2-Owner-Operations/53d78a68-154d-489a-b5ec-e6d8655ec00c/scratchpad/review-m2-merge-0f1429b22e314038a1491df3fb29289f`
**Candidate bundle identity:** `sha256:f4dcfdb4294b9be7c6e49451aed8fa8481526f07b5930b7d72856194c57daa4f`
**Work-order path:** `reviews/Refactor-RepoSplit-M2-Owner-Operations.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

The recorded watermark `97b447bcf` is not an ancestor of this head: the landed-main restack rewrote that
ancestry, and its live equivalent is `7ec03757b`. This pass therefore bases on `7ec03757b` and re-stamps both
top-level markers onto live ancestry. Every earlier pass descriptor, finding, severity and disposition is
preserved unchanged above.

The candidate is bounded because a merge head's full range is dominated by landed main. It is exactly the
union of M2's own commits since that base (`c74ca5f3a`, `42b035c1c`, `73ef5db31`) and the paths the three
`origin/main` merges resolved rather than took verbatim from a parent.

### Findings

- [x] **M2-007 — LOW — needless indirection** — `scripts/test-owner-operations.ps1:25`
  `Get-CompatibleRelativePath` existed only because `[IO.Path]::GetRelativePath` is absent from Windows
  PowerShell 5.1. `73ef5db31` replaced its URI implementation with that very API, leaving a single-call-site
  forwarding wrapper whose name advertises a constraint the script's `#requires -Version 7.0` already rules
  out, and whose `GetFullPath` calls duplicate what `GetRelativePath` performs internally. Inline the
  framework call.
  Resolved by `5c94815a0`: the helper is deleted and its one call site calls
  `[IO.Path]::GetRelativePath($source, $template.FullName)` directly. Both arguments were already absolute.
  The complete owner-operation gate passes after the change.

No other findings. `73ef5db31` is correct: `[IO.Path]::GetRelativePath` is the cross-platform API, and the
`[Uri]::MakeRelativeUri` form it replaced genuinely mis-handles absolute POSIX paths, which is what stopped
the Linux gate. The `POLYREPO_ROADMAP.md` resolution keeps main's stream ordering and restores only the M2
detail main's row dropped. The foundation ledger is taken from main byte-for-byte, so M2 contributes nothing
to the M1 stream's live state and cannot collide with it again. The `map.yaml` auto-merge is correct in both
directions: M2's three System exclusions, their matching dissolves and the exact `OwnerOperations.psm1`
rename all survive, and main's Search and AppHost.Shared note rewrites are taken. Main added no `scripts/`
entrypoint, so the exclusion set is still complete, and no landed context escaped the manifests — the gate
still proves 24. The `REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md` auto-merge keeps M2's delegator sentence
and main's expanded B2B packable roster.

Security lens: no findings. The candidate touches one test script and four plan documents. Every
security-sensitive path in M2's contribution — `.github/workflows/test.yml`, and the Auth and Payment
`initial-migrations.ps1`, `migrations.psd1`, `setup-local-dev.ps1` and `tools/OwnerOperations.psm1` — is
byte-identical to its blob at the previously security-approved `97b447bcf`, so that approval carries forward
and the re-stamp records the same verified state at a live SHA.
