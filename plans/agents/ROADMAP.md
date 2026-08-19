# How roadmaps work (`*_ROADMAP.md`)

What a roadmap *is* — an epic's living tracker with no ledger, never deleted, named `<EPIC>_ROADMAP.md` in
its own `plans/<epic>/` folder, spinning off a plan per buildable item and keeping the tick when that plan
is deleted — is the `plans` skill. So is the stable backticked `<epic>/<slug>` key that a spun-off ledger
records in `Roadmap item:`. This file is the rest: how they are kept current here, and what reads them. The
hub is [`../AGENTS.md`](../AGENTS.md); plans are [`PLAN.md`](PLAN.md).

## A plan's file must not cite the roadmap — but an agent working the plan may read it

The document dependency runs one way: a plan is spun off *from* a roadmap item and reports completion back
to it, and a plan that cites the roadmap couples the disposable artifact to the permanent one.

Reading is different from citing. The roadmap is also the epic's **cross-plan dependency map**, so an agent
*executing* a plan may read it to find which sibling plan owns a suspected blocker, then check that plan's
`_PROGRESS.md` for live status (see [`PLAN.md`](PLAN.md) "Cross-plan blockers"). Navigate by reading it;
never write a reference to it into the plan.

## Keep it current in the same commit as the work

`plans/launch/LAUNCH_ROADMAP.md` is the launch epic's roadmap and the driving doc for the current effort —
most work in this period traces back to one of its items. Whenever landed work affects anything it tracks —
a blocker shipped or partially shipped, a decision resolved, scope changed, a new blocker discovered —
update the relevant line **in the same commit** as the work, in its existing ✅/🔴/🟠/🟡 style, exactly like
ticking a phase in a plan. Don't defer it to a close-out pass.

## `/continue-roadmap` picks the next item

`/continue-roadmap [@plans/<X>_ROADMAP.md] [preferred item in natural language]` reads a roadmap and
classifies every outstanding item against real git/PR/worktree state: in flight, implementation-blocked,
delivery-gated but implementable, or ready and unowned. It offers every independently implementable
candidate; an unlanded PR, publication, or platform sync excludes an item only when it prevents safe local
implementation. The handoff tells a fresh context to write that item's feature plan. This is the epic-level
analog of `/resume-plan`; it **creates** a new plan rather than resuming one.
