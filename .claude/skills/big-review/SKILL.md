---
name: big-review
description: Review a very large branch diff in resumable area-stages, instead of one unreviewable pass. A staging wrapper around the `review` skill for branches too big to review at once (hundreds/thousands of changed files, e.g. `Refactor/Microservices`). Reviews the NET diff `merge-base..HEAD` (current state vs master — never walks intermediate commits, which waste time on superseded designs), sliced into area-stages. Each run reviews the next unreviewed area, appends findings, and ticks a coverage checklist in `reviews/BIG-<branch-slug>-Review.md`. Use when the user wants to "big review", "review this massive PR in stages", "stage the review", or resume a staged review ("continue the big review", "next stage"). For a normal-sized branch use `review`; for only-new-commits use `incremental-review`.
---

# big-review

`big-review` **is the `review` skill applied in resumable area-stages** to a branch too large to review in one sitting. Two things differ from `review`:

1. **Scope per run** — instead of reviewing the whole diff at once, each run reviews the NET diff `merge-base..HEAD` **scoped to one area's paths**. The net diff is what actually ships; intermediate commits are NOT walked (a long-lived refactor branch builds things then refactors them away — reviewing history wastes effort on code that no longer exists).
2. **Progress contract** — a **coverage checklist** of areas at the top of `reviews/BIG-<branch-slug>-Review.md` is the source of truth for what's done. Each run picks the next `[ ]` area, reviews it, appends its findings, and ticks it `[x]`. This is the resume mechanism (the analogue of `incremental-review`'s SHA marker).

Everything else — the rule docs, the five lenses, the ≥80-confidence filter — comes from `review` unchanged. **Read `.claude/skills/review/SKILL.md` and follow its Steps 2–4 verbatim for each area.** Keep this skill in sync with it.

## When to use

- "big review", "review this massive PR in stages", "stage the review"
- "continue the big review", "next stage", "resume the big review"
- Any branch where `review`'s single-pass diff would be too large to review with real recall (rule of thumb: >300 changed files or it spans multiple services).

## When NOT to use

- **A review whose Coverage checklist is already fully `[x]`. The staging pass is DONE.** "Continue the review", "address the comments/findings", or pointing me at a completed `reviews/BIG-*.md` means **work the findings in that doc** — fix the open ones, verify the rest — NOT re-invoke this skill. Re-running here only re-reports "complete" and does zero useful work. Read the doc, act on its findings; do not launch the skill.
- Normal-sized branch → `review`.
- Only re-review commits added since a prior review → `incremental-review`.
- Multi-agent cloud review → `/code-review ultra`.

## Resuming in a fresh context (the normal flow)

This skill is built to run one stage per context, then `/clear` and continue later. **To continue, just run `/big-review` again with no arguments** — nothing to tag by hand. On each run the skill:

1. derives the branch slug from the current git branch and opens `reviews/BIG-<branch-slug>-Review.md`;
2. reads the **Coverage** checklist — the `[x]`/`[ ]` marks are the bookmark;
3. reviews the **first `[ ]` stage**, appends its findings, and flips it to `[x]`.

So the loop is: `/big-review` → `/clear` → `/big-review` → `/clear` → … until every stage is `[x]`. Optional: pass a stage name (e.g. `/big-review B2B`) to review a specific stage out of order; pass nothing for the default next-unticked behaviour. You never edit the markdown manually — the skill owns the checklist.

## Step 0 — Find or create the tracking file

`reviews/BIG-<branch-slug>-Review.md` at repo root (branch `/` → `-`, e.g. `reviews/BIG-refactor-Microservices-Review.md`). Create the `reviews/` dir if missing.

- **File exists** → this is a resume. Read it, go to Step 2.
- **File missing** → first run. Go to Step 1.

## Step 1 — First run: compute the staging plan

1. Establish the range: `git merge-base master HEAD` (start) and `git rev-parse HEAD` (end). Show `git diff <start>..HEAD --stat | tail -1`.
2. **Derive the areas from the diff itself** — never from a preconceived map of the repo. Run `git diff <start>..HEAD --name-only`, cluster the files by component (top-level dirs, service/project roots, `app/` surfaces — whatever structure the changed files actually exhibit), and turn the clusters into stages:
   - **Only changed code gets a stage.** A component the branch didn't touch does not appear in the plan, no matter how important it is to the repo.
   - **Size stages for one sitting** — roughly 50–150 changed files or ~10k diff lines each. Split a huge component into sub-stages (by module/sub-tree); merge several small components into one stage.
   - **Order by dependency, foundation first.** Whatever shared code the rest of the diff builds on (contracts, kernel/shared libs, messaging, schema/migrations) is the first stage — its findings reframe everything downstream, and the cross-service/boundary sweep belongs there. Then consumers/services, then adapters/infra/CI, then tests + frontend last (reviewing tests after the code they test).
   - **Every changed file must belong to exactly one stage.** After bucketing, verify the union of stage paths covers the full `--name-only` list; sweep leftovers into a final **Everything else** stage rather than letting them silently escape review.
