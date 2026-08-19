# How plans work here (`plans/*.md`)

**What a plan is and its lifecycle in the abstract is the `plans` skill** — git history as the archive, the
progress ledger as a compact rolling snapshot rather than an append-only history, plans outliving their PR
worktrees, the implementation-versus-delivery split, cross-plan blockers as two-way handoffs, finishing and
superseding, and the rename grep gate. Read it first.

This file is the Concertable half: the ledger sections `/resume-plan` and `plan_graph` depend on, the exact
blocker schema the Stop hook enforces, and the commands each lifecycle step runs. The hub
[`../AGENTS.md`](../AGENTS.md) carries the scripts and hooks; roadmaps are [`ROADMAP.md`](ROADMAP.md).

## Shape of a plan

A plan describes a chunk of work too big for one commit, broken into **phases that are each independently
shippable and each end green**. A phase states what it changes, why, and its verification gate. Phases
sequence so that every intermediate state builds and passes.

## What a ledger must contain

Every `plans/<epic>/<NAME>_PLAN.md` has at least one same-directory `<NAME>_PROGRESS.md` companion, created
with the plan and started from
[`resume-plan/assets/progress-template.md`](../../.agents/skills/resume-plan/assets/progress-template.md).
**A plan may have several ledgers** — one per parallel workstream, each named for that workstream; the
`- Plan:` header is the authoritative plan↔ledger grouping (`/resume-plan` greps it), not the filename.

Required headers: `Plan:`, `Roadmap:`, and the stable `<epic>/<slug>` `Roadmap item:` key. Together they
form the roadmap→plan→ledger graph that `plan_graph.py` checks. Reconstruct them from the plan and roadmap
when adopting a legacy ledger.

Each ledger keeps these current sections, plus an optional short `## Recent transitions`:

- plan, absolute worktree, branch, PR, and relevant dependency or package gates;
- current state, including partial/uncommitted work the next agent must preserve;
- completed milestones, normally one concise item per phase or delivery gate with commit/PR evidence;
- the latest verification and review state still valid for the current candidate, including every open
  finding and its disposition;
- decisions, discoveries, blockers, and deviations that still affect execution and cannot be safely
  reconstructed from code or durable artifacts;
- **`## Next Steps`** — the single resolved action for the next agent, as concrete self-contained steps. If
  no action can proceed, start it with the four blocker fields below. This is the **single source of truth**
  for what to do next; resume/handoff prompts point here instead of restating it.

`## Recent transitions` is temporary working memory for a change not yet represented by the stable sections.
Omit it when empty; delete each entry as soon as its outcome is folded in. The retention test for anything
else: **could removing this fact cause a fresh agent to take the wrong action or repeat a costly failed
approach?** If not, remove it.

**Every workflow that advances or evaluates plan-managed work owns this update before it ends** — including
`/review`, `/big-review`, `/incremental-review`, `/address-review`, verification, committing, opening or
updating a PR, `/merge`, publication, and platform sync. That another skill records part of the event does
not make the ledger update someone else's later close-out. The mandatory procedure is
[`resume-plan/references/plan-progress-checkpoint.md`](../../.agents/skills/resume-plan/references/plan-progress-checkpoint.md);
apply it directly for plan-aware work with no skill wrapper.

## Never leave the codebase out of sync — the plan isn't done until the whole thing is

A refactor isn't finished when the convenient half lands. If a package boundary forces it into multiple PRs
(e.g. a Kernel change and the B2B change that depends on it), do them back-to-back — but the plan stays open
until **all** of them land and the codebase is in sync again. Merging the B2B PR and calling the plan done
while Kernel still speaks the old shape is the thing to never do.

## The delivery state vocabulary

Use these states consistently in roadmaps, plans, ledgers, reports, and handoffs:

- **implementation-blocked** — a required source/API/design or trustworthy exact artifact is unavailable;
- **implementable, delivery-gated** — local implementation can proceed, but the branch cannot merge yet;
- **delivery-ready** — implementation, tests, and review are green against the recorded exact producer
  artifact; published-baseline revalidation remains;
- **merge-ready** — temporary inputs are gone and the branch is green against the real published baseline;
- **terminal** — all required merge, publication, sync, and closeout gates are complete.

When an exact local package is sufficient, record its producer commit, package version, hashes, and
reproducible location; never commit a machine-specific feed path, temporary version pin, or local-only
configuration.

## Cross-plan blockers — the return path is a named ledger section

Read the epic roadmap to find which sibling plan owns the dependency and open that plan's `_PROGRESS.md` for
its live state; don't guess from memory. Then, for an edge that genuinely blocks *implementation*:

