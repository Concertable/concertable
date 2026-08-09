# Semantic typed HTTP terminals progress

- Plan: `plans/typed-result/HTTP_RESULT_TERMINALS_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Refactor\typed-result_http-terminals`
- Branch: `Refactor/typed-result_http-terminals`
- PR: not opened
- Dependency/package gates: `plans/typed-result/REUNION_INTEGRATION_PROGRESS.md` owns the sole
  Shared.Api producer/publication sequence; this worktree supplies a verified local semantic-terminal
  checkpoint after Reunion Phase 2 publishes matching packages
- Last reconciled: 2026-08-09 against `origin/main` `c72b058af`, local head `1d261e3ce`, the preserved
  dirty correction set, in-progress review artifact, and merged Reunion PRs #443/#444

## Current state

Phase 1 replaces the generic Shared.Api terminal API with semantic `*OrProblem` Result terminals and
explicit Option absence terminals. The read-name convention now forbids `Get` prefixes on errors and
HTTP responses. The branch is 100 commits behind and one ahead of current main. Its working tree
preserves final corrections in `OptionHttpExtensions.cs`, focused tests, conventions, this plan, and
this ledger; the review artifact remains in progress. These paths must not be discarded or published
as a standalone package PR.

The merged Reunion plan changes the same Shared.Api package. This terminal work remains independently
owned as a local semantic checkpoint but is incorporated into the one Reunion Phase 3 Shared producer
after matching packages publish. Customer PR #425 consumes the resulting Phase 4 integrated baseline.

## Next Steps

Preserve the current dirty tree. Verify the final Option-terminal correction, Shared.Api tests,
Release solution build, and rename grep; commit the complete local Phase 1 checkpoint and finish the
code review. Do not push, open a PR, publish Shared.Api, merge current main, or change Customer source
in this worktree. Record the verified commit in the Reunion owner ledger, then wait for that plan's
Phase 2 package-publication gate so the checkpoint can be incorporated into Phase 3.

## Completed work

- Created this dedicated package-cutover plan and worktree from fresh `origin/main`.
- Identified `Concertable.Shared.Api` as the only package owner and Customer PR #425 as the first
  consumer migration; current main has no consumer source calls to the old terminal API.
- Replaced the Shared.Api terminal surface with `ToOkOrProblem`, `ToCreatedOrProblem`,
  `ToCreatedAtOrProblem`, `ToAcceptedOrProblem`, `ToNoContentOrProblem`, `ToOkOrNotFound`,
  and `ToOkOrNoContent`; added focused Option-terminal coverage and the operation/read naming
  convention.

## Verification

- `git grep` at `origin/main`: old terminal names occur only in Shared.Api implementation/tests.
- Package topology: Customer, B2B, Payment, and Search consume `Concertable.Shared.Api` through
  `ConcertablePlatformVersion` pins.
- `dotnet test api/Concertable.Shared/tests/Concertable.Shared.Api.UnitTests/Concertable.Shared.Api.UnitTests.csproj --configuration Release`: 60/60 passed.
- `dotnet build api/Concertable.slnx --configuration Release`: 0 errors, 6 existing warnings.
- Repository old-terminal grep: zero matches outside `bin`/`obj`.

## Decisions, discoveries, blockers, and deviations

- Shared.Api is HTTP infrastructure; Kernel remains HTTP-free.
- An Option never encodes a status. Each Option terminal names its specific None-status policy.
- A caller-visible failure reason belongs in an operation-owned `Result<TValue, TError>`, not `Option`.
- The Customer current-user lookup will migrate from Option to `CurrentUserError` after the integrated
  baseline; no Option terminal maps absence to an authentication status.
- The Reunion producer absorbs this semantic terminal checkpoint. A separate Shared.Api publish would
  duplicate the package and platform-sync boundary and is prohibited.

## Downstream handoffs

- This ledger waits for `plans/typed-result/REUNION_INTEGRATION_PROGRESS.md` to complete Phase 2
  package publication before its verified checkpoint is incorporated into Phase 3. Customer PR #425
  is now registered directly with the Reunion owner and waits for the Phase 4 generated sync.

## Event log

### 2026-08-09 — Reunion producer handoff registered

- Action: Reconciled the preserved HTTP-terminal implementation with merged Reunion planning PRs
  #443/#444 and moved package/publication ownership to the centralized integration plan.
- Evidence: local head `1d261e3ce`; fresh `origin/main` `c72b058af`; 100 behind / 1 ahead; five
  modified source/docs paths; in-progress review artifact; prior Shared.Api 60/60 and solution 0 errors.
- Outcome: this worktree may finish and review its local checkpoint immediately but must not publish
  independently. Reunion Phase 3 incorporates it after the Phase 2 package gate.
- Follow-up: preserve, verify, commit, and review the local checkpoint, then register its commit with
  the Reunion owner and wait.

### 2026-08-08 — package cutover planned

- Action: Audited the Shared.Api terminal surface and its published-package topology after the user
  directed a consistent semantic terminal convention.
- Evidence: current-main old-name grep has no service consumer source calls; Customer PR #425 adds
  three Result calls and three Option controller terminals; Customer, B2B, Payment, and Search pin the
  package through their service `Directory.Packages.props`.
- Outcome: The work is a two-leg package publish → platform-sync → Customer migration, not an atomic
  feature-branch edit.
- Follow-up: Implement and publish the Shared.Api owner leg.

### 2026-08-08 — Phase 1 implemented and verified

- Action: Replaced ambiguous Result terminals, added explicit Option terminals, and recorded the
  read-error/read-response naming rule.
- Evidence: Shared.Api Release 60/60; Release solution build 0 errors and 6 existing warnings; the
  old terminal-name grep is zero.
- Outcome: Phase 1 is complete locally and awaits its review/publish lifecycle; Customer PR #425 is
  registered for the later consumer migration.
- Follow-up: Commit, review, and publish the package owner change.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Refactor\typed-result_http-terminals
Read @plans/typed-result/HTTP_RESULT_TERMINALS_PLAN.md and @plans/typed-result/HTTP_RESULT_TERMINALS_PROGRESS.md and do what its `## Next Steps` says.
```
