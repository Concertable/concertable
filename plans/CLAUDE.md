# How plans work (`plans/*.md`)

Plans are **working docs for unfinished, multi-step work** — not an archive. Git history is the
archive. A finished plan kept "for reference" is rot: it misleads the next reader into thinking the
work is still pending. This file is the workflow; the root [`CLAUDE.md`](../CLAUDE.md) carries the
short version.

## Never leave the codebase out of sync — the plan isn't done until the whole thing is

A refactor isn't finished when the convenient half lands. If a repo/package boundary forces it into
multiple PRs (e.g. a Kernel change and the B2B change that depends on it), do them back-to-back —
but the plan stays open until **all** of them land and the codebase is in sync again. Merging the
B2B PR and calling the plan done while Kernel still speaks the old shape is the thing to never do.
Don't `git rm` the plan (Lifecycle 4) until that final synced state is in.

### Rename definition-of-done: the grep gate (mechanical, not judgement)

A rename is done **only when `grep -rniE "<oldterm>"` over the entire repo returns zero** — every
tier, no exceptions: type names, identifiers **of every case** (PascalCase, camelCase, snake, kebab),
local vars, fields, params, comments, string literals, `data-testid`s, HTTP routes, JSON keys, blob
paths, file/folder names, and docs. There is **no "cosmetic tier."** A field typed `IDealAccessor`
named `contractAccessor`, or a comment still saying "the agreement", is *not done* — it's exactly the
dishonest naming the rename exists to remove.

Run the grep before claiming done. The only acceptable non-zero is an **explicit, written allowlist**
of deliberate survivors, each justified in the plan — a term that legitimately means something else
now (a word reused for a different concept), a shared `.Contracts` event package, a wire value kept on
purpose. Anything not on that allowlist is outstanding work. "I renamed the types, the build's green,
tests pass" is **not** the bar — the grep is. Don't decide by hand which occurrences "count"; the whole
failure mode is discretion. Remove the discretion — grep, allowlist, zero.

## LAUNCH_PLAN.md is the master tracker — keep it current with every change it tracks

[`plans/b2b/LAUNCH_PLAN.md`](./b2b/LAUNCH_PLAN.md) is the driving doc for the current launch
effort (a weeks-to-months horizon; most work in this period traces back to one of its items).
Whenever landed work affects anything it tracks — a blocker shipped or partially shipped, a
decision resolved, scope changed, a new blocker discovered — update the relevant LAUNCH_PLAN line
**in the same commit** as the work (tick/strike/annotate in its existing ✅/🔴/🟠/🟡 style), exactly
like ticking a phase in a feature plan. Don't defer it to a close-out pass. Unlike ordinary plans
it is **not** deleted as items complete — it lives until launch.

## Branch first

Before any plan work, create a `Feature/<Name>` branch relevant to the plan if you're not already on one — never commit plan work to `master` or an unrelated branch.

## Shape of a plan

A plan describes a chunk of work too big for one commit, broken into **phases that are each
independently shippable and each end green**. A phase states what it changes, why, and its
verification gate. Phases sequence so that every intermediate state builds and passes.

## Lifecycle

1. **Write it** when the work spans multiple commits/PRs or needs a design decided up front.
2. **Branch, then work a phase** — on the plan's `Feature/<Name>` branch (see "Branch first"), land the phase's commit(s).
3. **Check off / strike the shipped phase in the plan, in the same commit as the work.** A
   partially-done plan stays; only the outstanding work should remain un-ticked, so the next reader
   sees exactly what's left.
4. **Delete the plan** (`git rm`) in the commit that completes its *last* phase — never defer deletion
   to a later cleanup pass.
5. A plan **superseded** by a newer plan, or describing a **rejected** design, is deleted the moment
   that's decided — no tombstones.

### The trap that ships a finished plan as rot — check `git status` before the completing commit

Lifecycle 4 assumes the plan is already a **tracked** file you `git rm`. The case that slips through is a
plan **written and fully implemented in the same session** (a "fresh-context implementation plan"): it
exists only as an **untracked** working-tree file, so a blanket `git add -A` / `git add .` before the
completing commit **stages it as a new file** — the exact opposite of deleting it — and it ships inside
the PR as rot. This is precisely how `DISPLAYNAME_CONST_CONSOLIDATION.md` reached `master`'s PR: born and
completed in one commit, swept in as an addition instead of never being committed.

So, **before any commit that completes plan work, run `git status --short plans/` and eyeball it:**
- a plan finished by this commit must **not** appear as `A`/`??` (born-and-done → never stage it, or `git rm`);
- a pre-existing tracked plan whose **last** item this commit lands must appear as `D`, not survive untouched
  (that second miss is how `HTTP_GUARD_CONSOLIDATION.md` lingered after its arch-test shipped).