1. the waiting ledger records the blocker fields below and stops polling;
2. every ledger named by `Blocked by:` gains a **`## Downstream handoffs`** entry with the waiting ledger,
   its worktree, and the same gate — this exact heading is the durable return path `plan_graph.py` checks;
3. when the owner crosses the gate, it updates and compacts the waiting ledger in that same session and
   surfaces its resume prompt to Tommy;
4. an owner plan/ledger is never closed while a downstream handoff remains undispatched.

## Hard blockers — hand off the resolver, never the blocked plan

If safe authorized work in the current session can remove the obstacle, or local implementation can proceed
while delivery waits, do that work; it is not a hard blocker. Otherwise `## Next Steps` must begin with four
single-line fields, reported verbatim in the final response:

```text
Blocked: <the exact unmet gate>
Blocked by: <the owning plans/..._PROGRESS.md ledger, or the external owner>
Unblock action: <what must be done, by whom or where>
Resume when: <the objective evidence that proves the gate opened>
```

When the blocker or user action concerns an existing PR — especially permission to make it ready or merge
it — include the clickable `[PR #<number>](<url>)` link in the relevant field and in the report. **Never ask
for merge permission with only a bare PR number.** Route the resolving work by ownership:

- existing PR, plan, or session → register the downstream handoff, name the owner, emit no prompt;
- no owner and a separate context is appropriate → emit a paste-ready dispatch prompt for the resolver,
  including the blocked ledger and the condition it unlocks. This is not the blocked plan's pointer;
- user or external action only → replace the four fields with one
  `Paused: <who> — <action and observable resume condition>` line and give that action directly.

Do not create a checkpoint merely to prove an unchanged blocker is still blocked.

## Lifecycle

1. **Write it** when the work spans multiple commits/PRs or needs a design decided up front. Before creating
   its ledger/worktree, check existing branches, worktrees, PRs, and ledgers for the same work, then assign
   each logical workstream exactly one canonical ledger; never create a second implementation owner.
2. **Branch, then work a delivery slice** — create a worktree from current `origin/main`. That
   branch/worktree carries one PR-sized slice, not the lifetime of the plan.
3. **Check off / strike the shipped phase in the plan and update the ledger, in the same commit as the
   work.** A partially-done plan stays; only outstanding work should remain un-ticked.
4. **Keep both artifacts after the last local phase while delivery is live.** Make the ledger's exact next
   action the gate that now owns progress — **`/review` comes first: never write a merge as the next step
   until a review is recorded** (a `## Reviews` entry or a review watermark); then PR, merge, publication,
   dependency, or platform sync. `plan_graph` enforces this. Once the PR merges, close its worktree with
   `./scripts/worktrees.ps1 close -PlanManaged`; if work remains, create a fresh worktree from current
   `origin/main` and resume the same ledger.
5. **Close out only after the entire lifecycle is terminal.** Record the final gate's outcome and evidence,
   make that ledger checkpoint durable, then delete the plan and ledger together (`git rm`) in the
   *following* commit. Land that commit through `/merge-docs`, then remove the close-out worktree. The
   source PR never deletes its own recovery artifacts.

### Check `git status` before the close-out commit

Lifecycle 5 assumes the plan is already a **tracked** file you `git rm`. The case that slips through is a
plan written and fully implemented in the same session: it exists only as an **untracked** working-tree
file, so a blanket `git add -A` before the close-out commit **stages it as a new file** — the exact opposite
of deleting it — and it ships inside the PR as rot.

So run `git status --short plans/` and eyeball it:

- a plan closed by this change must **not** appear as `A`/`??` (born-and-done → never stage it, or `git rm`);
- a tracked plan whose final lifecycle gate is terminal must appear as `D`, not survive untouched;
- its `_PROGRESS.md` companion must disappear too, and must not be added by the close-out change.

Mechanically: after the close-out commit, no terminal plan is in the tree — as an addition or a survivor.

### Post-merge continuation — delete the PR worktree, resume from `main`

The final pushed PR head carries the plan and ledger recovery state. After the PR is confirmed merged,
switch to another checkout and run:

```powershell
./scripts/worktrees.ps1 close -Worktree <path> -PullRequest <n> -PlanManaged
```

The command refuses post-PR commits, dirty paths, PR/head mismatches, missing merged ledgers, casing
collisions, and persistent worktrees. One merged PR ends one worktree, while the plan remains on `main`.

If implementation continues, recreate its worktree from current `origin/main` and resume the same ledger. If
only remote gates or final documentation remain, create `Docs/<epic>_<name>_closeout` from current
`origin/main`, record terminal evidence there, then delete the plan and ledger together and land through
`/merge-docs`.

