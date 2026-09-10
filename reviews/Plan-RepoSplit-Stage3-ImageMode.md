# Code review — Plan/RepoSplit-Stage3-ImageMode

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `4e1fd82c52e9a6c0aa90dd0a8a890727cc4b5d3c`  `(2026-08-30)`
**Security-reviewed up to commit:** `4e1fd82c52e9a6c0aa90dd0a8a890727cc4b5d3c`  `(2026-08-30)`
**Judgment:** `approved`

**Security layer — no HIGH or MEDIUM findings.** Triggered by `.github/workflows/publish-images.yml`, which
holds `packages: write` and a registry credential. It has no `pull_request_target` trigger and checks out no
untrusted ref — it fires only on `push` to `main` and `workflow_dispatch` — so it never runs fork code with a
write-scoped token. `${{ matrix.project }}` is interpolated into a `run:` block, but its value is a `grep`
over the repository's own tracked csproj files evaluated on `main` after merge, so influencing it already
requires write access to `main`. The PR-time `container-images` job builds untrusted PR code with
`GITHUB_PACKAGES_TOKEN`, identical to the pre-existing `build` and `carve-*` jobs, holds only
`packages: read`, and never pushes; `basename` strips directories so its archive path cannot traverse.

## Review pass — 2026-08-30 — full

