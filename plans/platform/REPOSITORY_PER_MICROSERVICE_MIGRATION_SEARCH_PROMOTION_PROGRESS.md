# Repository-per-microservice migration — Search promotion progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Repository: retained `Concertable/search` (repository id `1351099165`)
- Worktree: `C:\Users\tommy\source\repos\search-next` (legacy local directory for the retained repository;
  the folder name is not a repository identity)
- Branch: `main` at `f3d73da3937e6d101708144f18ae95aff1a03671`
- PR: latest preparation PR [`Concertable/search#11`](https://github.com/Concertable/search/pull/11) merged and green
- Dependency/package gates: repository preparation is terminal; checkpoint 12 delivery remains ordered behind
  its producer/system gates and requires its normal authorization gate
- Last reconciled: **2026-09-05** from the retained repository identity, current main, and terminal PR #11

## Current state

Private `Concertable/search` retains its repository identity. Preparation through PR #11 now covers
repository-owned CI, Hosting/TestKit, migration and container candidates, standalone AppHost, seed convergence,
image security, and release-candidate verification. This ledger owns only checkpoint-12 Search preparation;
it must not edit another service or shared execution ledger.

State: **repository preparation terminal; delivery-gated**. The legacy local directory and remote redirect do
not create a `search-next` target. Search consumes published Contracts and producer simulator artifacts rather
than another data service's runtime source. No repository rename/replacement, visibility change, production
publication/deployment, or monorepo source removal is authorized by this ledger.

## Next Steps

Do not start another preparation slice. Before future Search work, canonicalize the existing checkout's remote
URL from the historical redirect to `https://github.com/Concertable/search.git`, fetch retained main, and verify
it is clean and exact. After foundation/M1 and the ordered producer gates are terminal, run a fresh current-head
review and execute checkpoint 12 without creating or importing another repository.

## Completed work

- Search extraction proof and the first CI slice remain historical preparation evidence from before the target
  retained its final repository name.
- Retained-repository PRs #1–#11 delivered the complete Search preparation sequence to
  `Concertable/search` main.

## Verification

- Local at `c7e6f766256ddaa0c6eb3bd4514a65bddbd96b1d`: Release Web and Workers builds succeeded; UnitTests passed
  21/21; Docker-backed IntegrationTests passed 27/27; `git diff --check` passed.
- The original PR #1 package-access failure was resolved before the retained preparation sequence completed.
  PR #11 exact-head `ci-complete` passed before merge to current main.

## Reviews

Full and security review completed through the historical first slice
`c7e6f766256ddaa0c6eb3bd4514a65bddbd96b1d`; approved with no findings. The old local work-order path is
historical evidence only. Run a fresh current-main review before checkpoint-12 delivery changes.

## Decisions, discoveries, blockers, and deviations

- Search is a data service: its current projection inputs, including rating updates, are B2B-owned events; it consumes B2B simulator artifacts, never B2B/Customer runtime source or databases.
- Delivery ordering does not prevent repository-local preparation against exact current artifacts.
- `Concertable/search` is the retained target; `search-next` is historical proof/local-folder vocabulary, not
  an actionable repository identity.
