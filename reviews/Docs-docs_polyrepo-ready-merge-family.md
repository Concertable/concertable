# Docs review — Docs/docs_polyrepo-ready-merge-family

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `29a82099041170bf8ecf805aa8a876a0588fd65a`  _(2026-08-20)_

> Range reviewed: `2ce95368..29a82099` (1 commit), plus the producer branch it is gated on —
> `agent-standards` `Docs/polyrepo-ready-merge-family`, whose four new docs are this slice's real payload.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Scope classification

**This branch is a code PR by the path gate, on one comment.** `.github/workflows/test.yml` is a CI
workflow definition, which `standards/process/review/DOCS.md`'s scope guard and `merge/META_ONLY.md`'s
in-scope list both exclude by path — deliberately, and the exclusion is correct even though the edit is a
single comment line re-pointing a path this branch deletes. Consequences, both taken:

- it lands through `/merge`, not `/merge-docs`; and
- the code-review layer has no runtime surface to judge — 674 of 687 deleted lines are markdown and the
  remainder is prose — so the substantive review is the docs lenses below, run from the moved copy of the
  procedure (`standards/process/review/DOCS.md` on `agent-standards` `main`, merged as #6). That also
  discharges this family's own gate a second time: the moved review procedure runs end-to-end against a
  real diff.

Leaving the comment stale instead, to keep the branch meta-only, would have shipped a known Lens A defect
to `main`. Splitting it to a second PR would have left the same dead pointer live in between.

## Findings

Four found, all fixed on the producer branch before it was pushed; no open findings.

- [x] **HOME1 — MEDIUM — Lens C** — `standards/process/merge/QUEUE.md` step 5
  The moved step restated the worktree-close command's full refusal list (dirty, detached, post-PR,
  PR/head mismatch, missing ledger, case-colliding, persistent) — the same enumeration root `AGENTS.md`
  "Worktree cleanup" and `docs/INDEX.md` already own in the consuming repo, carried over verbatim from the
  deleted skill. A generic doc in another repo cannot link to that owner, but it does not need the list
  either: it needs the reader to run the command and trust the refusal. Collapsed to that, and the
  separate "a persistent branch is never removed" line went with it, since the same command already
  enforces it.
- [x] **INST1 — MEDIUM — Lens F** — `standards/process/merge/QUEUE.md` step 0
  "hand the user the command to run themselves, prefixed so it executes in their session outside the hook"
  named no prefix, so the instruction could not be followed. The old skill named `!` and dropping it in
  the name of portability deleted the actionable half of the rule. Restored, attributed to Claude Code,
  with the reason (it is the user's invocation, not the agent's) stated ahead of the harness-specific
  token.
- [x] **ACC1 — LOW — Lens A** — `standards/process/merge/QUEUE.md` step 2
  The staleness grep used a `<sync-branch-prefix>` placeholder while step 6 of the same doc used the
  literal `chore/platform-sync-`. One value written two ways in one file; a reader cannot tell whether
  they are the same thing. Made both literal — per the family-1 finding, a parameter with one value in
  every repo is worse than the value.
- [x] **ACC2 — LOW — Lens A** — `AGENTS.md` "Merging"
  The rewritten run-book pointer added "its Step 4 is the single source of truth for the E2E tier", which
  the "Validation is remote-first" section of the same file already states. Introducing a second copy of a
  rule in the always-loaded root file, in the very commit whose purpose is de-duplication. Clause removed;
  the validation section keeps sole ownership.

## Lenses checked

- **A — accuracy vs reality.** Every relative link in the four new docs and in the edited `MERGING.md`
  resolved by walking the tree, not by eye: zero broken. `PLANS.md` really carries the "Never leave the
  codebase out of sync" heading `PREFLIGHT.md` cites. `docs_reachability.py` → 0 errors, 26 warnings against 27 on this
  branch's base — zero added, and the one removed is this branch's own ledger header, which had carried a
  markdown link pointing at the literal placeholder `<url>`. Nothing was orphaned. No surviving reference anywhere in the
  repo names a deleted skill's path or the old `create-gh-pr` name.
- **B — contradiction with siblings.** The skill names `/merge` and `/merge-docs` are unchanged, so root
  `AGENTS.md`'s "Ready for review is not merge authorization" and its docs-push exception still resolve.
  `META_ONLY.md`'s new "a comment-only edit to a CI workflow still fails this gate by path" agrees with
  what this very branch had to do, rather than contradicting it.
- **C — right home.** HOME1 above. Otherwise: the four docs cite `MERGING.md`, `BRANCHING.md`,
  `COMMITTING.md`, `PLANS.md`, `REMOTE_VALIDATION.md` and the review family for rules those own, and
  `MERGING.md`'s own "the runnable loop belongs to whatever executable merge command the repository owns"
  was re-pointed at `merge/QUEUE.md` rather than left describing a command that no longer exists.
- **D — concision of reloaded docs.** Root `AGENTS.md` nets +1 line: four skill names added to the roster
  (the delivery mechanism, so the words carry a rule) against a shortened Merging pointer. ACC2 was the
  one addition that carried no rule.
- **E — dangling references.** No durable doc gained a plan filename, phase number or ticket. The plan and
  family are named only in commit messages and this review.
- **F — followable instruction.** INST1 above; every other gate in the four docs has a stated pass
  condition.