The rule is mechanical: after the completing commit, no finished plan is in the tree — as an addition or a survivor.

## Doc-only close-out — never open its own PR; let it ride the next change

Ideally the plan deletion + blocker tick land **inside the feature's final commit** (Lifecycle 4). But
sometimes the plan has to outlive the merge — e.g. it was the live working doc while the PR was still
being debugged in the merge queue — so the close-out only happens *after* the feature already merged.
When that happens, **do not open a standalone PR for it.** Deleting a completed plan and ticking a
blocker are doc-only and cannot break a build or another PR (root `CLAUDE.md`: docs are exempt from
branch hygiene). Spinning up a branch + PR + full merge-queue **E2E cycle (~20-30 min)** for a two-file
doc change is pure waste — and pushing straight to the protected `master` is (correctly) blocked.

So: make the close-out edits and **leave them in the working tree** to ride along with the next PR that
lands (or bundle them into the next commit). The same goes for any tiny, non-breaking doc/markdown
tweak — `TECH_DEBT.md` lines, scratch notes, blocker ticks: never a dedicated PR, just let it travel
with the next real change.

## Boundary-blocked refactors — capture in a plan, don't force into this PR

Cross-service deps go through **published packages**, not project references (the carve — see
[`api/ARCHITECTURE.md`](../api/ARCHITECTURE.md)). B2B and Customer compile against the *published*
`Concertable.*` packages (e.g. `Payment.Client`, `Payment.Contracts`), not the source sitting next to
them in the solution. So a refactor that **changes a published contract** — renaming/removing a public
type consumers use, changing a return type, moving a DTO between packages — is a **breaking package
change**: it can't build/land in one PR, because the consumers won't see the new shape until the
package republishes (on merge to master). Adding a *method* is safe (additive); changing *types
consumers already use* is not (no back-compat shim for a return-type change → expand/contract across
merges).

When you hit one of these mid-feature: **don't force it into the current PR, and don't derail the
feature to do it.** Capture it in a dedicated plan (design + the expand/contract steps + why it's
multi-merge), do the safe/additive part the feature needs now, and reference the plan from your commit.

(If the boundary friction itself is being questioned — "is the polyrepo sim worth it yet?" — that's an
architecture decision for the root, not something to resolve inside a feature PR either.)

## End of a phase — leave it compaction/clear-safe

A phase isn't done when the code is green; it's done when **nothing important lives only in the chat
context**. After the phase's commit lands and its gate passes, get all durable state written down so
the conversation can be thrown away without losing anything:

- **Plan markdown** — checked off / struck per the Lifecycle above (or `git rm`'d if it was the last
  phase). The next reader should see exactly what's left and nothing that already shipped.
- **Memory** — if the phase changed a decision, convention, or fact worth carrying forward, update
  the relevant `CLAUDE.md` / `TECH_DEBT.md` so it survives independent of the chat.
- Anything else you'd be annoyed to re-derive (a command that worked, a gotcha hit) belongs in the
  commit message or the appropriate doc — not left implicit in the context.

Most of the time the context is then **compacted** (summarized, work continues) — but the same prep
makes it safe to **clear** (start fresh) instead. Either way, treat the end of every phase as the
point where the context becomes disposable. Don't carry unwritten state across a phase boundary.

## Before a clear — hand off a resume prompt

When the work is fully done for now, or the user says they'll clear, do two things after the durable
state is written:

1. **Prepare for clear** — confirm the plan markdown, `CLAUDE.md`/`TECH_DEBT.md`, and commit messages
   hold everything; the chat must be safe to throw away.
2. **Give the user a resume prompt to paste after `/clear`.** Assume zero context: name the **working
   directory** (this repo runs many parallel worktrees — the branch name alone doesn't say which tree to
   `cd` into, so give the absolute path: the main checkout, or the sibling `…worktrees/<Branch>`), the
   plan file, the branch/PR, and the exact next step. Keep it to a few lines.

**The trigger is mechanical — not a judgment call.** The moment you finish a discrete chunk of work
(a plan designed, a phase landed, a question fully answered) and your next sentence *would* be
"Want me to start X?" / "Shall I do the next phase?" / "…or leave it as-is?" — **that question IS the
trigger.** Do not send it. Write the durable state and hand off the resume prompt in the **same
turn**, unprompted. You do not wait for the user to say "I'll clear now" — finishing the work is the
signal. Asking whether to continue instead of handing off is the exact anti-pattern this section
exists to kill: the plan already records what's next, so the honest move is to hand off, not to ask.

