# Repository-per-microservice migration — Stage 3 RT3 progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Plan-RepoSplit-Stage3-Hosting-rt3`
- Branch: `Plan/RepoSplit-Stage3-Hosting-rt3` (the PR head is authoritative)
- PR: [#897](https://github.com/Concertable/concertable/pull/897) — open, unmerged, and carrying the `full-e2e` label
- Dependency/package gates: published platform `0.1.0-alpha.0.1281` is available; Stage 4 is merged on `main`
- Last reconciled: **2026-09-02** from exact-head CI run `33656931486`, merge-group run `33658754750`, and diagnostic artifact `9859035450`

## Current state

RT3 exclusively owns the standalone AppHost cutover from foreign source references to published Hosting
packages and digest-pinned service containers. Exact-head CI is green. Merge-group E2E now authenticates to
GHCR successfully, but the pinned pre-cutover Auth image starts as its non-root image user and Duende then
tries to create the relative development key at `/app/tempkey.jwk`. The image exits with
`UnauthorizedAccessException`; B2B subsequently reports the dependent Payment health timeout at
`https://localhost:7086/health`.

The focused RT3 repair runs only the local Aspire Auth bridge containers as root so that immutable legacy image
can persist its development signing key. It does not change the Auth deployable, image defaults, or production
runtime. All four consumer AppHosts carry an exact composition assertion for the runtime arguments. The final
incremental review also restored each standalone AppHost's own `*.Hosting` project reference from source;
foreign Hosting dependencies remain published packages.

Merge-group run `33658754750` also had an independent Payment integration fixture failure after 44 tests
passed: its Testcontainers SQL container stopped before the remaining five tests ran. That is a runner/container
startup failure rather than an RT3 assertion failure, but the next merge candidate must still prove the shard
green.

## Next Steps

Validate the five standalone AppHost/carve builds, four focused composition suites, and exact-head PR CI for
the own-Hosting source references and Auth bridge-user repair. If
green, return PR #897 to the merge queue with `full-e2e`, own API/UI E2E to a terminal result, and confirm the
merged commit on `main`. The merge-group proof must show GHCR login, Auth startup, B2B and Customer API E2E,
UI E2E, the Payment integration shard, and `ci-complete` green.

After Auth publishes an image that writes its development signing key to a non-root-writable path, update the
pinned Auth digest and remove every `--user root` bridge override and its composition assertion. After RT3
lands, repository promotion proceeds through the canonical plan's single-writer cutovers: refresh each
extracted service from the approved final monorepo SHA, freeze that monorepo path, validate and publish from the
service repository, switch package/image consumers, then remove the frozen monorepo source. Auth follows
checkpoints 10A–10E; ongoing Auth changes in the monorepo are allowed until 10A and must be included in that
exact refresh.

## Completed work

- Hosting seam and digest repairs landed through PRs #870, #881, #888, and #892.
- Platform `0.1.0-alpha.0.1281` published successfully in run `33408113198` and was merged into the RT3 candidate.
- All five standalone AppHosts built in Release package mode against `1281`; inventory and diff gates passed.
- Service AppHost implementation files/classes use the canonical local `AppHost.cs` / `AppHost` names.
- Stage 4's container-backed E2E support was merged from `main`; the sole merge conflict in
  `eng/repository-split/inventory.json` passed exact-head CI.
- Exact-head PR CI runs `33649519103` and `33656931486` passed; the latter recorded 81 successful checks and
  three expected E2E skips.
- All three E2E jobs authenticate to GHCR with read-only `GITHUB_TOKEN` package permission, protected by
  `.github/scripts/e2e-ghcr-login.test.mjs`.
- Merge-group run `33658754750` proved GHCR authentication works and exposed the legacy Auth image's
  `/app/tempkey.jwk` permission defect.
- Every canonical standalone AppHost now compiles its owning service's `*.Hosting` project from source while
  consuming only foreign Hosting seams as packages, matching checkpoint 2 and the post-extraction layout.

## Verification

Focused composition suites, all five package-mode AppHost builds, split inventory, and diff checks passed
before the current repair. Exact-head CI run `33656931486` passed all 81 executed checks. Merge-group run
`33658754750` completed the new `Log in to GHCR` step successfully. Diagnostic artifact `9859035450`
contains Auth's `UnauthorizedAccessException: Access to the path '/app/tempkey.jwk' is denied` and shows
Payment itself starting; its public health timeout is downstream of Auth's exit.

## Reviews

The prior native, security, persistence, test-impact, and repository review was approved through
`438744ed7d150eb76c72d494c19bc6cb280176a5`. Incremental review through
`e88723e49fa9bf1867fc54cd52bd3910fbd9a279` found no open RT3 finding. The final incremental review found and
resolved RT3-F5: own-service Hosting had incorrectly remained a package edge that the monorepo source swap
masked. Security and boundary lenses found no additional issue. Revalidate the resolved candidate before the
final requeue.

## Decisions, discoveries, blockers, and deviations

- RT3 consumes four foreign images: Auth, Payment Web, Payment Workers, and B2B Seed Simulator; image references remain immutable digests.
- Pre-cutover bridge images may remain private. Every CI job that starts an image-backed AppHost authenticates to GHCR with its existing read-only `GITHUB_TOKEN` package permission.
- The pinned Auth image's development signing-key path is not writable by its image user. The temporary `--user root` override is confined to local Aspire bridge composition and has an explicit removal gate tied to a corrected Auth image and digest.
- A pinned Auth image does not transfer Auth source ownership. The monorepo remains Auth's writer until the explicit checkpoint-10 refresh/freeze/publish cutover.
- There is no ongoing bidirectional source synchronization. Each service promotion performs one final monorepo-to-service refresh and then flips to the service repository as the sole writer.
- The local command runner was unavailable during this checkpoint (`unsupported protocol version 5`), so branch writes and CI evidence were handled through GitHub; the existing RT3 worktree remains the designated local checkout.
