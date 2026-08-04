# How roadmaps work (`*_ROADMAP.md`)

The topic playbook for **roadmaps**. A roadmap is the living progress tracker for one **epic** — a
large, multi-feature body of work (the launch, later the marketplace). It is a different artifact from
a plan ([`PLAN.md`](PLAN.md)); the hub is [`../AGENTS.md`](../AGENTS.md).

## A roadmap is not a plan

- **No `_PROGRESS.md` ledger.** Its ✅/🔴/🟠/🟡 checkboxes *are* its progress.
- **Never deleted as items complete** — it lives until the epic ships. (A plan, by contrast, is deleted
  when its lifecycle is terminal.)
- **Named `<EPIC>_ROADMAP.md`**, so a second epic gets its own (`LAUNCH_ROADMAP.md`,
  `MARKETPLACE_ROADMAP.md`), each tracking one epic.

## It spins off feature plans — it does not do the work itself

Each buildable roadmap item **spins off its own feature plan** (e.g. `VAT_PLAN.md` + its
`_PROGRESS.md`) under the normal plan lifecycle in [`PLAN.md`](PLAN.md). That plan owns the design,
phases and delivery; when it ships it **ticks its roadmap line**, then is deleted. The roadmap keeps the
tick.

**Don't reference a roadmap from inside a plan.** The dependency runs one way: a plan is spun off *from*
a roadmap item and reports completion back to it. A plan that cites the roadmap couples the disposable
artifact to the permanent one.

## Keep it current in the same commit as the work

`plans/b2b/LAUNCH_ROADMAP.md` is the launch epic's roadmap and the driving doc for the current effort
(most work in this period traces back to one of its items). Whenever landed work affects anything it
tracks — a blocker shipped or partially shipped, a decision resolved, scope changed, a new blocker
discovered — update the relevant line **in the same commit** as the work (tick/strike/annotate in its
existing ✅/🔴/🟠/🟡 style), exactly like ticking a phase in a plan. Don't defer it to a close-out pass.

## `/continue-roadmap` picks the next item

`/continue-roadmap` reads a roadmap, classifies every outstanding item against real git/PR/worktree
state (in-flight / blocked / ready), and — once Tommy picks one — emits a handoff prompt that a fresh
context uses to write that item's feature plan. It is the epic-level analog of `/resume-plan`; it
**creates** a new plan rather than resuming one.
