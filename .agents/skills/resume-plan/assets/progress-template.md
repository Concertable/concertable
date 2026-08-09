# <Plan title> progress

- Plan: `<repo-relative plan path, plans/<epic>/<NAME>_PLAN.md>`
- Worktree: `<absolute worktree path>`
- Branch: `<Type/<epic>_<name>, matching the <NAME>_PROGRESS stem>`
- PR: `<number and URL, or not opened>`
- Dependency/package gates: `<state, or none>`
- Last reconciled: `<date/time and evidence source>`

## Current state

<What exists now, including partial or uncommitted work that must be preserved.>

## Next Steps

<The single resolved action for the next agent, expressed as concrete, self-contained steps. If no
action can proceed, start with three single-line fields: `Blocked: <exact unmet gate>`,
`Unblock action: <what must be done, by whom or where>`, and `Resume when: <objective evidence>`.
Apply the repository's standing instructions and current evidence before writing it. Actionable
resume/handoff prompts point here instead of restating it; blocked plans never emit their own pointer.>

## Completed work

<Completed phases and changes with commit/PR evidence.>

## Verification

<Commands/checks, outcomes, and the code state they verified.>

## Reviews

<Review type and range, artifact, findings, dispositions, and resulting commits.>

## Decisions, discoveries, blockers, and deviations

<Durable context that cannot be reconstructed safely from code alone.>

## Event log

### <YYYY-MM-DD — event>

- Action: <what happened>
- Evidence: <commit, command/result, artifact, PR/check, or package state>
- Outcome: <what changed>
- Follow-up: <remaining consequence or none>

## Resume prompt

<The single cd-first pointer to hand off after `/clear` when `## Next Steps` is actionable. Never emit
it while the ledger carries the blocked-state fields. It ONLY points at this ledger's `## Next Steps`;
never restate the branch, gates, checkpoints, or steps here — they live in `## Next Steps` and must not
be duplicated where they can drift. Keep it verbatim in this fenced block:>

```
cd <absolute worktree path>
Read @plans/<PLAN>_PLAN.md and @plans/<PLAN>_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