### Boundary-blocked refactors — capture in a plan, don't force into this PR

Cross-service deps go through **published packages**, not project references (the carve — see
[`api/ARCHITECTURE.md`](../../api/ARCHITECTURE.md)). B2B and Customer compile against the *published*
`Concertable.*` packages (e.g. `Payment.Client`, `Payment.Contracts`), not the source sitting next to them
in the solution. So a refactor that **changes a published contract** — renaming/removing a public type
consumers use, changing a return type, moving a DTO between packages — is a **breaking package change**: it
can't land in one PR, because consumers won't see the new shape until the package republishes on merge to
main. Adding a *method* is safe (additive); changing *types consumers already use* is not.

When you hit one mid-feature: **don't force it into the current PR, and don't derail the feature to do it.**
Capture it in a dedicated plan (design + expand/contract steps + why it's multi-merge), record both
dependency graphs, and do every consumer preparation step supported by an exact local producer artifact.
Keep those consumers delivery-gated until the published-package revalidation.

(If the boundary friction itself is being questioned — "is the polyrepo sim worth it yet?" — that's an
architecture decision for the root, not something to resolve inside a feature PR.)

## End of a phase — leave it compaction/clear-safe

A phase isn't done when the code is green; it's done when **nothing important lives only in the chat
context**. After the phase's commit lands and its gate passes: the plan checked off or struck, the ledger
current through the last material event with one exact next action, and any decision or convention worth
carrying forward written into the doc that owns it. Anything you'd be annoyed to re-derive — a command that
worked, a gotcha hit — belongs in the commit message or that doc, not left implicit in the context. Treat
every phase boundary as the point where the conversation becomes disposable.

## Plan handoff

- Every plan `.md` carries a pointer near its top to its ledger(s) and holds no next-action prose of its
  own. One workstream: "**Next steps live in @plans/<STEM>_PROGRESS.md → `## Next Steps`**". Parallel
  workstreams list each ledger separately.
- Because the steps live in the ledger, a plan resume/handoff prompt is ONLY the pointer — an opener line
  then "Read @plans/<PLAN>_PLAN.md and @plans/<its-worktree-ledger>_PROGRESS.md and do what `## Next Steps`
  says", naming one workstream's ledger. The opener is `/worktree create <Type>/<epic>_<name>` when that
  worktree doesn't exist yet — including continuation after a prior PR merged — so implementation runs in an
  isolated worktree, never the main checkout; it's `cd <worktree>` once the worktree exists. No branch to
  verify, checkpoints, gates, commands, or steps in the prompt — every such specific lives in the ledger, so
  the prompt can't drift. See [`../../PROMPTS.md`](../../PROMPTS.md).
- `/resume-plan` takes a **ledger**, a **plan**, or a **worktree**. A ledger resolves to its live worktree
  when one exists; otherwise it creates a fresh worktree from current `origin/main`. A plan alone resolves by
  the ledgers whose `- Plan:` names it: one → resume it; several → list them and ask which. Always confirm
  the ledger still matches git/PR reality first.
- A ledger whose `## Next Steps` begins with the blocker fields or a `Paused:` line does not get its resume
  pointer. Report the blocker or the human action and its resume condition; dispatch the resolver when
  appropriate.
- A completed and verified phase ends the turn after its handoff. Start the next phase only when Tommy
  explicitly names it and says to do it now.
- When several ledgers have independently executable `## Next Steps`, surface one exact pointer per ledger. A
  delivery-gated ledger stays actionable until its local preparation reaches `delivery-ready`; only an
  implementation-blocked ledger suppresses its pointer.

## Verification gate per phase

Every phase, no exceptions:

- Run required generators, grep/invariant gates, and the smallest affected project or frontend build.
- Run focused unit tests for the changed behaviour.
- Commit and push the coherent checkpoint to its draft PR. Exact-head PR CI is authoritative for the full
  solution, standalone carves, and complete unit/integration matrices.
- Phases that change the model end with `./initial-migrations.ps1` from `api/` (re-scaffold, never additive
  migrations).
- **Final phase only:** select the merge-queue E2E tier under
  [Merge-queue E2E tier](../AGENTS.md#merge-queue-e2e-tier). A phase's own "verification gate" line naming
  E2E *selects the queue's tier*; it is not an instruction to duplicate the queue run locally.

A plan must not restate a full local build/integration gate; inherit
[`../../docs/REMOTE_VALIDATION.md`](../../docs/REMOTE_VALIDATION.md).
