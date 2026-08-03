# How plans work (`plans/*.md`)

Plans are **working docs for unfinished, multi-step work** — not an archive. Git history is the
archive. A finished plan kept "for reference" is rot: it misleads the next reader into thinking the
work is still pending. This file is the workflow; the root [`AGENTS.md`](../AGENTS.md) carries the
short version.

## Commit finished work the instant its gate goes green — this is unconditional

**The default is to commit, not to "leave it for review."** The moment a discrete chunk of work is
complete and its gate has passed (build green + the affected tests), **commit it, without asking.**
Committing is a local, reversible checkpoint; only `push` waits for Tommy's explicit go-ahead
(**commit ≠ push**).

This rule is **not** scoped to phase boundaries, to the "Before a clear" handoff below, or to any
other ritual — it is standing and applies to *every* finished, verified chunk: a phase, a bug fix, a
refactor, an investigation's output, a doc close-out. Reading the commit rule as conditional on
handing off / clearing is exactly the misread that produces the failure below.

**"I've left it uncommitted so you can look at it first" is the anti-pattern, not the courtesy:**

- **Review runs off commits.** `/code-review` diffs `main..HEAD`; work sitting in the working tree
  is invisible to it. Leaving it uncommitted is precisely what stops the reviewer seeing it.
- **Uncommitted is the fragile state.** It survives no `git checkout`, no stray `git restore`, no
  context clear. A commit is the cheapest insurance that exists.
- **It silently wrecks PR scoping.** Finished work left loose gets swept into an unrelated PR by a
  later `git add -A`, or stranded when the branch moves on — and a reviewer opening the PR finds the
  feature the branch is *named after* missing from it.

If finished work belongs in a **different PR** than the branch you're on, that is a reason to
**branch and commit it there** — never a reason to leave it sitting in the working tree.

**Mechanical trigger:** the gate went green → commit. If your next sentence would be *"should I
commit this?"* or *"I've left it uncommitted for your review"*, **that sentence is the trigger** —
don't send it, commit. The only thing you ever stop and ask about is `push`.

## Never leave the codebase out of sync — the plan isn't done until the whole thing is

A refactor isn't finished when the convenient half lands. If a repo/package boundary forces it into
multiple PRs (e.g. a Kernel change and the B2B change that depends on it), do them back-to-back —
but the plan stays open until **all** of them land and the codebase is in sync again. Merging the
B2B PR and calling the plan done while Kernel still speaks the old shape is the thing to never do.
Don't `git rm` the plan (Lifecycle 5) until that final synced state is in.

## Use the fewest safe merges

Complete a plan in the fewest PRs its real dependencies allow. Numbered steps, commits and phases do
not each need their own PR; keep coherent work together. Split only where a merge, package publication,
platform sync or runtime deployment must finish before the next work can build or run, and group all
work possible on each side of that gate.

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

Before any plan work, create a `Feature/<Name>` branch relevant to the plan if you're not already on one — never commit plan work to `main` or an unrelated branch.

## Shape of a plan

A plan describes a chunk of work too big for one commit, broken into **phases that are each
independently shippable and each end green**. A phase states what it changes, why, and its
verification gate. Phases sequence so that every intermediate state builds and passes.

## Companion progress ledger — record the whole operational history

Every new `plans/<NAME>.md` has a same-directory companion named
`plans/<NAME>_PROGRESS.md`, created with the plan. Keep design, intended phases, dependencies, and
definition-of-done in the plan. Put the detailed history and current operational truth in the
progress ledger so the plan remains readable. Start it from
[`resume-plan/assets/progress-template.md`](../.agents/skills/resume-plan/assets/progress-template.md).

The ledger must make the chat disposable at any point. Record **every project action and state
transition as it happens**, not just phase summaries: user direction and scope changes, partial
implementation, commits, verification commands and results, reviews and every
finding's disposition, fixes after review, PR creation and checks, merges, publications, platform
syncs, decisions, discoveries, deviations, blockers, failed approaches worth avoiding, and external
gates. "A review happened" is insufficient: identify the review type and range, its artifact, whether
findings remain open, and the commit or deferral that resolved each one. Never leave a project fact
only in chat because it happened between phase boundaries.

Every workflow that advances or evaluates plan-managed work owns this update before it ends. That
includes implementation, `/code-review`, `/big-review`, `/incremental-review`, addressing findings,
verification, committing, opening or updating a PR, merging, publishing, and platform sync. The fact
that another artifact or skill records part of the event does not make the progress update someone
else's later close-out.

Each ledger keeps these current sections above its chronological event log:

- plan, absolute worktree, branch, PR, and relevant dependency or package gates;
- current state, including partial/uncommitted work that the next agent must preserve;
- completed work with commit or PR evidence;
- verification and review state;
- decisions, discoveries, blockers, and deviations;
- **exact next action**, with any prerequisite that prevents it starting now.

Update the summary whenever an event changes it, then append the evidenced event to the log. Include
enough commands, paths, identifiers, results, and reasoning to continue without the prior conversation;
do not paste entire routine logs when their command, outcome, and durable artifact fully preserve the
fact. Commit ledger updates with the work they describe whenever possible. Remote-only transitions
that happen after a commit, such as a PR entering the queue or a package publishing, go into the next
immediate ledger checkpoint.

