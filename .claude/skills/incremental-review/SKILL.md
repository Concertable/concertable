---
name: incremental-review
description: Re-run a code review covering only the commits added since the last review, never re-reviewing code already covered. A thin wrapper around the `review` skill — does everything `review` does (correctness bugs, microservice-isolation + module-boundary + seeding + C# convention checks, high-confidence filter), but starts the diff at the `Reviewed up to commit:` SHA marker recorded in the review markdown instead of the branch merge-base. Then appends findings and re-stamps the marker to HEAD. Use whenever the user wants to "review the new commits", "review what changed since the last review", "incremental review", "continue the review", or re-review a long-lived branch without re-covering old code.
---

# incremental-review

`incremental-review` **is the `review` skill with one input changed**: the review range starts at the SHA recorded in the review markdown's marker, not the branch's merge-base. Every later run sees only the delta, so a massive long-lived branch (e.g. `Refactor/Microservices`, 1500+ files) gets fully reviewed once and never re-reviewed. The marker is the contract between runs; each run reads it, reviews `SHA..HEAD`, then rewrites it to the new HEAD.

## When to use

- "review the new commits", "review what's changed since the last review", "incremental review"
- "continue the review", "pick up where the review left off"
- Re-running a review on a branch already reviewed once that has moved on

## When NOT to use

- First-ever review of a branch with no review markdown yet → use the **`review`** skill (it reviews the whole branch and creates the file + marker). Then use this one going forward.
- Multi-agent cloud review → `/code-review ultra`.

## The marker — the whole mechanism

One greppable line near the top of the review markdown:

```
**Reviewed up to commit:** `<full-40-char-sha>`  _(<ISO-date>)_
```

It is the **only** source of truth for where the last review stopped. Don't infer the watermark from git history, timestamps, or dates in prose — if the marker is absent, ask the user (Step 2). Heuristic recovery is a one-time problem this marker exists to eliminate.

## Step 0 — Find the review markdown

Default: `reviews/<branch-slug>.md` (branch `/` → `-`). If the user named a file, or a legacy review file exists for this branch (e.g. `plans/PR_FEEDBACK.md`), use that. The file must already exist — this skill is incremental. If none exists, tell the user to run `/review` first.

## Step 1 — Read the watermark

```powershell
git rev-parse HEAD
```

Grep the markdown for `Reviewed up to commit:` and extract the backtick-wrapped SHA (use the Grep tool).

- **Marker found** → that SHA is the start commit. Go to Step 3.
- **Marker absent** → first incremental run on this file. Go to Step 2.

## Step 2 — No marker: seed the start once (first run only)

Show the recent timeline and ask which commit the prior review covered up to — do not guess:

```powershell
git log -20 --format='%h  %ci  %s'
```

Ask: **"This review has no recorded watermark. Which commit did the existing review cover up to? I'll review everything after it."** Use their answer as the start commit.

## Step 3 — Run the `review` procedure over `start..HEAD`

Read `.claude/skills/review/SKILL.md` and **follow it from Step 3 onward verbatim**, with exactly one substitution: the review range is `<start>..HEAD` (the watermark SHA from Step 1/2), not the merge-base.

That means, on the scoped diff:

- Load the rules (review Step 2 doc list).
- Review through all five lenses — correctness, microservice isolation, module boundaries, seeding, C# conventions (review Step 3).
- Apply the ≥80-confidence filter (review Step 4).
- **Append** a new `## Incremental review — <date>` section to the existing markdown, preserving prior findings and their status marks; continue the file's finding-ID scheme without renumbering (review Step 5).

Validate the start SHA resolves (`git rev-parse "<start>"`) and abort with a clear message if not. If `<start>` already equals HEAD (empty range), report "Nothing new to review — already reviewed up to `<short-sha>`" and stop without appending or re-stamping.

## Step 4 — Re-stamp the marker (mandatory when the range was non-empty)

Replace the existing marker line in place (exactly one in the file) with current HEAD:

```
**Reviewed up to commit:** `<full-HEAD-sha>`  _(<today's ISO date>)_
```

## Step 5 — Report

Concise: range reviewed (`<short-start>..<short-head>`, N commits), new findings by lens/severity (or "none"), file appended to, new watermark stamped. Point at the file. No "Generated with Claude" trailers.