3. Write the tracking file (shape below) with the coverage checklist — **each checklist item carries its exact path globs**, which are the literal `git diff -- <paths>` arguments later runs will use — the plan-anchor SHA, an empty Cross-area notes section, and an empty Findings section. Then go to Step 3 to review the first area.

## Step 2 — Resume: pick the next area

Read the coverage checklist. If the plan-anchor SHA differs from current HEAD, note it in the report (areas already `[x]` stay done; new commits after the anchor are picked up by a later `incremental-review` if needed — do not silently re-plan).

- A `[~]` area means a previous run died mid-stage: its findings section (if any) is **incomplete, not trusted** — re-review that area from scratch and replace its partial section.
- Otherwise pick the **first `[ ]` area**.
- If all areas are `[x]`: write a short `## Summary` rollup at the top of the file (finding counts by severity/lens, the handful of must-fix items), confirm the Cross-area notes section has no unresolved entries (resolve any stragglers now), report "Big review complete — all N areas reviewed", and stop.

## Step 3 — Review the chosen area (the `review` procedure, path-scoped)

First, flip the area's checklist item to `[~]` and save — if this context dies mid-review, the next run knows the stage is incomplete rather than untouched. Then **read the Cross-area notes section**: any note targeting this area's paths is a mandatory check for this stage.

The review range is `<merge-base>..HEAD` **filtered to the area's paths** (the globs recorded on its checklist line). Get the area's real changed surface and **skip move-only files** (a pure rename/move with no content change is not worth line review):

```powershell
git diff <merge-base>..HEAD --stat -- <area-path-1> <area-path-2> ...
git diff <merge-base>..HEAD --diff-filter=d --find-renames -- <area-paths>   # excludes pure deletes
```

Then follow **`review` Steps 2–4 verbatim** on that scoped diff:

- Load the rule docs relevant to the area (`review` Step 2).
- Review through all five lenses — correctness, microservice isolation, module boundaries, seeding, C# conventions (`review` Step 3). For whichever stage holds the shared/contract code the rest of the diff depends on, the isolation/boundary lens is the headline check.
- Apply the ≥80-confidence filter (`review` Step 4).

While reviewing, when you spot something whose other half lives in a **different** area (a changed contract whose consumers are in a later stage, a renamed config key, a behaviour change that some other component must absorb), don't drop it and don't review outside your paths — **add a one-line entry to Cross-area notes** naming the target area and what to verify there.

For a very large area, fan out reading with parallel sub-agents (one per sub-tree), but **you** apply the confidence filter and write the findings.

## Step 4 — Append findings and tick the area

In `reviews/BIG-<branch-slug>-Review.md`:

- **Append** a `## <Area> — reviewed <date>` section with the findings (use `review`'s finding shape and stable IDs; continue the ID scheme across areas, no renumbering). No findings → write the "No issues found in this area" line.
- Mark any Cross-area note you checked during this stage as resolved (strike it through with the outcome); leave notes you added for later stages open.
- Flip that area's checklist item from `[~]` to `[x]` with the date.
- Preserve every prior area's section and status marks. Never overwrite (the one exception: replacing a partial section left by a dead `[~]` run, per Step 2).

## Step 5 — Report

Concise: area just reviewed, its finding counts by lens/severity (or none), remaining `[ ]` areas, and the file. Tell the user to run `/big-review` again for the next stage. No "Generated with Claude" trailers.

## Tracking file shape

```markdown
# Big review — <branch>

**Plan anchored to commit:** `<full-HEAD-sha>`  _(<ISO date>)_
Net diff reviewed: `<short-merge-base>..<short-head>`. Move-only files skipped.
Status legend: `[ ]` not yet reviewed · `[x]` reviewed (date) · `[~]` in progress (incomplete — re-review).

## Coverage
<!-- one item per stage, derived from THIS branch's diff in Step 1 — dependency order,
     foundation first; each item lists its exact path globs (the `git diff -- <paths>` args).
     Example shape: -->
- [ ] <Stage name> — <N files> — `<path/glob/one>` `<path/glob/two>`
- [ ] <Stage name> — <N files> — `<path/glob>`
- [ ] Everything else — <N files> — <leftover paths not matched above, if any>

## Cross-area notes
<!-- one-liners added during a stage for things a LATER stage must verify
     ("Contract X gained field Y — check consumers in <area>"); struck through with the
     outcome when the owning stage checks them. Empty when the review completes. -->

## Findings
<!-- appended per area; finding IDs continue across areas: MS#, MB#, BUG#, SEED#, CV# -->
```
