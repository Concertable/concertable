# Code review — Refactor/DotNetPlatformPublisherConsumers

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `2eb7e68c3f016987b14f85bea88c436d43a4f08e`  `(2026-09-08)`
**Security-reviewed up to commit:** `2eb7e68c3f016987b14f85bea88c436d43a4f08e`  `(2026-09-08)`
**Judgment:** `approved`

Checkpoint 7B of [`REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`](../plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md).
7B is two changes with different risk: rename the global pin to the plan's
`ConcertableDotNetPlatformVersion`, and stop the monorepo publishing platform package IDs. Only the
rename is landing. Stopping publication before `platform-dotnet` publishes its first version would
leave every service closure unable to restore, so it is prepared below and deliberately not committed.

## Review pass — 2026-09-08 — full

**Candidate base:** `ef8d505fdb0133d8b967d58634022192169e90e1`
**Candidate head:** `c0252bdd06f5f8436dcd3571a448422b71ad901d`
**Candidate branch:** `Refactor/DotNetPlatformPublisherConsumers`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:df01093673d0c85599c6cd70a12cb5460884f40aa904d720892dfc92b0d64b39` `(14 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\claude\C--Users-TommySeery-source-repos-Concertable--worktrees-Refactor-DotNetPlatformPublisherConsumers\2897c3da-9893-4e4e-8d18-8e435457a178\scratchpad\review-bundle-7b`
**Candidate bundle identity:** `sha256:1543fe206ba4136c11ef916a63ac796a66101b6bb9f1579f426cb9db97e3e57a`
**Work-order path:** `reviews/Refactor-DotNetPlatformPublisherConsumers.md`
**Work-order mode:** `new`
**Pass judgment:** `changes-requested`

Routed rules for the frozen paths: `packages` (7 pin files), `docs-and-debt` (2 TECH_DEBT files),
`plans` (1 plan file). `.github/workflows/**` and `scripts/*.ps1` route to no standard.

### Findings

- [x] **F1 — MEDIUM — test-coverage** — `.github/scripts/bump-platform-version.sh:17`
  Nothing gated the bump script's element name against the pin files it edits, so an incomplete rename
  of this property fails silently. The script keys three separate things off the literal element name —
  its discovery `grep -rlE`, its "no pins found — refusing" guard, and its in-place `sed` — while the
  name itself is declared in seven `api/**/Directory.Packages.props`. Rename the property and miss the
  script and the failure mode is not a red build: the script finds zero files, hits its refusal guard,
  and `platform-sync` stops opening bump PRs entirely, leaving every service pinned to a stale platform
  with nothing saying so. Rename the script and miss one props file and that service silently stops
  being bumped. Fix: a test that reads the element out of the script rather than restating it and
  asserts every declaring pin file declares exactly that one.
  **Resolved** in `2eb7e68c3` by `.github/scripts/bump-platform-version.test.mjs`. Proven to bite:
  with the script reverted to the old name while the props files carry the new one, it reports
  `not ok 1`.

### Verified in this pass

- **Every changed line carries the property name and nothing else.** `git diff -U0` filtered to lines
  containing neither the old nor the new name is empty across all 14 paths — no incidental edits, no
  line-ending churn.
- **Pin-file count is seven, by enumeration, not memory.** `grep -rlE
  '<ConcertableDotNetPlatformVersion>[^<]+</…>' api --include=Directory.Packages.props` — the bump
  script's own discovery expression — returns Auth, B2B, Customer, Payment, Search, Shared and `tests`.
  That set is exactly the set of folders declaring any `PackageVersion Include="Concertable.*"`.
  `api/Concertable.Auth.Contracts/Directory.Packages.props` consumes no `Concertable.*` package, so it
  correctly carries no pin; the handoff's "nine folders" is wrong at this base.
- **No orphaned reference.** Every file referencing `$(ConcertableDotNetPlatformVersion)` also declares
  it in the same file (20/38/39/19/21/7/1 references against one declaration each). A CPM props file
  never imports a sibling, so a reference without a local declaration would resolve to an empty
  `Version=""` — the silent failure this check exists to rule out. Zero old-name declarations survive
  under `api/`, so `local-platform.ps1`'s renamed `-p:` global override cannot be shadowed by a stale
  declaration.
- **`platform-sync`'s cascade guard and superseded-PR closer are untouched.** Both key off
  `chore/platform-sync-` and `chore(platform): sync`, neither of which contains the property name; the
  new commit message `chore(platform): sync ConcertableDotNetPlatformVersion to <v>` still matches the
  guard's `grep -qE`. This was the one hidden coupling that could have turned the rename into an
  infinite sync cascade.
- **`local-platform.ps1`'s two `-p:` overrides moved with the name.** They are MSBuild *global*
  properties, which a props-file `<PropertyGroup>` cannot override, so leaving them on the old name
  would have sent the local inner loop to the feed version instead of the locally packed
  `0.1.0-local.*`.
- **`ConcertablePaymentVersion` is untouched.** It is a separate hand-maintained cross-service pin in
  B2B and Customer that `platform-sync` does not track.
- **`docs-and-debt`:** the four TECH_DEBT sentences renamed in place keep their single home; no rule
  gained a second copy and no violation site was named in a rule doc.

### Rename grep-gate allowlist

`plans` requires `grep -rniE 'ConcertablePlatformVersion'` to reach zero or carry a justified
allowlist. Every functional surface is at zero — `api/`, `.github/` and `scripts/` return nothing. The
survivors are all prose, in three classes, none executable:

| Class | Files | Why it survives |
|---|---|---|
| Frozen review work orders | `reviews/` × 10 | Each records what was reviewed at a past commit under the name in force then. Rewriting them falsifies the record. |
| Sibling-epic plans and ledgers | `plans/data-access/` × 3, `plans/launch/` × 3, `plans/typed-result/` × 2 | Owned by other epics' active worktrees. `plans` forbids editing one logical ledger from two worktrees, so these are handoffs, not edits. |
| M1 stream's ledger | `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_FOUNDATION_PROGRESS.md` | Same epic, but the M1 stream's exclusive ledger — a sibling stream already collided on it today. |

Five survivors are live forward-looking prose that now names a property that does not exist
(`SPECIFICATION_QUERY_BOUNDARY_PLAN.md:304`, `SPECIFICATION_QUERY_BOUNDARY_PROGRESS.md:9`,
`AUDIT_DATETIMEOFFSET_PLAN.md:13`, `DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md:94`,
`FOUNDATION_PROGRESS.md:175`). Each owner follows the rename at its next material checkpoint. The rest
are dated records of a delivered version and are correct as history.

**Outside this repository:** `Concertable/agent-standards` states the old name in the `dotnet:packages`
standard — `standards/dotnet/PACKAGES.md:110` plus its three generated copies
(`.claude/skills/packages/SKILL.md:115`, `plugins/dotnet/skills/packages/SKILL.md:115`,
`plugins/dotnet/standards/dotnet/PACKAGES.md:110`). That is loaded agent guidance and goes stale the
moment this merges. Per `docs-and-debt` it needs one authored edit to `standards/dotnet/PACKAGES.md`
plus a regeneration of its copies, in that repo on its own PR — not four hand edits. Its checkout is
currently on the unrelated dirty branch `Fix/Prune-Superseded-Plugins`, so this worktree does not
touch it.

## Review pass — 2026-09-08 — incremental over the F1 remediation

**Candidate base:** `c0252bdd06f5f8436dcd3571a448422b71ad901d`
**Candidate head:** `2eb7e68c3f016987b14f85bea88c436d43a4f08e`
**Candidate branch:** `Refactor/DotNetPlatformPublisherConsumers`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:c67b3f25734e1318b92e0f7edce38cbf9851b13148f5a0365fdcf1aa6f9ac391` `(4 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\claude\C--Users-TommySeery-source-repos-Concertable--worktrees-Refactor-DotNetPlatformPublisherConsumers\2897c3da-9893-4e4e-8d18-8e435457a178\scratchpad\review-bundle-7b-p2`
**Candidate bundle identity:** `sha256:7ca7529270f2cebddf344c450d5c59a9f47f5ce2f08ac5b69ed0825e88923c69`
**Work-order path:** `reviews/Refactor-DotNetPlatformPublisherConsumers.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No findings. The new test sits beside its script so the existing `node --test
.github/scripts/*.test.mjs` step picks it up with no `test.yml` change, matching the
`platform-sync-pr-action.test.mjs` precedent. It shells out with `execFileSync` (no shell), writes and
removes only its own `mkdtemp` directories, and touches no credential, network or repository state. Its
one portability wrinkle — recursive `readdirSync` Dirent path fields — is exercised on `v20.19.5`
locally, the same major the `workflow-tests` job pins, and carries a `parentPath ?? path` fallback. The
three `plans/platform/` doc edits are the same one-word rename applied to sentences that state the
mechanism as current.

## Security review

The frozen paths include `.github/workflows/` and `.github/scripts/`, so the merge gate requires the
security marker. No new trust boundary, credential, permission or trigger in either pass:

- `platform-sync.yml` — only a commit-message string and a PR-body string. `permissions`,
  `PLATFORM_SYNC_TOKEN`/`GITHUB_TOKEN` usage, the `workflow_run` trigger and every `if` condition are
  byte-identical.
- `publish-images.yml` — one comment line above an unchanged `GITHUB_PACKAGES_TOKEN` assignment.
- `bump-platform-version.sh` — the interpolated `$target` still arrives from the workflow's
  `steps.ver.outputs.version`, which its own `case` guard validates before the script runs. The rename
  changes no quoting, no delimiter and no data path.
- `bump-platform-version.test.mjs` — no network, no credentials, no writes outside its own temp dirs.

## Validation

| Gate | Result |
|---|---|
| `./scripts/local-platform.ps1 prepare` | green — 58 packages packed at `0.1.0-local.1788899153911`, 0 errors. The renamed `-p:` override resolving against the renamed pin is what makes this restore succeed. |
| `./scripts/local-platform.ps1 build api/Concertable.slnx --configuration Release` | green — 0 errors |
| `python .github/workflows/tests/test_service_scope.py` | green — 20/20, plus the chained `test_publish_packages_policy.py` |
| `node --test .github/scripts/e2e-ghcr-login.test.mjs` | green — 3/3 |
| `node --test .github/scripts/platform-sync-pr-action.test.mjs` | green — 4/4 |
| `node --test .github/scripts/bump-platform-version.test.mjs` | green — 4/4, and proven to fail on a simulated incomplete rename |
| Merge-queue E2E tier | `skip-e2e` — a build-property rename ships no runtime behaviour and has no positive trigger |

`CI` does not build package-clean: `local-platform.ps1` packs all 58 packages from source, so `CI` can
be green while real feed pins are broken. The gates that exercise published pins are `Publish images`
and the `carve-*` jobs, and both restore the renamed pin from each service folder's own props file.

## Prepared and NOT landed — stop the monorepo publishing platform package IDs

The second half of 7B. **Do not land before `platform-dotnet` (7A) has published its first version:**
the `carve-*` gates and every service closure restore the platform from the feed, so removing the
monorepo as publisher first strands all five services.

**The ID split is already machine-readable — do not hand-list it.** `eng/repository-split/inventory.json`
assigns every one of the 58 packable projects a `target`, and the `split-inventory` CI gate
(`eng/repository-split/inventory.py --check`) fails on drift. At this head: 34 → `platform-dotnet`,
23 → the five retained services (auth 2, b2b 12, customer 4, payment 4, search 1), and
`Concertable.Testing.E2E` → `system`.

The mechanism, in the order it must be written:

1. **Filter the pack output, not `IsPackable`.** Setting `IsPackable=false` on the 34 platform projects
   would break `local-platform.ps1 prepare`, which packs all 58 from source to feed the local inner
   loop and the carve gates. Instead add a step to `publish-packages.yml` between `Pack publishable
   projects` and `Require a new lockstep package version` that removes the `.nupkg` files whose IDs
   `inventory.json` assigns to a target other than a retained service. `package_publication_policy.py`
   then validates only the retained batch and the push pushes only the retained batch, with **no change
   to the policy script**.
2. **Split `verify-restore`'s generated consumer.** It currently pins every `IsPackable` project at the
   publish job's `$VERSION`. Once platform IDs stop publishing at that version, asking the feed for
   them at `$VERSION` fails NU1101. It must emit two `ItemGroup`s: retained IDs at `$VERSION`, foreign
   IDs at the `ConcertableDotNetPlatformVersion` read out of
   `api/Concertable.Shared/Directory.Packages.props`. That keeps the BUILD1 closure check intact — it
   still proves no retained package declares a feed-absent `Concertable.*` dependency.
3. **Hand the platform pin to `platform-dotnet`'s train.** `platform-sync.yml`'s `BELLWETHER` is
   `concertable.payment.client`, a *retained* package, so after step 1 its version no longer tracks the
   platform train and the pin it bumps would be wrong. Per the plan's "Replacement for platform-sync",
   Renovate owns the platform pin from there and `platform-sync.yml` stops bumping it.

Steps 1 and 2 are fully specified and writable now. **Step 3 is genuinely blocked**, and not merely by
sequencing: it needs `platform-dotnet`'s bellwether package ID, its feed location and its first
published version, none of which exist yet. It belongs with 7C and the Renovate rollout, not with this
rename.

Retiring `platform-sync.yml` also retires the only consumer of `bump-platform-version.sh` and its new
test. Delete all three together at that point rather than leaving the script as a dead entry point.

```text
Blocked: 7B's publication half — the monorepo cannot stop publishing platform package IDs
Blocked by: checkpoint 7A in the platform-dotnet repository, which does not exist yet
Unblock action: Tommy creates platform-dotnet; 7A lands its source, CI and publication and publishes
  the first non-conflicting version
Resume when: a platform-dotnet release is resolvable on the org feed for the 34 platform IDs
```

## Next

`plan-execution` owns delivery. This pass is complete and the branch is merge-ready for the rename
half; the publication half resumes on the evidence above.
