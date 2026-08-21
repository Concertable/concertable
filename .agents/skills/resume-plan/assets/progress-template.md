# <Plan title> progress

- Plan: `<repo-relative plan path, plans/<epic>/<NAME>_PLAN.md>`
- Roadmap: `<repo-relative roadmap path, plans/<epic>/<EPIC>_ROADMAP.md>`
- Roadmap item: `<stable epic/slug key carried by the roadmap checklist item>`
- Worktree: `<current delivery worktree path, or none after merged cleanup>`
- Branch: `<current delivery branch, or next proposed Type/<epic>_<name>>`
- PR: `<number and URL, or not opened>`
- Dependency/package gates: `<state, or none>`
- Last reconciled: `<date/time and evidence source>`

## Current state

<What exists now, including partial or uncommitted work that must be preserved.>

## Next Steps

<The single resolved action for the next agent, expressed as concrete, self-contained steps. If no
action can proceed, start with four single-line fields: `Blocked: <exact unmet gate>`,
`Blocked by: <owning ledger path or external owner>`, `Unblock action: <what must be done, by whom or
where>`, and `Resume when: <objective evidence>`; or, when only a human decision remains, a single
`Paused: <who> — <action and observable resume condition>` line.
Apply the repository's standing instructions and current evidence before writing it. Never write a
merge as the next step until a review is recorded — `/review` is the first delivery gate. Actionable
resume/handoff prompts point here instead of restating it; blocked and paused plans never emit their own pointer.>

## Completed work

<Compact milestones only: normally one item per completed phase or delivery gate, with commit/PR evidence.>

## Verification

<Only the latest commands/checks still valid for the current candidate, with outcomes and tested state.>

## Reviews

<Current review type/range/artifact and open findings. Once clean, collapse to the reviewed state and
resulting commits; leave detailed history in the review artifact and git.>

## Decisions, discoveries, blockers, and deviations

<Only durable context that still affects execution and cannot be reconstructed safely from code or
named artifacts.>

## Recent transitions

<Optional temporary working memory. Keep only material transitions not yet fully represented above.
Delete or collapse each entry once its outcome is folded into the snapshot; omit this section when empty.>

## Resume prompt

<The single cd-first pointer to hand off after `/clear` when `## Next Steps` is actionable. Never emit
it while the ledger carries the blocked-state fields or a `Paused:` line. It ONLY points at this ledger's `## Next Steps`;
never restate the branch, gates, checkpoints, or steps here — they live in `## Next Steps` and must not
be duplicated where they can drift. Keep it verbatim in this fenced block:>

```
<cd existing-worktree OR /open-worktree Type/epic_name>
Read @plans/<PLAN>_PLAN.md and @plans/<PLAN>_PROGRESS.md and do what its `## Next Steps` says.
```
