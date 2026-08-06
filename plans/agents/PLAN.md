# How plans work (`plans/*.md`)

The topic playbook for **plans** — working docs for unfinished, multi-step work. The hub
[`../AGENTS.md`](../AGENTS.md) carries the cross-cutting rules (commit-when-green, branch-first,
merges, tests); this file is what a plan *is* and its lifecycle. Roadmaps are a different artifact —
see [`ROADMAP.md`](ROADMAP.md).

A finished plan kept "for reference" is rot: git history is the archive, and a stale plan misleads the
next reader into thinking the work is still pending.

## Shape of a plan

A plan describes a chunk of work too big for one commit, broken into **phases that are each
independently shippable and each end green**. A phase states what it changes, why, and its
verification gate. Phases sequence so that every intermediate state builds and passes.

## Companion progress ledger — record the whole operational history

Every `plans/<epic>/<NAME>_PLAN.md` has at least one same-directory `<NAME>_PROGRESS.md` companion,
created with the plan. Plan and ledger share the `<NAME>` stem; the plan's worktree/branch is
`<Type>/<epic>_<name>` so branch, plan, and ledger carry one identity. The **plan** is the shared design — phases, dependencies, definition-of-done; each **ledger** is
the operational truth of **one worktree** working it, so the ledger (not the plan) is 1:1 with a
worktree and owns the `Worktree`/`Branch`/`PR` identity. Keep the plan readable by putting the detailed
history and current truth in the ledger. Start each from
[`resume-plan/assets/progress-template.md`](../../.agents/skills/resume-plan/assets/progress-template.md).

**A plan may have several ledgers.** Worked in one worktree it has one `plans/<NAME>_PROGRESS.md`. When
its phases run in **parallel worktrees** — e.g. a publish-gated prerequisite phase built on its own
branch while a later phase waits — it has **one ledger per worktree**. Name each so it identifies its
worktree, and set its `- Plan:` header to this plan: that header is the authoritative plan↔ledger
grouping (`/resume-plan` greps it), not the filename.

The ledger must make the chat disposable at any point. Record **every project action and state
transition as it happens**, not just phase summaries: user direction and scope changes, partial
implementation, commits, verification commands and results, reviews and every
finding's disposition, fixes after review, PR creation and checks, merges, publications, platform
syncs, decisions, discoveries, deviations, blockers, failed approaches worth avoiding, and external
gates — all scoped to the plan's substance (code, design, delivery, gate state). Not tooling,
environment or git mishaps, or incident narration; record the durable fact ("committed as `<sha>`"),
never the drama. "A review happened" is insufficient: identify the review type and range, its artifact,
whether findings remain open, and the commit or deferral that resolved each one. Never leave a project
fact only in chat because it happened between phase boundaries.

Every workflow that advances or evaluates plan-managed work owns this update before it ends. That
includes implementation, `/code-review`, `/big-review`, `/incremental-review`, addressing findings,
verification, committing, opening or updating a PR, merging, publishing, and platform sync. The fact
that another artifact or skill records part of the event does not make the progress update someone
else's later close-out.

The mandatory procedure is
[`resume-plan/references/plan-progress-checkpoint.md`](../../.agents/skills/resume-plan/references/plan-progress-checkpoint.md).
Every repository workflow skill named above must invoke it before any report or stop. Apply it directly
for plan-aware implementation or plain `gh pr create` work that has no repository skill wrapper.

Each ledger keeps these current sections above its chronological event log:

- plan, absolute worktree, branch, PR, and relevant dependency or package gates;
- current state, including partial/uncommitted work that the next agent must preserve;
- completed work with commit or PR evidence;
- verification and review state;
- decisions, discoveries, blockers, and deviations;
- **`## Next Steps`** — the paste-ready prompt for the next agent: the concrete step(s) to take now,
  self-contained, with any prerequisite or blocking gate. This is the **single source of truth** for
  what to do next; resume/handoff prompts point here instead of restating it, so a prompt can never
  drift from reality. Keep it current at every checkpoint.

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

## Never leave the codebase out of sync — the plan isn't done until the whole thing is

A refactor isn't finished when the convenient half lands. If a repo/package boundary forces it into
multiple PRs (e.g. a Kernel change and the B2B change that depends on it), do them back-to-back —
but the plan stays open until **all** of them land and the codebase is in sync again. Merging the
B2B PR and calling the plan done while Kernel still speaks the old shape is the thing to never do.
Don't `git rm` the plan (Lifecycle 5) until that final synced state is in.

## Cross-plan blockers — establish the return path before stopping

When a phase can't proceed because it depends on work owned by a **different** plan in the same epic
(e.g. B2B's migration waiting on Payment's), don't guess the dependency's state from memory. Read the
epic roadmap as the cross-plan dependency map, find which sibling plan owns the blocker, and open that
plan's `_PROGRESS.md` for its live state (merged? published? platform-sync green?). Only then proceed or
record the exact unlanded gate.

Blocking is a two-ledger state transition:

1. In the waiting ledger, make `## Next Steps` name the owner ledger and the exact terminal gate. The
   waiting worktree does not poll after that checkpoint.
2. In the owner ledger, add a `## Downstream handoffs` entry with the waiting ledger, its worktree, and
   the same gate. This is the durable return path.
