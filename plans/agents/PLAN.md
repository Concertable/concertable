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

## Companion progress ledger — keep a compact operational snapshot

Every `plans/<epic>/<NAME>_PLAN.md` has at least one same-directory `<NAME>_PROGRESS.md` companion,
created with the plan. Plan and ledger share the `<NAME>` stem; the plan's worktree/branch is
`<Type>/<epic>_<name>` so branch, plan, and ledger carry one identity. The **plan** is the shared design — phases, dependencies, definition-of-done; each **ledger** is
the operational truth of **one worktree** working it, so the ledger (not the plan) is 1:1 with a
worktree and owns the `Worktree`/`Branch`/`PR` identity. Keep the plan readable by putting the current
operational truth in the ledger. Start each from
[`resume-plan/assets/progress-template.md`](../../.agents/skills/resume-plan/assets/progress-template.md).

Each ledger records the owning `Roadmap:` and stable `<epic>/<slug>` `Roadmap item:` key. The roadmap
checklist line carries that key in backticks. Together with `Plan:`, these required headers form the
explicit roadmap→plan→ledger graph. Reconstruct them from the plan and roadmap when adopting a legacy
ledger.

**A plan may have several ledgers.** Worked in one worktree it has one `plans/<NAME>_PROGRESS.md`. When
its phases run in **parallel worktrees** — e.g. a publish-gated prerequisite phase built on its own
branch while a later phase waits — it has **one ledger per worktree**. Name each so it identifies its
worktree, and set its `- Plan:` header to this plan: that header is the authoritative plan↔ledger
grouping (`/resume-plan` greps it), not the filename.

The ledger must make the chat disposable at any point, but it is a **rolling recovery snapshot, not an
append-only history**. Preserve only facts a fresh agent needs to continue safely: current partial or
uncommitted work, active gates and blockers, the latest verification valid for the current candidate,
open review findings, compact completed-milestone evidence, and decisions or failed approaches that
still constrain future work. Git history, PRs, check runs, review artifacts, and package feeds are the
archive for superseded detail.

Record each material state transition immediately, then reconcile the snapshot: replace stale facts,
remove resolved blockers and obsolete next steps, drop verification superseded by a newer candidate,
and collapse finished phases or delivery sequences to their terminal commit/PR evidence. Do not retain
routine chronology once its durable outcome is represented elsewhere in the ledger. The retention test
is: **could removing this fact cause a fresh agent to take the wrong action or repeat a costly failed
approach?** If not, remove it.

Every workflow that advances or evaluates plan-managed work owns this update before it ends. That
includes implementation, `/code-review`, `/big-review`, `/incremental-review`, addressing findings,
verification, committing, opening or updating a PR, merging, publishing, and platform sync. The fact
that another artifact or skill records part of the event does not make the progress update someone
else's later close-out.

The mandatory procedure is
[`resume-plan/references/plan-progress-checkpoint.md`](../../.agents/skills/resume-plan/references/plan-progress-checkpoint.md).
Every repository workflow skill named above must invoke it before any report or stop. Apply it directly
for plan-aware implementation or plain `gh pr create` work that has no repository skill wrapper.

Each ledger keeps these current sections, plus an optional short `## Recent transitions` section:

- plan, absolute worktree, branch, PR, and relevant dependency or package gates;
- current state, including partial/uncommitted work that the next agent must preserve;
- completed milestones, normally one concise item per phase or delivery gate with commit/PR evidence;
- the latest verification and review state still valid for the current candidate, including every open
  finding and its disposition;
- decisions, discoveries, blockers, and deviations that still affect execution and cannot be safely
  reconstructed from code or durable artifacts;
- **`## Next Steps`** — the single resolved action for the next agent, expressed as concrete,
  self-contained steps. If no action can proceed, start it with the exact `Blocked:`, `Blocked by:`,
  `Unblock action:`, and `Resume when:` fields defined below. Apply the repository's standing instructions and current
  evidence before writing it, so it directs execution instead of presenting alternatives. This is the
  **single source of truth** for what to do next; resume/handoff prompts point here instead of restating
  it only when the action is executable. Keep it current at every checkpoint.

`## Recent transitions` is temporary working memory for a material change not yet fully represented by
the stable sections. Omit it when empty. Delete or collapse each entry as soon as its outcome is folded
into current state, a milestone, verification, review, or a durable decision; it must never become a
permanent event log. Include enough commands, paths, identifiers, results, and reasoning to continue
without the prior conversation, but do not duplicate evidence available in a named durable artifact.
Commit ledger updates with the work they describe whenever possible. Remote-only transitions that
happen after a commit, such as a PR entering the queue or a package publishing, still require the next
immediate ledger checkpoint; that checkpoint replaces prior state instead of accumulating narration.

**Backward compatibility:** the absence of a progress ledger means the plan predates this rule, not
that no work has happened. Read the plan, git history and working tree, review artifacts, test evidence,
PR/check state, and package gates. Create `<NAME>_PROGRESS.md` with an explicitly labelled
"reconstructed baseline" containing only verifiable facts; do not invent missing history. All future
progress goes into that ledger. When a workflow next touches a legacy append-only ledger, compact its
superseded history under this rule as part of that checkpoint.

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

