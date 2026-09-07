Finish PR #633: https://github.com/Concertable/concertable/pull/633

Work in this exact worktree on the existing branch:
`Refactor/launch_deal-lifecycle-modules-phase2`

Read the repository `AGENTS.md` instructions first. This is a personal repo: commit and push the completed fix without asking. Do not touch `main`, create a new PR, or use a cross-service `ProjectReference`.

PR #633 is open, marked ready for review, and has auto-merge enabled. The sole objective is to get its CI green so GitHub can merge it.

The current CI run is https://github.com/Concertable/concertable/actions/runs/33331587219. All early boundary and local-platform package checks passed. `build` and `container-images` failed on the same two compiler errors in:

`api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Infrastructure/Services/ApplicationWorkflow.cs`

- line 152: `ct` does not exist in the current context;
- line 247: `ApplicationState` does not exist in the current context.

Inspect the enclosing methods and make the minimal correct source fix: use the method's actual cancellation-token parameter/name, and import/reference the existing `ApplicationState` type correctly. Do not paper over either error and do not redesign the lifecycle/save-failure work.

Preserve these settled rules:

- ordinary saves remain repository-owned where the workflow already has that repository;
- UoW `TrySaveChangesAsync(Func<DbUpdateException, bool>, ct)` is only the classified expected EF failure boundary;
- matching expected failures clear tracking and return `false`; unexpected failures and cancellation propagate;
- B2B/Payment consume DataAccess through packages only.

Run the focused affected tests or build. Then commit, push to the existing branch, and confirm #633 has a fresh green CI run with auto-merge still enabled. Preserve the user's untracked `handoff-save-pattern-investigation.md` and `scratchpad/`.
