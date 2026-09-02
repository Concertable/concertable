# Repository-per-microservice migration — Stage 3 RT3 progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Plan-RepoSplit-Stage3-Hosting-rt3`
- Branch: `Plan/RepoSplit-Stage3-Hosting-rt3` (the PR head is authoritative)
- PR: [#897](https://github.com/Concertable/concertable/pull/897) — open, unmerged, and carrying the `full-e2e` label
- Dependency/package gates: published platform `0.1.0-alpha.0.1281` is available; Stage 4 is merged on `main`
- Last reconciled: **2026-09-02** from exact-head CI run `33676402751`, merge-group run `33672179048`, and diagnostic artifact `9864132171`

## Current state

RT3 exclusively owns the standalone AppHost cutover from foreign source references to published Hosting
packages and digest-pinned service containers. Exact-head CI run `33676402751` is green with 82 successful
checks and three expected E2E skips. Merge-group run
`33672179048` proved the GHCR login and temporary Auth root-user bridge work: Auth created its development
key and seeded 75 credentials. Auth then exited because the pinned pre-cutover image attempted to bind HTTPS
on port 8080 without a server certificate. B2B consequently remained at `users=0/71`, because it could not
receive Auth credential-registration events, and its E2E readiness gate failed.

The repair preserves the logical endpoint name `https` used by service discovery while explicitly setting
the legacy Auth container process to `ASPNETCORE_URLS=http://+:8080`. Aspire still owns the host-facing
endpoint. All four image-consuming standalone AppHosts carry exact composition assertions for the root runtime
arguments, HTTP endpoint annotation, and effective process environment. Each AppHost's own `*.Hosting`
project remains source-backed; foreign Hosting dependencies remain published packages.

## Next Steps

Return PR #897 to the merge queue with `full-e2e`, own API/UI E2E to a terminal result, and confirm the
merged commit on `main`. The merge-group proof must show GHCR
login, Auth startup and continued operation, B2B and Customer API E2E, UI E2E, the Payment integration shard,
and `ci-complete` green.

After Auth publishes an image that uses a non-root-writable development signing-key path and an explicit
certificate-free local HTTP listener, update the pinned Auth digest and remove every `--user root` and
`ASPNETCORE_URLS` bridge override with their composition assertions. After RT3 lands, repository promotion
proceeds through the canonical plan's single-writer cutovers: refresh each extracted service from the approved
final monorepo SHA, freeze that monorepo path, validate and publish from the service repository, switch
package/image consumers, then remove the frozen monorepo source. Auth follows checkpoints 10A–10E; ongoing
Auth changes in the monorepo are allowed until 10A and must be included in that exact refresh.

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
- Exact-head CI run `33670103562` passed 82 checks with three expected E2E skips.
- Merge-group run `33672179048` proved the root-user repair, then exposed the legacy Auth image's
  certificate-free HTTPS listener defect; diagnostic artifact `9864132171` records the exact failure.
- Every canonical standalone AppHost now compiles its owning service's `*.Hosting` project from source while
  consuming only foreign Hosting seams as packages, matching checkpoint 2 and the post-extraction layout.

## Verification

Exact-head CI run `33676402751` passed all 82 executed checks with three expected E2E skips, including all
four changed AppHost composition suites, all five standalone carve builds, container-image validation, split
inventory, and every selected unit and integration shard. Merge-group run `33672179048` completed GHCR authentication, created Auth's development signing
key, and seeded 75 credentials. Diagnostic artifact `9864132171` contains
`InvalidOperationException: Unable to configure HTTPS endpoint`; B2B's `users=0/71` readiness state is a
downstream consequence of Auth exiting before credential events could populate B2B users.

## Reviews

The prior native, security, persistence, test-impact, and repository review was approved through
`438744ed7d150eb76c72d494c19bc6cb280176a5`. Incremental review through
`e88723e49fa9bf1867fc54cd52bd3910fbd9a279` found no open RT3 finding. The final incremental review found and
resolved RT3-F5: own-service Hosting had incorrectly remained a package edge that the monorepo source swap
masked. Security and boundary lenses found no additional issue. Fresh correctness, security, and service-boundary
reviews of `9851f81646b587d08a1929eb662d2573f8ad0013..36299abfc8915a65ddd5aeda47a52bb8e8e84c7a`
found no additional issue; the repaired candidate is approved for the final requeue.

## Decisions, discoveries, blockers, and deviations

- RT3 consumes four foreign images: Auth, Payment Web, Payment Workers, and B2B Seed Simulator; image references remain immutable digests.
- Pre-cutover bridge images may remain private. Every CI job that starts an image-backed AppHost authenticates to GHCR with its existing read-only `GITHUB_TOKEN` package permission.
- The pinned Auth image's development signing-key path is not writable by its image user. The temporary `--user root` override is confined to local Aspire bridge composition and has an explicit removal gate tied to a corrected Auth image and digest.
- The pinned Auth image also assumes an HTTPS process listener without carrying a certificate. RT3 keeps the logical Aspire endpoint name `https` for service discovery but forces the legacy container process to listen on HTTP port 8080 until a corrected image is available.
- A pinned Auth image does not transfer Auth source ownership. The monorepo remains Auth's writer until the explicit checkpoint-10 refresh/freeze/publish cutover.
- There is no ongoing bidirectional source synchronization. Each service promotion performs one final monorepo-to-service refresh and then flips to the service repository as the sole writer.
- The local command runner was unavailable during this checkpoint (`unsupported protocol version 5`), so branch writes and CI evidence were handled through GitHub; the existing RT3 worktree remains the designated local checkout.