**A completed + verified phase is a HARD STOP — end the turn on the resume prompt.** After you hand off,
do **not** begin the next phase in the same session — not even when asked — **unless the user explicitly
names the next phase AND says to do it now, in this session.** Everything short of that is a stop:

- A vague *"why are we stopping?"* / *"aren't we continuing?"* / *"keep going"* / *"yeah"* is a request
  to **clarify or re-show the handoff**, never license to start the next phase. Re-present the resume
  prompt; don't start coding.
- **Never** append *"want me to continue?"* or a two-way *"continue vs. review"* fork — that reopens
  "continue" as an option and is precisely how the stop gets skipped. The resume prompt **is** the
  deliverable; the turn ends there. At most, one plain sentence noting the phase is done — no question.
- When genuinely unsure whether the user authorized the next phase in-session, **stop and ask one
  narrow question**; default to stopping, never to starting.

## Verification gate per phase

Every phase, no exceptions:

- `dotnet build api/Concertable.slnx` green (0 errors).
- The **affected** module's unit + integration tests — run them via the `integration-debug` skill.
- Phases that change the model end with `./initial-migrations.ps1` from `api/` (re-scaffold, never
  additive migrations).
- **Final phase only:** run the UI E2E suite via the `e2e-ui-debug` skill.

## A failing test is never just reported — enter the matching debug skill and drive it to green

Whenever **any** test run comes back red — unit, integration, API E2E, or UI E2E — the next action is
**always** the matching debug skill, not a status report back to the user. A failure *is* the trigger;
don't make the user ask for it.

- unit / integration failures → **`integration-debug`**
- API E2E failures → **`e2e-api-debug`**
- UI E2E failures → **`e2e-ui-debug`** (or **`e2e-debug`** to sweep both layers)

The debug skill owns the whole **run → diagnose → fix → re-run** loop: find the root cause, fix it in
code (service / handler / page-object / step-def / fixture / config — wherever the real bug is), and
re-run until green. Never report a red suite and wait. Never disable, skip, `--no-build`, or
inflate-a-timeout to get past a failure — that's bypassing, not fixing. For E2E, the debug skill also
does **flaky-vs-real triage**: re-run the failed scenario alone on a fresh stack — passes clean = a
host-load blip (proven, not assumed); fails again = a real bug, so fix it.

## When to run the E2E suites — judgment, not reflex

The full E2E suites (API `Concertable.B2B.E2ETests` + the UI regress) are **expensive and
Docker-gated**. Run them only when the change earns it; otherwise build + unit + integration is the
gate, and you update the plan markdown and move on.

**The PR merge queue IS the E2E gate — never run E2E locally ahead of a merge.** When the change is
going out as a PR, the merge-queue pipeline runs the full suite (E2E included) as the gate. Running it
locally first just burns ~25-30 min duplicating exactly what CI will do on the way in. So for anything
headed to a PR, the local gate stops at build + unit + integration — **push it and let the queue run
E2E.** The **only** reason to run E2E locally is when **the merge fails on failing E2E tests** — then
run the failing scenarios via the **`e2e-ui-debug`** / **`e2e-debug`** skill to diagnose and fix, and
push the fix (the queue re-runs E2E on the way back in). **This overrides any plan phase line or
kickoff prompt that says "run the E2E regress"** — if a PR will run it, let the PR run it; a written
"run E2E" step is not a reason to duplicate the queue.

**Run E2E when the change is _massive_ or _risky_:**

- It spans multiple services or is otherwise broadly cross-cutting.
- It changes **user-facing or runtime behavior** in a flow E2E covers — registration/login, payments
  & payouts, settlement, the event/projection chain, messaging.
- It's the kind of change that's **likely to break something and you'd want to debug it first** —
  i.e. you're not confident unit + integration fully covers the blast radius.

**Skip E2E (just build + unit + integration, update the markdown, continue) when:**

- It's foundational / stage-1 implementation with **zero behavior change** (a new table + seam that
  nothing exercises yet).
- It's small, isolated, or covered well by integration tests.
- It's doc-only or comments-only.

When in doubt, or when a phase explicitly flips behavior on a covered flow, run E2E. **How** to run it
safely (the mandatory `./docker-health.ps1` pre-flight, only via the `e2e-*` skills) is unchanged —
see the "E2E suites — Docker health first" section in `CLAUDE.md`. This section governs **whether**,
that one governs **how**.

A phase's own "verification gate" line may name E2E; treat that as "run E2E *if* this phase meets the
massive/risky bar above," not as an unconditional requirement for every phase.