**Backward compatibility:** the absence of a progress ledger means the plan predates this rule, not
that no work has happened. Read the plan, git history and working tree, review artifacts, test evidence,
PR/check state, and package gates. Create `<NAME>_PROGRESS.md` with an explicitly labelled
"reconstructed baseline" containing only verifiable facts; do not invent missing history. All future
progress goes into that ledger.

The ledger is working state, not a permanent report. Completing and verifying the final local phase
does **not** close it while delivery is still live. Keep the plan and ledger discoverable until every
required review and finding, commit, verification, PR/check/merge, publication, dependency, and
platform-sync gate is terminal. Record the final gate's outcome and evidence before deleting both
artifacts. A plan with no later delivery or package gates can become terminal in its final phase
commit. A superseded or rejected plan closes immediately under Lifecycle 6. Git history remains the
archive.

## Lifecycle

1. **Write it** when the work spans multiple commits/PRs or needs a design decided up front, and create its `_PROGRESS.md` companion at the same time.
2. **Branch, then work a phase** — on the plan's `Feature/<Name>` branch (see "Branch first"), land the phase's commit(s).
3. **Check off / strike the shipped phase in the plan and update the progress ledger, in the same commit as the work.** A
   partially-done plan stays; only the outstanding work should remain un-ticked, so the next reader
   sees exactly what's left.
4. **Keep both artifacts after the last local phase while delivery is live.** Check off the phase and
   make the ledger's exact next action the review, fix, PR, merge, publication, dependency, or
   platform-sync gate that now owns progress. Continue recording every transition; local completion
   is not lifecycle completion.
5. **Close out only after the entire lifecycle is terminal.** Record the final gate's outcome and
   evidence, make that ledger checkpoint durable, then delete the plan and ledger together (`git rm`)
   in the following close-out change. When the final phase has no later delivery or package gates,
   that phase's completing commit may perform the close-out.
6. A plan **and its progress ledger** superseded by a newer plan, or describing a **rejected** design, are deleted the moment
   that's decided — no tombstones.

### The trap that ships a terminal plan as rot — check `git status` before the close-out change

Lifecycle 5 assumes the plan is already a **tracked** file you `git rm`. The case that slips through is a
plan **written and fully implemented in the same session** (a "fresh-context implementation plan"): it
exists only as an **untracked** working-tree file, so a blanket `git add -A` / `git add .` before the
close-out change **stages it as a new file** — the exact opposite of deleting it — and it ships inside
the PR as rot. This is precisely how `DISPLAYNAME_CONST_CONSOLIDATION.md` reached `main`'s PR: born and
completed in one commit, swept in as an addition instead of never being committed.

So, **before any change that closes a terminal plan, run `git status --short plans/` and eyeball it:**
- a plan closed by this change must **not** appear as `A`/`??` (born-and-done → never stage it, or `git rm`);
- a pre-existing tracked plan whose **final lifecycle gate is terminal** must appear as `D`, not survive untouched
  (that second miss is how `HTTP_GUARD_CONSOLIDATION.md` lingered after its arch-test shipped).
- the plan's `_PROGRESS.md` companion must also disappear; it must not survive or be added by the close-out change.

The rule is mechanical: after the close-out change, no terminal plan is in the tree — as an addition or a survivor.

## Doc-only close-out — never open its own PR; let it ride the next change

If no later delivery or package gate exists, the plan deletion + blocker tick can land **inside the
feature's final commit** (Lifecycle 5). Normally the plan must outlive that commit and often the merge
itself while reviews, checks, publication, or platform sync remain live. Record the final gate outcome
in the ledger and make that checkpoint durable before applying the close-out after the feature merged.
When that happens, **do not open a standalone PR for it.** Deleting a completed plan and ticking a
blocker are doc-only and cannot break a build or another PR (root `CLAUDE.md`: docs are exempt from
branch hygiene). Spinning up a branch + PR + full merge-queue **E2E cycle (~20-30 min)** for a two-file
doc change is pure waste — and pushing straight to the protected `main` is (correctly) blocked.

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
package republishes (on merge to main). Adding a *method* is safe (additive); changing *types
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

- **Plan markdown** — checked off / struck per the Lifecycle above (or `git rm`'d only when the whole
  lifecycle is terminal). The next reader should see exactly what's left and which delivery gate is live.
