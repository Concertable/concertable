# Repository-per-microservice migration — Stage 3 RT3 progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Plan-RepoSplit-Stage3-Hosting-rt3`
- Branch: `Plan/RepoSplit-Stage3-Hosting-rt3` (the PR head is authoritative)
- PR: [#897](https://github.com/Concertable/concertable/pull/897) — open, unmerged, and carrying the `full-e2e` label
- Dependency/package gates: published platform `0.1.0-alpha.0.1281` is available; Stage 4 is merged on `main`
- Last reconciled: **2026-09-02** from PR CI run `33649519103`, merge-group run `33652310158`, and its diagnostic artifact `9856488528`

## Current state

RT3 exclusively owns the standalone AppHost cutover from foreign source references to published Hosting
packages and digest-pinned service containers. The implementation and exact-head PR CI are green. The first
post-Stage-4 merge-group candidate passed every build, image, carve, architecture, unit, and integration job,
then failed API E2E because the runner tried to pull the private bridge Auth image anonymously.

The diagnostic root cause is explicit: Docker returned `unauthorized` for
`ghcr.io/concertable/auth@sha256:8b7ba47e...`. Search at `https://localhost:7087/health` timed out only
because it correctly waited for the Auth dependency that failed to start. This is not a Search defect and
not the former container-vs-project resource defect.

## Next Steps

Validate the focused workflow-policy test and exact-head PR CI for the GHCR-login repair. If green, return
PR #897 to the merge queue with `full-e2e`, own the API/UI E2E run to a terminal result, and confirm the
merged commit on `main`. Do not make the pinned foreign services source-backed merely to avoid registry
authentication; RT3's boundary is deliberately image-backed.

After RT3 lands, this stream is complete. Repository promotion then proceeds through the canonical plan's
single-writer cutovers: refresh each extracted service from the approved final monorepo SHA, freeze that
monorepo path, validate and publish from the service repository, switch package/image consumers, then remove
the frozen monorepo source. Auth follows checkpoints 10A–10E; ongoing Auth changes in the monorepo are allowed
until 10A and must be included in that exact refresh.

## Completed work

- Hosting seam and digest repairs landed through PRs #870, #881, #888, and #892.
- Platform `0.1.0-alpha.0.1281` published successfully in run `33408113198` and was merged into the RT3 candidate.
- All five standalone AppHosts built in Release package mode against `1281`; inventory and diff gates passed.
- Service AppHost implementation files/classes use the canonical local `AppHost.cs` / `AppHost` names.
- Stage 4's container-backed E2E support was merged from `main`; the sole merge conflict in
  `eng/repository-split/inventory.json` passed exact-head CI.
- Exact-head PR CI run `33649519103` passed at `e88723e49fa9bf1867fc54cd52bd3910fbd9a279`.
- Merge-group run `33652310158` proved the remaining failure was missing GHCR authentication, not service behavior.

## Verification

Focused composition suites, all five package-mode AppHost builds, split inventory, and diff checks passed.
PR CI run `33649519103` passed all 78 jobs. Merge-group run `33652310158` passed all non-E2E gates; API
E2E failed before scenarios because Auth's pinned private image could not be pulled. Diagnostic artifact
`9856488528` contains the Docker `unauthorized` response and the dependent Search failure.

## Reviews

The prior native, security, persistence, test-impact, and repository review was approved through
`438744ed7d150eb76c72d494c19bc6cb280176a5`. Incremental review through `e88723e49fa9bf1867fc54cd52bd3910fbd9a279`
found no open RT3 finding. Review the focused workflow authentication repair before requeueing.

## Decisions, discoveries, blockers, and deviations

- RT3 consumes four foreign images: Auth, Payment Web, Payment Workers, and B2B Seed Simulator; image references remain immutable digests.
- Pre-cutover bridge images may remain private. Every CI job that starts an image-backed AppHost must authenticate to GHCR with its existing read-only `GITHUB_TOKEN` package permission.
- A pinned Auth image does not transfer Auth source ownership. The monorepo remains Auth's writer until the explicit checkpoint-10 refresh/freeze/publish cutover.
- There is no ongoing bidirectional source synchronization. Each service promotion performs one final monorepo-to-service refresh and then flips to the service repository as the sole writer.
- The local command runner was unavailable during this checkpoint (`unsupported protocol version 5`), so branch writes and CI evidence were handled through GitHub; the existing RT3 worktree remains the designated local checkout.
