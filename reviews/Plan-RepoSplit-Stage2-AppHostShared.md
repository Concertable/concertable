# Code review — Plan/RepoSplit-Stage2-AppHostShared

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `06dca73d57d50f2cf1c128fcf978f928734da7f6`  `(2026-08-26)`
**Judgment:** `approved`

## Review pass — 2026-08-26 — full

**Candidate base:** `e4c91fe2d8ad3df74b47ef64341cf9c223016e05`
**Candidate head:** `06dca73d57d50f2cf1c128fcf978f928734da7f6`
**Candidate branch:** `Plan/RepoSplit-Stage2-AppHostShared`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:a0e8cfe71a1224e5fcdbc25dee7e76e17abee017fcb6858178e452d05380eac8` `(4 paths)`
**Work-order path:** `reviews/Plan-RepoSplit-Stage2-AppHostShared.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

Native/general review layer plus a packaging-correctness lens over the frozen diff (repo-split stage 2,
round-trip 1: opt `Concertable.AppHost.Shared` into publishing). No repository skill is routed for the
changed paths; the governing standard is `packages` (BUILD1 + MinVer + PrivateAssets), applied by the
parent. No security-sensitive path is touched, so no security layer applies.

### Findings

- [x] **NAT1 — HIGH — packaging-correctness** — `api/Concertable.AppHost.Shared/Directory.Packages.props`
  The folder's own nearest-wins `Directory.Packages.props` had no `MinVer` `GlobalPackageReference`, so
  once packable the package would version at the SDK default `1.0.0` instead of the platform
  `0.1.0-alpha.N`, and never match the platform-sync pins round-trip 2 consumes. **Fixed** in `06dca73d5`:
  added `<GlobalPackageReference Include="MinVer" Version="7.0.0" />`, mirroring
  `api/Concertable.Messaging/Directory.Packages.props`. Verified: `dotnet pack` now emits
  `0.1.0-alpha.0.1204`, not `1.0.0`.
- [x] **NAT2 — MEDIUM — packaging-correctness** — `api/Concertable.AppHost.Shared/Directory.Build.props`
  The now-published folder omitted the package metadata every published sibling sets, and its comment
  still claimed "this folder doesn't yet [publish]". **Fixed** in `06dca73d5`: added `Authors`, `Company`,
  `RepositoryUrl`, `PackageProjectUrl`, `RepositoryType`, `PackageRequireLicenseAcceptance`,
  `MinVerTagPrefix`, `MinVerMinimumMajorMinor`, and corrected the comment, mirroring
  `api/Concertable.Messaging/Directory.Build.props`.

BUILD1 verified independently by the parent: AppHost.Shared's only `ProjectReference`
(`Concertable.Messaging.AzureServiceBus`) is already packable/published, and the project exposes no
Reunion carrier, so no `PrivateAssets="all"` is owed. Ledger doc changes carry no reviewable defect.
