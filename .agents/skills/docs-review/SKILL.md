---
name: docs-review
description: Full review of a branch's documentation/meta diff (**/*.md, .agents/.claude/.codex agent metadata, plans/**, docs/**, AGENTS.md, CLAUDE.md, PROMPTS.md, README*) against the repo's own doc conventions — accuracy vs the real code/commands/paths it cites, contradiction with sibling docs, the topic-playbook convention, concision of harness-reloaded docs, and dangling references to transient artifacts. The docs counterpart to `review`: same review-file work-order format and confidence bar, different lenses. Use when the user wants to "docs-review", "review these docs", or before merging a docs/meta-only PR (the `/merge` docs branch and `/merge-docs` gate on it). For runtime/source changes use `review`.
---

# docs-review

Full review of the current branch's **documentation/meta** diff, judged against the repo's own doc
conventions — not generic style. A docs change has no build or test gate, so this review *is* its gate.
Output is the same per-branch review markdown `review` produces (a `reviews/<branch>.md` work
order with a `Reviewed up to commit:` SHA marker), so `address-review` and `incremental-review` consume
it unchanged.

This is the docs sibling of [`review`](../review/SKILL.md): identical file, marker, confidence
filter and no-hedge rule; the lenses (Step 3) and the rules it loads (Step 2) are what differ.

## When to use

- "docs-review", "review these docs/plans/skills", "review this docs PR"
- Gating a docs/meta-only PR before it merges — the `/merge` Step 0 docs branch and `/merge-docs` both
  require a clean docs-review.

## When NOT to use

- The diff touches runtime/product/package/CI-test-selection code → `review` (docs-review does not
  judge code). A mixed PR is a code PR: run `review`, which covers its docs too.
- Re-reviewing only commits added since a prior review → `incremental-review` (reads the SHA marker).

## Step 1 — Confirm the checkout, then determine the review range

Reviews the git repo **in the current working directory**; no path argument. Identify the checkout
first so a wrong-checkout run is caught, not silently reviewed:

- **Checkout** = `git rev-parse --show-toplevel` + `git branch --show-current`. Echo both.
- **Start** = `git merge-base main HEAD` (whole branch). **End** = `git rev-parse HEAD`.

```powershell
git rev-parse --show-toplevel
git branch --show-current
git rev-parse HEAD
git merge-base main HEAD
git diff "<start>..HEAD" --stat
```

If the range is empty or the branch is `main`, that's the wrong-checkout symptom — say so and stop.

**Scope guard:** list changed paths (`git diff --name-only "<start>..HEAD"`). If any is a
runtime/product/package/CI-test-selection path (`api/**`, `app/**` source, `*.csproj`/CPM,
`package.json`/lockfiles, `.github/workflows/**`, migrations), this is **not** a docs-only diff — stop
and route to `review`. In-scope meta paths: `**/*.md`, `.agents/**`, `.claude/**`, `.codex/**`,
`plans/**`, `docs/**`, `AGENTS.md`, `CLAUDE.md`, `README*`, `PROMPTS.md`.

## Step 1b — Create the review file NOW, before reviewing (mandatory)

Same as `review` Step 1b: the moment the range is known, resolve the target path
(`reviews/<branch-slug>.md`, `/` → `-`; or an existing/named file) and, if it doesn't exist, write the
Step-5 skeleton — the `# Docs review — <branch>` header, the work-order blurb, the
`**Reviewed up to commit:**` marker at current HEAD, the range line, and a `## Findings` section with a
single `- _(review in progress — findings appended as they're confirmed)_` placeholder. If the file
already exists, leave it intact and append. Then review and append each confirmed finding as you go.

## Step 2 — Load the rules (read before flagging anything)

The doc conventions are the source of truth. Read the ones the diff touches; flag a **convention**
issue (Lens C) only when a doc actually states it, but Lens A/B/D/E/F stand on accuracy, internal
consistency, and the observable fact of which files the harness reloads — not on a written convention:

- Root `AGENTS.md` / `CLAUDE.md` — top-of-context conventions, incl. the comment/doc philosophy.
- `plans/AGENTS.md`, `plans/agents/ROADMAP.md`, `plans/agents/PLAN.md` — the ROADMAP→PLAN→PROGRESS
  convention and its rules (when a doc referencing the roadmap is legitimate vs a coupling).
- `reviews/AGENTS.md` — review-file lifecycle (work-order, delete-when-spent).
- `PROMPTS.md` — handoff/resume/review prompt shape.
- Any `AGENTS.md` in a directory the diff touches, and for a skill diff the sibling skills it names.

## Step 3 — Review the diff through these lenses

Review **only** the changes in `<start>..HEAD`. Read beyond them only to confirm a finding.

### Lens A — Accuracy vs reality (the high-value lens)

Every concrete claim the doc makes must match the actual repo. Flag, with the mismatch:

- A **dead or wrong reference** — a link, relative path, filename, or `@`-include pointing at something
  that doesn't exist or has moved; a heading anchor that no longer resolves.
