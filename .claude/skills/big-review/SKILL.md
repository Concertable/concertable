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
2. Bucket the **net-diff** changed files into areas. Default areas (merge/split to fit the actual tree; aim for stages of roughly comparable, digestible size):
   - **Shared foundation** — `api/Kernel`, `api/Contracts`, anything `Messaging`/`DataAccess`, `api/Shared/*` libraries. (Review first — every service depends on it; do the microservice-isolation boundary sweep here.)
   - **One stage per data service** — `api/Concertable.B2B/`, `api/Concertable.Customer/`, `api/Concertable.Search/`. Split a service across stages if it alone is huge.
   - **Adapters** — `api/Concertable.Payment/`, `api/Concertable.Auth/`, Notification.
   - **Seed + infra** — Seed projects, AppHosts, CI workflows, `.slnx`/`.sln`, `Directory.Build.props`, root docs.
   - **Tests + frontend** — integration/E2E test projects, `app/`.
3. Write the tracking file (shape below) with the coverage checklist, the plan-anchor SHA, and an empty Findings section. Then go to Step 3 to review the first area.

## Step 2 — Resume: pick the next area

Read the coverage checklist. If the plan-anchor SHA differs from current HEAD, note it in the report (areas already `[x]` stay done; new commits after the anchor are picked up by a later `incremental-review` if needed — do not silently re-plan). Pick the **first `[ ]` area**. If all areas are `[x]`, report "Big review complete — all N areas reviewed" and stop.

## Step 3 — Review the chosen area (the `review` procedure, path-scoped)

The review range is `<merge-base>..HEAD` **filtered to the area's paths**. Get the area's real changed surface and **skip move-only files** (a pure rename/move with no content change is not worth line review):

```powershell
git diff <merge-base>..HEAD --stat -- <area-path-1> <area-path-2> ...
git diff <merge-base>..HEAD --diff-filter=d --find-renames -- <area-paths>   # excludes pure deletes
```

Then follow **`review` Steps 2–4 verbatim** on that scoped diff:

- Load the rule docs relevant to the area (`review` Step 2).
- Review through all five lenses — correctness, microservice isolation, module boundaries, seeding, C# conventions (`review` Step 3). For the shared-foundation stage, lens B (isolation) is the headline check.
- Apply the ≥80-confidence filter (`review` Step 4).

For a very large area, fan out reading with parallel sub-agents (one per sub-tree), but **you** apply the confidence filter and write the findings.

## Step 4 — Append findings and tick the area

In `reviews/BIG-<branch-slug>-Review.md`:

- **Append** a `## <Area> — reviewed <date>` section with the findings (use `review`'s finding shape and stable IDs; continue the ID scheme across areas, no renumbering). No findings → write the "No issues found in this area" line.
- Flip that area's checklist item from `[ ]` to `[x]` with the date.
- Preserve every prior area's section and status marks. Never overwrite.

## Step 5 — Report

Concise: area just reviewed, its finding counts by lens/severity (or none), remaining `[ ]` areas, and the file. Tell the user to run `/big-review` again for the next stage. No "Generated with Claude" trailers.

## Tracking file shape

```markdown
# Big review — <branch>

**Plan anchored to commit:** `<full-HEAD-sha>`  _(<ISO date>)_
Net diff reviewed: `<short-merge-base>..<short-head>`. Move-only files skipped.
Status legend: `[ ]` not yet reviewed · `[x]` reviewed (date) · `[~]` in progress.

## Coverage
- [ ] Shared foundation — Kernel, Contracts, Messaging, DataAccess, Shared/* libs
- [ ] B2B service — `api/Concertable.B2B/`
- [ ] Customer service — `api/Concertable.Customer/`
- [ ] Search + adapters — Search, Payment, Auth, Notification
- [ ] Seed + infra — Seed, AppHosts, CI, .slnx, root docs
- [ ] Tests + frontend — integration/E2E, `app/`

## Findings
<!-- appended per area; finding IDs continue across areas: MS#, MB#, BUG#, SEED#, CV# -->
```
