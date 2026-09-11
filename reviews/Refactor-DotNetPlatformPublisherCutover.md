# Code review — Refactor/DotNetPlatformPublisherCutover

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `3686811e6e2eb0afd30d486a444d40e9ae4a23ca` `(2026-09-11)`
**Judgment:** `approved`

## Review pass — 2026-09-11 — full

**Candidate base:** `f48aec45680760aebb45aaf655341072dc769c86`
**Candidate head:** `3686811e6e2eb0afd30d486a444d40e9ae4a23ca`
**Candidate branch:** `Refactor/DotNetPlatformPublisherCutover`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:7367d4f100cf4b1a780235b9cd3fa77c4c8ccc7f8e6963138a3a1a1d0d7ecdb5` `(12 paths)`
**Candidate bundle:** `C:\Users\TommySeery\AppData\Local\Temp\concertable-review-Refactor-DotNetPlatformPublisherCutover-6c2149ef8`
**Candidate bundle identity:** `sha256:731c70da8e9f9ef001bbc14c343b908faa0ab77b5d4d8f8f809174ec2adfc35f`
**Work-order path:** `reviews/Refactor-DotNetPlatformPublisherCutover.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

The full native pass was followed by test-impact and security lenses over each corrective commit. The
temporary detached review bundle was removed after its tracked-tree identity and path manifest had been
validated and the review completed.

### Findings

- [x] **PKG-PUBLISH-001 · native · MEDIUM — Ownership inventory changes did not trigger package publication.** Added the generated
  inventory, its generator and the ownership map to `on.push.paths`, with policy assertions for all three.
- [x] **PKG-PUBLISH-002 · test-impact · MEDIUM — The pack-mode assertion did not prove retained package dependency metadata.** The publisher
  now packs against feed-backed platform packages and validates every service-to-platform dependency in the
  retained nuspec batch against the repository platform pin before accepting or pushing a version.
- [x] **PKG-PUBLISH-003 · security · MEDIUM — Platform dependency IDs were compared case-sensitively.** Both inventory and nuspec IDs are
  case-folded before comparison, with a mixed-case regression fixture proving a mismatched train is rejected.

### Verified

- The exact solution pack completed with 58 artifacts; the ownership filter retained 23 service packages
  and removed 35 platform/system packages.
- The real retained batch declared 39 service-to-platform dependencies and every one targeted
  `0.1.0-alpha.0.1370`.
- A fresh consumer restored the 23 candidate service packages plus 34 published platform packages through
  an isolated NuGet package and HTTP cache, with no dependency downgrade.
- Package publication policy tests, image publication policy tests and all 41 service/E2E scope cases pass.
- `python eng/repository-split/inventory.py --check`, Python compilation and `git diff --check` pass.
- Native incremental review, test-impact review and security review are clean at the reviewed head.

### Not covered by this pass

The immutable feed push and post-publish fresh-consumer restore are remote merge gates owned by the branch PR.
