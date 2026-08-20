# Docs review — Docs/docs_polyrepo-ready-git-family

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `060479d593b75aadde27c6d08e7526d1d0c4abd7`  _(2026-08-20)_

> Range reviewed: `1176a002f..060479d59` (4 commits), plus the producer half in `Concertable/agent-standards`
> `2d9a8fe..9bf6b55` (PR #9), which this repo's consumer half depends on.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

Scope guard: 17 changed paths, all `.agents/**`, `.claude/**`, `docs/**` or `plans/**`. Docs-only —
`FULL.md` not required. `scripts/worktrees.ps1` is byte-identical to what was already present; only its
provenance entry changed.

**Lenses checked:** A (accuracy vs reality), B (sibling contradiction), C (right home / one-rule-one-home),
D (concision of harness-reloaded docs), E (dangling/transient references), F (followable instruction).

## Findings

- [x] **ACC1 — HIGH — Lens A** — `.agents/hooks/plan_handoff_stop.py:339`
  The Stop hook *generates* a handoff prompt whose opener is `/worktree create {owner_branch}` whenever a
  ledger's declared worktree directory is missing. This family renames the repository's `worktree` skill to
  `open-worktree`, so that emitted instruction no longer names it — and it does not dangle, which would be
  the safe failure. It **silently resolves to the user-global personal `worktree`**: the sibling-layout
  script that junctions untracked skill directories into the new checkout. That is precisely the hazard
  `git/WORKTREE.md` was written to prevent, and the hook would route the next agent straight into it. Also
  `create` was never this repo's grammar; it is the personal skill's. Fix upstream in `agent-standards`
  (the file is vendored, `delivery: invoked`) to emit `/open-worktree {owner_branch}`, then re-vendor.

- [x] **ACC2 — MEDIUM — Lens A** — `.agents/skills/resume-plan/assets/progress-template.md:58` and
  `.agents/skills/resume-plan/references/plan-progress-checkpoint.md:130`
  Both carry the literal `<cd existing-worktree OR /worktree create Type/epic_name>` in the resume-prompt
  template they hand to the next agent — the same stale invocation as ACC1, reaching agents through the
  ledger template rather than the hook. `resume-plan` is family 5's to move, but the **rename** obligation is
  this family's: the plan floor's rename gate is not satisfied while a citation of the old name survives.
  Update both to `/open-worktree Type/epic_name`.

- [x] **ACC3 — MEDIUM — Lens A** — `AGENTS.md:4-8`
  The load-on-demand skill roster enumerates `git-branching`, `committing`, `merging`,
  `remote-validation`, `plans`, `failing-tests`, `docs-and-debt`, then the review family, the merge/PR
  family and the test-debug family — every family moved so far, each added by the family that moved it.
  The git family is absent, so the sentence that claims to say how work gets done now under-reports it by
  six skills, and the two renamed names (`sync-checkout`, `open-worktree`) appear nowhere a reader would
  look. Extend the roster, matching the existing family-grouped form.

## Not flagged

- **`docs/INDEX.md` rows cite skills that do not exist yet on this machine.** `sync-checkout` and
  `open-worktree` resolve only once agent-standards #9 merges and the plugin cache is refreshed. That is the
  ordering every family in this plan has used — producer first, then consumer — and the ledger's Next Steps
  states it as a precondition. Correct-by-sequence, not a defect.
- **`.agents/README.md`'s starter-kit block still lists `worktree/` and `sync/`.** Those are the *personal*
  global skills, which genuinely keep the bare names; the paragraph beneath now states the split. Accurate.
- **Line-count claims in the plan and ledger** (429 lines, 396, 176 files) were each verified against the
  tree rather than taken from the previous ledger.
- Lens D: the only harness-reloaded file touched is `AGENTS.md` (via ACC3), and the fix there adds names to
  an existing list rather than new prose.

## Incremental review — `060479d59..HEAD`

Two additions after the review above: the `origin/main` currency merge (no conflicts, no content of its
own) and the `^reviews/.*\.md$` row in `.agents/skill-routes.json`.

- **Row verified, not eyeballed.** `review-lifecycle` resolves from the installed plugin cache at
  `2d9a8fedf0e7`, and `standards/process/review/LIFECYCLE.md` is present beside it, so the row names a
  skill that exists rather than one this family still owes. `skill_router.py --skills-for
  reviews/Docs-docs_polyrepo-ready-git-family.md` fires it. `.agents/hooks/tests` 19 passed / 48 subtests.
- **Bare name, per the table's own `_comment`.** `review-lifecycle` has one home, so it takes no
  `plugin:` qualifier — unlike the paired rosters above it.
- Lens C: no second copy created. `docs/INDEX.md:50` already names `review-lifecycle` as the owner; the
  row restores *delivery*, which is a different job from ownership.