## Model implementation and delivery separately

Any plan spanning PR, package, publication, deployment, or platform-sync boundaries maintains two
dependency graphs:

- the **implementation DAG** records the source, API, design, and exact producer artifact needed to
  implement, test, review, and commit each independently owned branch;
- the **delivery DAG** records the merge, publication, generated sync, deployment, and final published-
  baseline revalidation order.

Use these states consistently in roadmaps, plans, ledgers, reports, and handoffs:

- **implementation-blocked** — a required source/API/design or trustworthy exact artifact is unavailable;
- **implementable, delivery-gated** — local implementation can proceed, but the branch cannot merge yet;
- **delivery-ready** — implementation, tests, and review are green against the recorded exact producer
  artifact; published-baseline revalidation remains;
- **merge-ready** — temporary inputs are gone and the branch is green against the real published baseline;
- **terminal** — all required merge, publication, sync, and closeout gates are complete.

An unlanded PR, unpublished package, or pending platform sync belongs in the delivery DAG unless evidence
shows it prevents safe local work. When an exact local package is sufficient, record its producer commit,
package version, hashes, and reproducible location; never commit a machine-specific feed path, temporary
version pin, or local-only configuration. Revalidate against the published package before calling the
consumer merge-ready.

## Cross-plan blockers — establish the return path before stopping

When a phase depends on work owned by a **different** plan in the same epic, don't guess the dependency's
state from memory. Read the epic roadmap as the cross-plan dependency map, find which sibling plan owns
the dependency, and open that plan's `_PROGRESS.md` for its live state. First classify the edge in both
DAGs: dispatch safe local preparation immediately and reserve the blocker protocol below for an
implementation-blocked edge.

An implementation blocker is a two-ledger state transition:

1. In the waiting ledger, record the exact terminal gate, owner-ledger action, and objective green
   evidence with the blocked-state fields below. The waiting worktree does not poll after that
   checkpoint or emit its own resume pointer.
2. In every plan-owner ledger named by `Blocked by:`, add a `## Downstream handoffs` entry with the
   waiting ledger, its worktree, and the same gate. This is the durable return path.
3. When the owner crosses the gate, update and compact the waiting ledger's current state and
   `## Next Steps` in that same delivery session, then surface its exact resume prompt to Tommy.
4. Do not close or delete the owner plan/ledger while a downstream handoff remains undispatched.

Reporting "waiting for X" without first proving that X blocks implementation is incomplete. Reporting
a genuine implementation blocker without registering the dependent in X's ledger also loses the
only reliable signal for returning to the work. The roadmap is used at runtime for navigation, never
cited inside a plan (see [`ROADMAP.md`](ROADMAP.md)).

## Hard blockers — hand off the resolver, never the blocked plan

If safe, authorized work in the current session can remove the obstacle—or if local implementation can
proceed while delivery waits—do that work; it is not a hard blocker. Otherwise `## Next Steps` must
begin with four single-line fields:

```text
Blocked: <the exact unmet gate>
Blocked by: <the owning plans/..._PROGRESS.md ledger, or the external owner>
Unblock action: <what must be done, by whom or where>
Resume when: <the objective evidence that proves the gate opened>
```

The final response reports all four lines verbatim and never emits this plan's continuation pointer while
they remain true. Route the resolving work according to ownership:

- Existing PR, plan, or session: register the downstream handoff, name the owner, and stop without a
  prompt. The owner updates this ledger and surfaces its pointer when the gate opens, then removes the
  dispatched entry from `## Downstream handoffs` before becoming terminal.
- No owner and a separate context is appropriate: emit a paste-ready dispatch prompt for the resolver,
  including the blocked ledger and the condition it unlocks. This is not the blocked plan's pointer.
- User or external action: give the exact action and verification condition directly, with no prompt.

Do not create a new checkpoint merely to prove an unchanged blocker is still blocked. Reconcile and
checkpoint only when evidence or routing changed.

## Lifecycle

1. **Write it** when the work spans multiple commits/PRs or needs a design decided up front. Before creating its ledger/worktree, check existing branches, worktrees, PRs, and ledgers for the same work, then assign each phase exactly one canonical ledger/worktree/branch; never create a second implementation owner.
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
multi-merge), record both DAGs, and do every consumer preparation step supported by an exact local
producer artifact. Keep those consumers delivery-gated until the published-package revalidation.

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
- A ledger whose `## Next Steps` begins with the hard-blocker fields does not get its resume pointer.
  Report the blocker, unblock action, and resume condition; dispatch the resolver when appropriate.
  The pointer becomes valid only after evidence opens the gate and the ledger is reconciled to an
  actionable next step.
- A completed and verified phase ends the turn after its handoff. Start the next phase only when Tommy
  explicitly names it and says to do it now.
- When several ledgers have independently executable `## Next Steps`, surface one exact pointer per
  ledger. A delivery-gated ledger remains actionable until its local preparation reaches
  `delivery-ready`; only an implementation-blocked ledger suppresses its pointer.

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
