# How roadmaps work (`*_ROADMAP.md`)

The topic playbook for **roadmaps**. A roadmap is the living progress tracker for one **epic** — a
large, multi-feature body of work (the launch, later the marketplace). It is a different artifact from
a plan ([`PLAN.md`](PLAN.md)); the hub is [`../AGENTS.md`](../AGENTS.md).

## A roadmap is not a plan

- **No `_PROGRESS.md` ledger.** Its ✅/🔴/🟠/🟡 checkboxes *are* its progress.
- **Never deleted as items complete** — it lives until the epic ships. (A plan, by contrast, is deleted
  when its lifecycle is terminal.)
- **Named `<EPIC>_ROADMAP.md`** and lives in its own epic folder `plans/<epic>/`, where every plan it
  spins off also lives (`plans/launch/LAUNCH_ROADMAP.md`, `plans/marketplace/MARKETPLACE_ROADMAP.md`),
  each tracking one epic.

## It spins off feature plans — it does not do the work itself

Each buildable roadmap item **spins off its own feature plan** (e.g. `VAT_PLAN.md` + its
`_PROGRESS.md`) under the normal plan lifecycle in [`PLAN.md`](PLAN.md). That plan owns the design,
phases and delivery; when it ships it **ticks its roadmap line**, then is deleted. The roadmap keeps the
tick.

Give every item that spins off a plan a stable backticked `<epic>/<slug>` key on its checklist line.
The new ledger records that exact key in `Roadmap item:` and the roadmap path in `Roadmap:`; this is
the machine-checkable roadmap→plan→ledger link. The disposable plan file still does not cite the
roadmap.

**A plan's file must not cite the roadmap — but an agent working the plan may read it.** The document
dependency runs one way: a plan is spun off *from* a roadmap item and reports completion back to it, and
a plan that cites the roadmap couples the disposable artifact to the permanent one. Reading is different
from citing: the roadmap is also the epic's **cross-plan dependency map**, so an agent *executing* a plan
may read it to find which sibling plan owns a suspected blocker and check that plan's `_PROGRESS.md` for
live status (see [`PLAN.md`](PLAN.md) "Cross-plan blockers"). Navigate by reading it; never
write a reference to it into the plan.

## Keep it current in the same commit as the work

`plans/launch/LAUNCH_ROADMAP.md` is the launch epic's roadmap and the driving doc for the current effort
(most work in this period traces back to one of its items). Whenever landed work affects anything it
tracks — a blocker shipped or partially shipped, a decision resolved, scope changed, a new blocker
discovered — update the relevant line **in the same commit** as the work (tick/strike/annotate in its
existing ✅/🔴/🟠/🟡 style), exactly like ticking a phase in a plan. Don't defer it to a close-out pass.

## `/continue-roadmap` picks the next item

`/continue-roadmap [@plans/<X>_ROADMAP.md] [preferred item in natural language]` reads a roadmap and
classifies every outstanding item against real git/PR/worktree state: in flight, implementation-blocked,
delivery-gated but implementable, or ready and unowned. It offers every independently implementable
candidate; an unlanded PR, publication, or platform sync excludes an item only when it prevents safe
local implementation. The handoff tells a fresh context to write that item's feature plan. This is the
epic-level analog of `/resume-plan`; it **creates** a new plan rather than resuming one.
