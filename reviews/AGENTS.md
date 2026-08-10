# How reviews work (`reviews/*.md`)

A review file is a **work order for a branch's findings — not an archive.** Git history is the archive
(same philosophy as [`plans/AGENTS.md`](../plans/AGENTS.md) for plans, and the throwaway-markdown rule in
the root [`AGENTS.md`](../AGENTS.md)). A review left lying around after its findings are addressed and its
PR has merged is rot: it misleads the next reader into thinking work is still outstanding, and `reviews/`
silently fills with dead files.

Produced by `review` / `incremental-review` / `big-review`; consumed by `address-review` (which
already deletes a review once every finding is fixed cleanly). This file states the lifecycle so it holds
even when a review is produced or resolved by hand.

## Addressing findings — fix the fixable part NOW; split, never defer-whole

A finding is **not all-or-nothing.** When a finding is partly fixable now and partly blocked, **split it:
fix the fixable part immediately and carry only the genuinely-blocked remainder** as its own `[ ]` item
that names the concrete blocker (e.g. "guards an endpoint that doesn't exist until phase N"). Deferring a
*whole* finding because one part can't be written yet is the exact anti-pattern this rule kills — it's how
a safe, isolated fix gets punted for no reason.

None of these is a reason to defer a fix you can write now — do the fix and note the caveat:

- **"It touches an already-committed / verified phase."** Addressing a review finding on that phase's own
  code is the review doing its job, not "starting the next phase." Fix it on the branch.
- **"It can't be fully tested until later."** Write the correct-by-inspection fix now; an absent test is a
  one-line caveat in the finding + commit message, not a blocker.
- **"It belongs to a future phase" / "it's only low-severity / latent."** Only the part that *literally
  cannot be written yet* waits; severity and latency never justify deferring a fixable defect.

Genuine deferral (`[-]`) is reserved for a real judgment call / tradeoff needing a human decision, or a
part with a hard blocker named. Default is: fix now.

**Deferring is never dropping — every `[-]` / `[wontfix]` MUST get a `TECH_DEBT.md` entry (owning area, with a `Resolves when:` line) in the same stroke.** The review dies at PR merge; tech debt is where the item persists. A `[-]` with no tech-debt entry is a silently-dropped finding.

## Branch ownership — findings stay with the reviewed change

Fix a finding on the branch being reviewed when that branch caused it, exposed it, worsened it, or
made the fix necessary. The finding is part of making that change reviewable; do not create a separate
review-fix branch.

Move a finding to its own branch only when it is demonstrably a pre-existing, wholly independent
defect: it already exists on the review base, the reviewed change neither exposes nor worsens it, and
its fix is independent of the reviewed change. Record the evidence for all three conditions in the
review work order before moving it. If any condition is uncertain, keep the fix on the reviewed branch.

## Lifecycle — delete once fully addressed

**Deleting a spent review is the default end state, not a later cleanup pass.** A review is spent — delete
it — the moment either holds:

- it found **nothing** ("No issues found") — it has no ongoing job the instant it's read; or
- **every finding is resolved** (fixed, or a conscious `[wontfix]` whose reason lives in the commit/PR or
  a `TECH_DEBT.md` line — *not* parked in the review file) **and the reviewed PR has merged.**

Delete it in the same stroke as the thing that spends it: address the last finding → delete; produce a
clean review to gate a merge → delete once that merge lands. An **untracked** review (never committed —
the usual case) is just `rm`'d; a **committed** one is `git rm`'d in the commit that resolves it.

## What stays

A review with **open, unaddressed findings** stays — that *is* its purpose. Tick `[x]` / mark `[wontfix]`
as you go so what remains is only what's genuinely open. Don't keep a review as a permanent record: if it
still has open findings when the PR merges, either they weren't real, or they belong in a follow-up
(a `TECH_DEBT.md` line / an issue), not a lingering file in `reviews/`.