3. When the owner crosses the gate, update the waiting ledger's current state, `## Next Steps`, and
   event log in that same delivery session, then surface its exact resume prompt to Tommy.
4. Do not close or delete the owner plan/ledger while a downstream handoff remains undispatched.

Reporting "waiting for X" without registering the dependent in X's ledger is incomplete: it loses the
only reliable signal for returning to the work. The roadmap is used at runtime for navigation, never
cited inside a plan (see [`ROADMAP.md`](ROADMAP.md)).

## Lifecycle

1. **Write it** when the work spans multiple commits/PRs or needs a design decided up front, and create its `_PROGRESS.md` companion at the same time.
2. **Branch, then work a phase** — on the plan's `Feature/<Name>` branch (see the hub's "Branch first"), land the phase's commit(s).
3. **Check off / strike the shipped phase in the plan and update the progress ledger, in the same commit as the work.** A
   partially-done plan stays; only the outstanding work should remain un-ticked, so the next reader
   sees exactly what's left.
4. **Keep both artifacts after the last local phase while delivery is live.** Check off the phase and
   make the ledger's exact next action the review, fix, PR, merge, publication, dependency, or
   platform-sync gate that now owns progress. Continue recording every transition; local completion
   is not lifecycle completion. Once the source PR merges, transfer the recovery commits to a clean
   `Docs/<epic>_<name>_closeout` worktree and remove the feature worktree immediately.
5. **Close out only after the entire lifecycle is terminal.** Record the final gate's outcome and
   evidence, make that ledger checkpoint durable, then delete the plan and ledger together (`git rm`)
   in the following close-out commit. Land that commit through `/merge-docs`, then remove the close-out
   worktree. When the final phase has no later delivery or package gates, that phase's completing
   commit may perform the close-out.
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

### Post-merge close-out — move state, delete the feature worktree, use `/merge-docs`

If no later delivery or package gate exists, the plan deletion + roadmap tick can land inside the
feature's final commit. Otherwise the source PR merges while the plan remains live. Immediately after
recording that merge, create `Docs/<epic>_<name>_closeout` from current `origin/main`, transfer every
ledger-only observation commit after the verified source PR head, update the ledger's worktree/branch
identity, and verify the transferred plan and ledger match the source worktree. Then delete the merged
feature worktree and branch before watching publication or platform sync.

The close-out worktree is the recovery anchor for the remaining remote gates. Once they are terminal,
commit the final ledger checkpoint, delete the plan and ledger together in the following commit, and
tick the owning roadmap item. Run `/docs-review`, land the net meta-only change through `/merge-docs`,
and remove the close-out worktree. Never leave close-out edits in a merged feature worktree or an
unrelated checkout; the fast docs path exists so no merged worktree needs to linger.

### Boundary-blocked refactors — capture in a plan, don't force into this PR

Cross-service deps go through **published packages**, not project references (the carve — see
[`api/ARCHITECTURE.md`](../../api/ARCHITECTURE.md)). B2B and Customer compile against the *published*
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

## Plan handoff

- Every plan `.md` carries a pointer near its top to its ledger(s) and holds no next-action prose of
  its own. One worktree: "**Next steps live in @plans/<STEM>_PROGRESS.md → `## Next Steps`**" (the `@`
  pulls the ledger when you tag the plan). Parallel worktrees: it lists each ledger with its worktree.
- Because the steps live in the ledger, a plan resume/handoff prompt is ONLY the pointer — an opener line
  then "Read @plans/<PLAN>_PLAN.md and @plans/<its-worktree-ledger>_PROGRESS.md and do what `## Next Steps`
  says", naming one worktree's ledger. The opener is `/worktree create <Type>/<epic>_<name>` when that
  worktree doesn't exist yet — a freshly-written plan, or a clear with no live worktree — so implementation
  runs in an isolated worktree, never the main checkout; it's `cd <worktree>` once the worktree exists. No
  branch to verify, checkpoints, gates, commands, or steps in the prompt — every such specific lives in the
  ledger, never restated, so the prompt can't drift. See [`../../PROMPTS.md`](../../PROMPTS.md).
- `/resume-plan` takes a **ledger**, a **plan**, or a **worktree**. A ledger — or a plan plus a named
  worktree — resolves straight to that worktree: `cd` there and do its `## Next Steps`. A plan alone
  resolves by the ledgers whose `- Plan:` names it: one → resume it; several → list them and ask which.
  Always confirm the ledger still matches git/PR reality first.
- A completed and verified phase ends the turn after its handoff. Start the next phase only when Tommy
  explicitly names it and says to do it now.

## Verification gate per phase

Every phase, no exceptions:

- `dotnet build api/Concertable.slnx` green (0 errors).
- The **affected** module's unit + integration tests — run them via the `integration-debug` skill.
- Phases that change the model end with `./initial-migrations.ps1` from `api/` (re-scaffold, never
  additive migrations).
- **Final phase only:** select the merge-queue E2E tier under the hub's
  [Merge-queue E2E tier](../AGENTS.md) criteria. Do not duplicate the queue's E2E run locally; use the
  matching E2E debug skill only after a queue failure.

A phase's own "verification gate" line may name E2E; treat that as selecting full merge-queue E2E when
the hub's criteria require it, not as an instruction to duplicate the queue run locally.
