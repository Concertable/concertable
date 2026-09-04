# Repository-per-microservice migration — Stage 3 RT3 progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: none — PR #897 merged
- Branch: `Plan/RepoSplit-Stage3-Hosting-rt3` — merged
- PR: [#897](https://github.com/Concertable/concertable/pull/897) — merged as `2979ab78f4204eeed07cca06654777a37965f007` on 2026-09-04
- Dependency/package gates: terminal — platform `0.1.0-alpha.0.1281` was the published consumer baseline
- Last reconciled: **2026-09-04** from merged PR #897 and its terminal GitHub Actions evidence

## Current state

RT3 is terminal. The merged cutover replaces standalone AppHost foreign source references with published
Hosting packages and digest-pinned service containers. `2979ab78f4204eeed07cca06654777a37965f007` is present
on `origin/main`.

## Next Steps

No RT3 action remains. The migration next promotes Auth through plan checkpoints 10A–10F: refresh and freeze
Auth at the approved SHA, land its canonical repository and release, qualify the resulting package/image in
the system compatibility set, promote that exact set through configuration, then remove frozen monorepo Auth
source. Those checkpoints remain owned by the Auth promotion workstream.

## Completed work

- Hosting seam and digest repairs landed through PRs #870, #881, #888, and #892; platform
  `0.1.0-alpha.0.1281` was published in run `33408113198`.
- RT3 standardized the service AppHost implementation files and types as `AppHost.cs` / `AppHost`, compiled
  each service's own Hosting project from source, and consumed only foreign Hosting seams as packages.
- [PR #897](https://github.com/Concertable/concertable/pull/897) merged the final RT3 candidate as
  `2979ab78f4204eeed07cca06654777a37965f007`.

## Verification

- Exact-head PR CI [run 33858822002](https://github.com/Concertable/concertable/actions/runs/33858822002)
  passed on `755928910973c41e9927dc51af2e850356149386`.
- Terminal merge-group [run 33860333526](https://github.com/Concertable/concertable/actions/runs/33860333526)
  passed on merge commit `2979ab78f4204eeed07cca06654777a37965f007`, including successful
  [API E2E](https://github.com/Concertable/concertable/actions/runs/33860333526/job/100985208858),
  [UI E2E](https://github.com/Concertable/concertable/actions/runs/33860333526/job/100987517438), and
  `ci-complete`.

## Reviews

The RT3 review work order completed with no open RT3 findings; the final candidate was approved before the
successful exact-head CI and merge-queue validation.

## Decisions, discoveries, blockers, and deviations

- RT3 consumes four foreign images: Auth, Payment Web, Payment Workers, and B2B Seed Simulator; image
  references remain immutable digests.
- The pinned Auth image's development signing-key path is not writable by its image user. Auth promotion
  must replace the temporary local Aspire `--user root` bridge when it publishes the corrected image.
- The pinned Auth image does not carry the certificate its HTTPS listener expects. Auth promotion must ship
  or supply the canonical certificate and then remove the temporary `WithHttpsDeveloperCertificate()` bridge
  and its composition assertions.
- A pinned Auth image did not transfer Auth source ownership. The monorepo remains Auth's writer until the
  explicit checkpoint-10 refresh/freeze/publish cutover.
- The future canonical full-system repository is `Concertable/system`; its Aspire project/namespace is
  `Concertable.System.AppHost`. Terraform and deployable environment state move to
  `Concertable/infrastructure` and `Concertable/configuration`.
