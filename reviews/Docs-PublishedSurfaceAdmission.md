# Code review — Docs/PublishedSurfaceAdmission

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `0ad659386004e3809326aca2341d2b9a5a8c5d74`  `(2026-09-11)`
**Judgment:** `approved`

## Review pass — 2026-09-11 — docs

**Candidate base:** `d01a2a4857c10be8e1e9ca27f3b2e18543d2c38e`
**Candidate head:** `e5432d5f04708db458ebed9d6d28b1d6b252a605`
**Candidate branch:** `Docs/PublishedSurfaceAdmission`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:f86c067aa960333d4724e4d48fe533763dd56b5d3e2abb4585238f2d7c6e0322` `(3 paths)`
**Candidate bundle:** `C:\Users\TommySeery\AppData\Local\Temp\concertable-review-Docs-PublishedSurfaceAdmission-e5432d5f0`
**Candidate bundle identity:** `sha256:354569b4cc6bad2c99c9a2f65ed21a76e2653c8af07f6984a510fa598e58d8da`
**Work-order path:** `reviews/Docs-PublishedSurfaceAdmission.md`
**Work-order mode:** `new`
**Pass judgment:** `changes-requested`

### Findings

- [x] **DOCS-001 — MEDIUM — plan consistency** — `plans/platform/PLATFORM_RELEASE_TRAINS_PLAN.md:79`
  Phase 3 extended release trains to every packable project, contradicting the binding admission table's
  vendor removals and System ownership. Resolved in `62581f6b4009b98f4c09cd7c6305161ef5b858c2` by making the
  admitted table the exact package set and excluding every vendor row from pack and push.
- [x] **DOCS-002 — MEDIUM — ownership precision** — `plans/platform/PUBLISHED_SURFACE_ADMISSION_PLAN.md:123`
  Eleven vendored `Shared.*` rows assigned ownership to the unstable phrase `consuming services` rather
  than naming the repositories that must take the code. Resolved in
  `62581f6b4009b98f4c09cd7c6305161ef5b858c2` by listing each exact consumer repository.
- [x] **DOCS-003 — MEDIUM — release-train naming** — `plans/platform/PUBLISHED_SURFACE_ADMISSION_PLAN.md:88`
  The B2B rows introduced `ConcertableB2BVersion`, conflicting with the existing release-train contract's
  `ConcertableB2BContractsVersion`. Resolved in `62581f6b4009b98f4c09cd7c6305161ef5b858c2` by using the
  established property consistently.
- [x] **DOCS-004 — MEDIUM — inventory accuracy** — `plans/platform/PUBLISHED_SURFACE_ADMISSION_PLAN.md:20`
  The rationale counted twelve shared adapters and five hosting packages while the binding table contained
  thirteen and six. Resolved in `62581f6b4009b98f4c09cd7c6305161ef5b858c2` by correcting both subtotals.
- [x] **DOCS-005 — MEDIUM — verdict definition** — `plans/platform/PUBLISHED_SURFACE_ADMISSION_PLAN.md:43`
  The `service-owned` definition required a service train even though `Concertable.Testing.E2E` correctly
  belongs to the System producer train. Resolved in `62581f6b4009b98f4c09cd7c6305161ef5b858c2` by explicitly
  admitting service or System producer ownership.

## Review pass — 2026-09-11 — incremental

**Candidate base:** `e5432d5f04708db458ebed9d6d28b1d6b252a605`
**Candidate head:** `62581f6b4009b98f4c09cd7c6305161ef5b858c2`
**Candidate branch:** `Docs/PublishedSurfaceAdmission`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:b42dab1a9df89f3fdde1932c10052aa8e6caa36a8c1f5b3c1aa62c59d1f13aa9` `(2 paths)`
**Candidate bundle:** `C:\Users\TommySeery\AppData\Local\Temp\concertable-review-Docs-PublishedSurfaceAdmission-62581f6b4`
**Candidate bundle identity:** `sha256:5be147d84807ff33b7286d21dcc658b5ba8b0c5ff04838b80b1af84338ae2ef0`
**Work-order path:** `reviews/Docs-PublishedSurfaceAdmission.md`
**Work-order mode:** `append`
**Pass judgment:** `changes-requested`

### Findings

- [x] **DOCS-006 — MEDIUM — release-train completeness** — `plans/platform/PLATFORM_RELEASE_TRAINS_PLAN.md:79`
  Phase 3 still referred to extending two trains even though the admission table defined seven distinct
  admitted properties. Resolved in `506da3bf9cf54310991dbd12954545ad0e9d229c` by defining one train per
  distinct non-vendor property and naming all seven.
- [x] **DOCS-007 — MEDIUM — inventory arithmetic** — `plans/platform/PUBLISHED_SURFACE_ADMISSION_PLAN.md:18`
  The revised non-contract package breakdown still overlapped `Concertable.Messaging.Contracts` and did not
  reconcile to 42. Resolved in `506da3bf9cf54310991dbd12954545ad0e9d229c` by keeping the exact arithmetic
  in the binding table and describing the non-contract categories without overlapping subtotals.

## Review pass — 2026-09-11 — incremental

**Candidate base:** `62581f6b4009b98f4c09cd7c6305161ef5b858c2`
**Candidate head:** `506da3bf9cf54310991dbd12954545ad0e9d229c`
**Candidate branch:** `Docs/PublishedSurfaceAdmission`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:b42dab1a9df89f3fdde1932c10052aa8e6caa36a8c1f5b3c1aa62c59d1f13aa9` `(2 paths)`
**Candidate bundle:** `C:\Users\TommySeery\AppData\Local\Temp\concertable-review-Docs-PublishedSurfaceAdmission-506da3bf9`
**Candidate bundle identity:** `sha256:c001d26a3bafec4cf76862e713c57b78d9e55bcb048909ffc3a7a378b020f605`
**Work-order path:** `reviews/Docs-PublishedSurfaceAdmission.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No findings. Native/general, train-set consistency, and inventory-coherence lenses confirmed that the
corrective delta resolves `DOCS-006` and `DOCS-007`; the binding table remains 58 unique rows with exactly
seven admitted train properties, and the plan graph is clean.

## Review pass — 2026-09-11 — incremental

**Candidate base:** `506da3bf9cf54310991dbd12954545ad0e9d229c`
**Candidate head:** `0ad659386004e3809326aca2341d2b9a5a8c5d74`
**Candidate branch:** `Docs/PublishedSurfaceAdmission`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:e46776609b8c8d64db00f64d5698c85e32ab26a0e9384bd9d2429fbb38675833` `(2 paths)`
**Candidate bundle:** `C:\Users\TommySeery\AppData\Local\Temp\concertable-review-Docs-PublishedSurfaceAdmission-0ad659386`
**Candidate bundle identity:** `sha256:2f6a374ce41a9a85d2fb98e5e1bf5dfcd55389d4a7803b4fc84320a3a2fddb7d`
**Work-order path:** `reviews/Docs-PublishedSurfaceAdmission.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

- [x] **DOCS-008 — MEDIUM — review evidence reachability** — `plans/platform/PUBLISHED_SURFACE_ADMISSION_PROGRESS.md:39`
  The ledger linked this canonical work order before it existed in the frozen branch tree. Resolved by
  tracking the work order in the terminal review-only commit; the progress content itself was otherwise
  clean under native/general and plan-ledger consistency review.
