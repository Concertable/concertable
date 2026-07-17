# How reviews work (`reviews/*.md`)

A review file is a **work order for a branch's findings — not an archive.** Git history is the archive
(same philosophy as [`plans/CLAUDE.md`](../plans/CLAUDE.md) for plans, and the throwaway-markdown rule in
the root [`CLAUDE.md`](../CLAUDE.md)). A review left lying around after its findings are addressed and its
PR has merged is rot: it misleads the next reader into thinking work is still outstanding, and `reviews/`
silently fills with dead files.

Produced by `code-review` / `incremental-review` / `big-review`; consumed by `address-review` (which
already deletes a review once every finding is fixed cleanly). This file states the lifecycle so it holds
even when a review is produced or resolved by hand.

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