- **Progress markdown** — current through the last material event, with review and verification evidence
  plus one exact next action (or `git rm`'d with a terminal plan).
- **Memory** — if the phase changed a decision, convention, or fact worth carrying forward, update
  the relevant `CLAUDE.md` / `TECH_DEBT.md` so it survives independent of the chat.
- Anything else you'd be annoyed to re-derive (a command that worked, a gotcha hit) belongs in the
  commit message or the appropriate doc — not left implicit in the context.

Most of the time the context is then **compacted** (summarized, work continues) — but the same prep
makes it safe to **clear** (start fresh) instead. Either way, treat the end of every phase as the
point where the context becomes disposable. Don't carry unwritten state across a phase boundary.

## Before a clear — hand off a resume prompt

When the work is fully done for now, or the user says they'll clear, do these things after the durable
state is written:

1. **Commit the completed phase FIRST** — per the standing rule at the top of this file ("Commit
   finished work the instant its gate goes green"), with the plan check-off in the same commit
   (Lifecycle 3). By the time you reach a handoff this should already be done; if anything finished is
   still sitting in the working tree, that rule was missed, not deferred to here.
2. **Prepare for clear** — confirm the plan markdown, progress ledger, `CLAUDE.md`/`TECH_DEBT.md`, and commit messages
   hold everything; the chat must be safe to throw away.
3. **Give the user a resume prompt to paste after `/clear`.** Assume zero context: name the **working
   directory** (this repo runs many parallel worktrees — the branch name alone doesn't say which tree to
   `cd` into, so give the absolute path: the main checkout, or the sibling `…worktrees/<Branch>`), the
   plan and progress files, the branch/PR, and the exact next step. Tell the next agent to read both
   files before acting. Keep it to a few lines.
4. **Before any implementation PR is merged, hand off a ready-to-paste `/code-review` prompt in the
   same turn, or `/big-review` when the branch is too large for one review pass.** Review once per PR,
   not once per numbered plan step or commit. The prompt must name the exact worktree path and branch.
   If code commits are added after review, run `/incremental-review` before merge.

   **The resume prompt's first line MUST be the exact directory to `cd` into, before anything else.**
   This applies to the main checkout and every separate worktree. A `/clear` (or a brand-new
   session) reopens in *whatever directory the last session was rooted in*. When the branch lives in a
   separate worktree that's routinely the **wrong** one: a resume prompt naming only the plan and the
   branch lands the paste in the wrong worktree on the wrong branch, and git then **won't** let you
   check the right branch out (it's already checked out in its own worktree) — so the whole handoff
   silently derails. Naming the branch is **not** enough; name the **directory**. Shape:

   > `cd <absolute worktree path>` — then: read `plans/<PLAN>.md` and `plans/<PLAN>_PROGRESS.md`, branch `<Type>/<Name>`, next step: …

   Check first: run `git worktree list`, resolve the branch's exact worktree, and lead with that absolute
   `cd` path. Never assume a new session opens in the intended checkout.

   The `cd` must sit **inside the prompt text the user pastes**, not on a line above it — a prompt is
   pasted as one blob, so a path parked outside it is simply lost.

   **Hand over exactly ONE prompt: the immediate next action.** A later phase gated behind a merge,
   publish, or platform-sync is named as a *gate* ("Phase 2 waits on the sync"), never handed over as a
   second ready-to-run prompt. Two prompts read as a menu, and the obvious way to "save time" on a menu
   is to run both at once — which for a publish-gated phase means restoring a package version that isn't
   on the feed yet (`NU1101`). The gate exists precisely because the two can't overlap.

**The trigger is mechanical — not a judgment call.** The moment you finish a discrete chunk of work
(a plan designed, a phase landed, a question fully answered) and your next sentence *would* be
"Want me to start X?" / "Shall I do the next phase?" / "…or leave it as-is?" — **that question IS the
trigger.** Do not send it. Write the durable state and hand off the resume prompt in the **same
turn**, unprompted. You do not wait for the user to say "I'll clear now" — finishing the work is the
signal. Asking whether to continue instead of handing off is the exact anti-pattern this section
exists to kill: the plan and progress ledger already record what's next, so the honest move is to
hand off, not to ask.

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

**When you skip E2E on a change headed to a PR, tell the merge queue too — `Skip-E2E: true` trailer.**
The queue runs the full E2E suite on every code change *by default*, so your local skip-judgment is
worthless unless it's encoded in the commit: without the trailer the queue still burns ~25-30 min of E2E
on a change that didn't earn it. So for a behaviour-preserving / small / well-covered change, add the
trailer `Skip-E2E: true` (own line, end of a commit message; any commit in the PR range —
`Skip-E2E-UI: true` for UI-only). Unit and integration tests never skip for code/package changes, and
build + carve never skip. It's a real git trailer parsed by git, not a `[bracketed]` token, so prose can't
trip it; a same-named PR label (`skip-e2e`) works too. This is the reflex-inversion: E2E-in-the-queue is
opt-*out* for a zero-behaviour-change PR, not automatic. Retrofitting the trailer onto a PR already in the queue means
closing + re-pushing (the branch is locked while queued) — so decide the tier **in the commit you push**,
not after.

When in doubt, or when a phase explicitly flips behavior on a covered flow, run E2E. **How** to run it
safely (the mandatory `./scripts/docker-health.ps1` pre-flight, only via the `e2e-*` skills) is unchanged —
see the "E2E suites — Docker health first" section in `CLAUDE.md`. This section governs **whether**,
that one governs **how**.

A phase's own "verification gate" line may name E2E; treat that as "run E2E *if* this phase meets the
massive/risky bar above," not as an unconditional requirement for every phase.