- A **stale fact** — a named skill/label/route/command/env var/file the doc cites that has been
  renamed or removed, or a described behaviour that contradicts the code or config it documents.
- A **command that won't run** as written (wrong flag, wrong path, wrong shell for the repo).

If the diff touches any `AGENTS.md`, `CLAUDE.md`, or `*/agents/*.md`, run
`python .agents/hooks/docs_reachability.py --root <absolute-checkout>` and fold each reported error in
as a Lens A finding (a dead/orphaned reference).

### Lens B — Contradiction with sibling docs (the other high-value lens)

A doc change must not make two docs disagree. When the diff adds or changes a rule, check the docs it
interacts with don't now state the opposite, and that the change didn't leave a now-false statement
elsewhere. Cross-doc contradiction (and self-contradiction within the file) is the defect docs-review
exists to catch — a new "you may read X" against an old "never touch X" must be reconciled in *both*
places, not just one.

### Lens C — Right home & the topic-playbook convention

The repo keeps each topic's rules in its **own playbook** (`plans/agents/PLAN.md` / `ROADMAP.md` are
literally "the topic playbook for …") and hubs (`AGENTS.md`) as pointers to them. Flag:

- New guidance **bolted onto a hub** that belongs in the topic's own doc — grow the playbook, not the hub.
- The **same rule stated in two places** so the copies will drift — collapse to one home + a pointer.
- A rule added to the **wrong owner** (a service rule in a global file, or vice versa).

### Lens D — Concision of harness-reloaded docs

The harness reloads some docs into **every prompt** — root `AGENTS.md`/`CLAUDE.md`, every skill
`SKILL.md`, the `plans/agents/*` playbooks — so each added word there is a recurring cost. In those
files flag additions that carry no rule: restating a rule already stated, example blocks or preamble
that add words without adding constraint, narration. The fix is concrete: tighten in place / delete the
redundant lines. (Ordinary long-form docs under `docs/` and `plans/*_PLAN.md` are held to clarity, not
word-count — don't nit their length.)

### Lens E — Dangling / transient references

A reference engineered to dangle: a specific plan filename, "Phase N", a ticket number, or a scratch
doc that will be deleted, cited from a **durable** doc that outlives it. Per the repo's own rule such a
pointer belongs in a commit message, not baked into a doc that survives the artifact.

### Lens F — Followable instruction

An instruction a reader can't act on: a step that contradicts another step, an ambiguous "this/it" with
no referent, a "must/never" that conflicts with a "may" elsewhere in the same doc, a gate with no
stated pass condition.

## Step 4 — Confidence filter

Same bar as `review`. For each candidate, judge it's real and a reader will actually be
misled/blocked by it. **Drop anything below ~80/100.** Discard: pre-existing issues on unchanged lines;
pure preference re-wording that changes no meaning; a "violation" no convention actually states;
deliberate, noted exceptions.

**No hedged findings — a kept finding is one you'd fix.** Every surviving finding names a *concrete
edit you're prepared to apply* (fix the wrong path to X, delete the duplicated rule, reconcile doc Y).
No "your call" / "non-blocking, noting it" — above the bar → state the fix; below → drop it silently.
`LOW` still means "fix it."

## Step 5 — Finalize the review markdown

The file already exists (Step 1b) with findings appended. Reconcile the final list: placeholder gone,
findings grouped and ID'd. Path `reviews/<branch-slug>.md` (`/` → `-`).

```markdown
# Docs review — <branch>

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `<full-HEAD-sha>`  _(<today's ISO date>)_

> Range reviewed: `<short-start>..<short-head>` (N commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [ ] **<ID> — <SEVERITY> — <lens>** — `file_path:line`
  <one-line description + which doc/reality it contradicts, quoting the rule or the true fact>
```

Give each finding a short stable ID (`ACC1` accuracy, `CON1` contradiction, `HOME1`, `CONC1`, `DANG1`,
`INST1`) so `incremental-review` can append without renumbering. If a review file already exists,
**append** a dated `## Incremental review — <date>` section; preserve existing status marks. No
findings → write `No issues found. Checked accuracy vs reality, cross-doc contradiction, doc home &
convention, harness-reloaded concision, dangling references, and followable instruction.`

## Step 6 — Stamp the marker (mandatory)

Set the top-of-file marker to current HEAD — exactly one such line:

```
**Reviewed up to commit:** `<full-HEAD-sha>`  _(<today's ISO date>)_
```

Date from session context, SHA from `git rev-parse HEAD`. Don't commit unless asked, except a
plan-managed checkpoint required by Step 7.

## Step 7 — Checkpoint and report

Before any report or stop, if this workflow is plan-managed, read and apply
[the shared plan-progress checkpoint](../resume-plan/references/plan-progress-checkpoint.md).

Concise chat summary: range reviewed (`<short>..<short>`, N commits), finding counts by lens/severity
(or "none"), the file written, the stamped watermark. Point at the file; don't restate every finding.