**Candidate base:** `bf15c221cb3365406fc7246106cce63a13588389`
**Candidate head:** `cf4107eae0aa6ad0a57f086a8968866818545685`
**Candidate branch:** `Plan/RepoSplit-Stage3-ImageMode`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:038f1993d4a89b6a0ab896d970e0e1776cffb1cb550d397e5d583799161a836c` `(29 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\claude\C--Users-TommySeery-source-repos-Concertable\ea4d65d6-d40c-457e-9b5d-c6f60719cb43\scratchpad\review-bundle-862`
**Candidate bundle identity:** `sha256:735e9f8ee56b377b8c304a95fdd2f2b88a92f7b583a01a25ff2f6bd924b91dae`
**Work-order path:** `reviews/Plan-RepoSplit-Stage3-ImageMode.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

**Reviewer independence — read this before trusting the judgment.** This pass had no independent lens
or native-review layer: subagent dispatch was withheld for this session, so the author reviewed their own
change. That is materially weaker than the canonical shape and it is the reason the two defects found in
this candidate were both caught by **CI and by reading vendor targets**, not by the human-style read. Treat
the `approved` judgment as "no further defect found by the author plus the repository's own gates", not as
independent confirmation. A second opinion on the two workflow files would still be worth having.

**Rules manifest** (resolved mechanically by `skill_router.py --skills-for` over the frozen path set, then
re-read and re-checked against the diff rather than assumed from earlier use in the session): `packages`,
`plans`, `docs-and-debt`, `csharp-style`, `csharp-naming`, `dotnet-standards:unit-testing`,
`dotnet:unit-testing`, `dotnet-standards:integration-testing`, `dotnet:integration-testing`.

Rule checks that mattered, with their evidence:

- **`packages` — per-folder build config, never repo-root.** `ContainerRegistry` is added to each of the
  five service `Directory.Build.props` rather than a shared `api/`-root file, which is what a carve requires.
  No nested `Directory.Build.props` was introduced, so nothing shadows an outer file. Verified empirically:
  `dotnet msbuild -getProperty:` resolves `ghcr.io/concertable/<name>` on all nine opted-in projects.
- **`packages` — a carve must not lose an import.** `api/Concertable.Payment/PublishedBaseline.props` sits
  *inside* the service folder and both importers (`tests/…UnitTests`, `tools/…Generator`) reach it as
  `..\..\`, so it survives the Payment carve. Deliberately **not** `Exists()`-guarded: unlike `api/`-root
  infra, a missing copy here means a broken tree and should fail loudly rather than silently yield an empty
  version.
- **`dotnet:unit-testing` — the tier gate.** Project name still ends `.UnitTests`; no host package added
  (the new item is an `AssemblyAttribute`, not a `PackageReference`); assertions remain xUnit.
- **`docs-and-debt` — debt is deleted when fixed.** The `TECH_DEBT.md` entry describing the expected-red
  baseline was removed in the same commit that satisfied its "Resolves when", rather than kept as an archive.
- **`plans`.** Ledger keeps its required headers, names review before merge in `## Next Steps`, and was
  compacted from 558 to ~150 lines against the 200-line/16 KB budget. `plan_graph.py`: 0 errors, 0 warnings.

### Findings

- [x] **F1 — HIGH — correctness (build)** — `api/Concertable.B2B/src/Concertable.B2B.Workers/Concertable.B2B.Workers.csproj:9`
  The hand-written Azure Functions container contract (`ContainerBaseImage`, `ContainerWorkingDirectory`,
  `ContainerAppCommandInstruction=None`, two `ContainerEnvironmentVariable` items) both duplicated and
  conflicted with the Functions SDK's own. `Microsoft.Azure.Functions.Worker.Sdk.targets:280` imports its
  Publish targets unconditionally, and `AssignFunctionsBaseImage` already sets the base image (derived from
  `AzureFunctionsVersion` + the TFM), `/home/site/wwwroot`, `linux-x64`, both `AzureWebJobs*` variables, and
  the real entrypoint `/opt/startup/start_nonappservice.sh`. Because
  `Microsoft.NET.Build.Containers.targets:211` skips its own `ContainerAppCommand` assignment when the
  instruction is `None`, the SDK-set entrypoint remained and tripped
  `error CONTAINER2026: ContainerAppCommand and ContainerAppCommandArgs must be empty`. Failure scenario:
  every `B2B.Workers` image build fails, so `main` publishes eight of nine images and the Functions image
  never exists. **Fixed** in `b74be666b` by deleting all of it and keeping only `EnableSdkContainerSupport`
  and `ContainerRepository` — also strictly better, since the SDK derives the base-image tag from the TFM
  where the hardcoded `4-dotnet-isolated10.0` would rot at the next framework bump.

- [x] **F2 — LOW — error handling** — `.github/workflows/publish-images.yml:99`
  The digest-reporting step could fail a publish job whose push had already succeeded. Failure scenario: a
  transient GHCR read or unexpected `dotnet msbuild -getProperty` output makes `imagetools inspect` exit
  non-zero, and a `main` run is reported red even though the image is pushed and immutable at its SHA tag —
  sending someone to investigate a publish that actually worked. **Fixed** in this pass: the step is
  `continue-on-error: true`, since it is reporting only.

**Considered and deliberately not raised.** `ContainerRegistry` duplicated across five files is required by
the carve, not duplication to remove. The nine sequential image builds in `container-images` are one job
rather than a matrix on purpose — they share the workspace `obj/`, so a matrix would rebuild the shared
closure nine times on nine cold runners. Neither the base-image tag pinning nor a `concurrency` group is a
violation of any loaded rule, and our own images are pinned downstream by digest.

**Not covered by this pass.** That the published images actually *run* — `container-images` proves only that
they build. The standalone-host boot smoke in image mode is stage 3 rt4, recorded in the plan ledger's
`## Next Steps`.

## Review pass — 2026-08-30 — incremental

**Candidate base:** `cf4107eae0aa6ad0a57f086a8968866818545685`
**Candidate head:** `134ec1b69ae4bee84d72299703ad0dda8b9b8b34`
**Candidate branch:** `Plan/RepoSplit-Stage3-ImageMode`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:62ed34cd416dc6118d48b8dd5c5cb93793eb03153a1889ed4f38a5840e3ff6b5` `(3 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\claude\C--Users-TommySeery-source-repos-Concertable\ea4d65d6-d40c-457e-9b5d-c6f60719cb43\scratchpad\review-bundle-862`
**Candidate bundle identity:** `sha256:735e9f8ee56b377b8c304a95fdd2f2b88a92f7b583a01a25ff2f6bd924b91dae`
**Work-order path:** `reviews/Plan-RepoSplit-Stage3-ImageMode.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

Exists because the full pass's own F2 remediation was a code change, and remediation must return through a
fresh watermark rather than leave the marker behind HEAD. The delta is exactly that remediation
(`continue-on-error` on a reporting-only step, already stated as F2's concrete fix and re-read here as
applied) plus this work order and the ledger's `## Reviews` entry. No new rule is routed by the three paths
beyond those already in the manifest above.

### Findings

No findings. The watermark moves to `134ec1b69`.

## Review pass — 2026-08-30 — incremental

**Candidate base:** `a466680f64e1eaba7101c6b3fb8fe38482088277`
**Candidate head:** `dd5aa27769e982a1543f867c1ab98229aff1a571`
**Candidate branch:** `Plan/RepoSplit-Stage3-ImageMode`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:9516aa5bda47be417840d44b4a081613e22dd1b37bb36c8d0c61b95cc3831620` `(13 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\claude\C--Users-TommySeery-source-repos-Concertable\ea4d65d6-d40c-457e-9b5d-c6f60719cb43\scratchpad\review-bundle-862`
**Candidate bundle identity:** `sha256:735e9f8ee56b377b8c304a95fdd2f2b88a92f7b583a01a25ff2f6bd924b91dae`
**Work-order path:** `reviews/Plan-RepoSplit-Stage3-ImageMode.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

Merge of `origin/main` (`c4451509f`) plus its conflict resolution. `#858` re-baselined the
published-contract snapshot to `0.1.0-alpha.0.1254` while this branch was doing the same to `1255`, so the
two collided on four baseline files, the test, the generator, and `TECH_DEBT.md`.

Resolution reviewed as a change in its own right, since a mis-resolution here is silent:

- **The four `1254` baseline files are byte-identical to `origin/main`.** Verified by comparing the staged
  blob hashes against `origin/main`'s, because Git reported them as *renames from this branch's `1255`
  files* — similarity detection, which would look the same had the resolution kept this branch's content
  under main's folder name. That would have produced a baseline claiming to be `1254` while holding `1255`'s
  snapshot. It did not.
- The redundant `1255` folder is gone and `1009` stays deleted, so exactly one baseline folder remains.
- `TECH_DEBT.md` needed nothing: main had already removed the same entry.
- The single-source-of-truth indirection is kept, because main still carries the duplicate literal this
  branch removes. Both projects resolve `0.1.0-alpha.0.1254` from `PublishedBaseline.props`, and the four
  compatibility tests pass against main's baseline through it.
- The remaining paths are `#858`'s own `ConcertablePlatformVersion` pin bumps, arriving unmodified.

### Findings

No findings. The watermark moves to `dd5aa2776`.

## Review pass — 2026-08-30 — incremental

**Candidate base:** `dd5aa27769e982a1543f867c1ab98229aff1a571`
**Candidate head:** `4e1fd82c52e9a6c0aa90dd0a8a890727cc4b5d3c`
**Candidate branch:** `Plan/RepoSplit-Stage3-ImageMode`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:5f82442a65a6a14e3da6a105e6f2de021ad111faa7ec7b7e6fa05d0837d3c91e` `(6 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\claude\C--Users-TommySeery-source-repos-Concertable\ea4d65d6-d40c-457e-9b5d-c6f60719cb43\scratchpad\review-bundle-862`
**Candidate bundle identity:** `sha256:735e9f8ee56b377b8c304a95fdd2f2b88a92f7b583a01a25ff2f6bd924b91dae`
**Work-order path:** `reviews/Plan-RepoSplit-Stage3-ImageMode.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

The prior pass's own work-order commit, plus a second merge of `origin/main` (`7e2c3498c`, PR #863 —
navbar slots). The merge was conflict-free and touches no file this branch touches, verified by intersecting
the two merge-base diffs: the overlap is empty.

The only non-documentation paths in this delta — `app/web/shared/src/components/{Navbar,AppLayout}.tsx` —
arrive unmodified from `main`, where they were reviewed and merged under their own work order
(`reviews/Chore-TechDebtNavbarSlots.md`). They are **not** in this PR's diff, because merging `main` moved
the base with them; `git diff origin/main...HEAD` is 24 paths and contains none of them. Nothing here is this
branch's change to review.

### Findings

No findings. The watermark moves to `4e1fd82c5`.
