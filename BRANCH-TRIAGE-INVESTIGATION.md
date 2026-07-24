# Investigation: which old unmerged branches actually shipped, and which genuinely need merging?

Working doc for a focused investigation. **Nothing here is a conclusion — verify everything.** Delete
this file when the question is settled and every branch below has a verdict + action taken.

You are being asked to **investigate and produce a verdict per branch**, not to merge or delete anything
yet. The whole point is that branch *names* and *PR titles* lie about what actually landed — so a
verdict must be grounded in **content**, never in what a branch is called.

## Why this exists (the trap)

The repo merges through a **squash/merge-queue**, so a branch's commits land in `master` under a *new*
SHA. `git branch --merged` therefore reports shipped branches as "unmerged" forever, and a leftover ref
looks identical to real outstanding work. Worse, multi-phase refactors were split into branches named
`-Phase1/2/3`, `-Merge1/2/3` — and some of those phases **did** ship while the base branch of the same
family still carries a few un-shipped commits on top. A human skimming the list cannot tell "done" from
"todo" by eye. That confusion is the thing to kill.

**Concrete example already found (verify, then use as the template):** the `DomainStereotypeLayout`
family. Every phase shipped via merged PRs **#126 (Merge 1), #128 (Merge 3), #133 (Phase 2), #135
(Phase 3), #137 (Phase 4), #142 (Phase 4 Merge 2), #149 (close out the plan)**. So there was **no
skipped phase** — the whole refactor is in `master`. Yet the local/origin branch
`Refactor/DomainStereotypeLayout` still shows `newContent=5` because it has a handful of **doc-only
commits added on top after the #126 merge** (`TESTS.md` split, DOC1/DOC2 link fixes, a tooling commit)
that were never PR'd. A draft PR was opened for it (**#189**) and mis-titled "Phase 1" — that title is
wrong and must be corrected or the PR closed; Phase 1 shipped months ago. Confirm this reading, then
decide whether those trailing doc commits are worth cherry-picking or just dropping.

## Method (apply to every branch below)

For each branch, establish which bucket it's in and **prove it**:

1. **Content-diff vs master, not ancestry.** `git cherry origin/master <ref>` marks each commit `+`
   (patch NOT in master) or `-` (equivalent patch already in master). `-` everywhere ⇒ shipped.
2. **Cross-reference the merged-PR log** for the same feature/scope/phase:
   `gh pr list --state merged --limit 100 --json number,title,headRefName`. A same-named or
   sibling-phase PR that merged is strong evidence the work landed under a different ref.
3. **For any `+` commit, inspect whether its *effect* is already in master** even if the patch-id
   differs (re-scaffold / squash / manual re-apply diverges the patch): pick a representative file the
   commit changed and compare the branch version to master (`git diff origin/master <ref> -- <path>`).
   Empty/equivalent ⇒ effectively shipped. Real delta ⇒ genuinely outstanding.
4. **Read the commit subjects for phase/step markers** (`Phase N`, `Merge N of M`, `Step N`) and map
   the whole family — did every phase ship, or is there a real gap?
5. **Judge staleness**: `git rev-list --count <ref>..origin/master` (how far behind). A branch 400–900
   behind with content genuinely outstanding is a *rebase-and-revive* decision, not a clean merge.

## Candidate branches (facts gathered 2026-07-23 — re-verify, don't trust blindly)

Format: `branch — newContent(git cherry '+') / commits-behind-master / age — lead commit subject(s)`

**The stereotype family (almost certainly ALL shipped — confirm, then clean up):**
- `Refactor/DomainStereotypeLayout` — 5 / 181 / 5d — base branch; `+` commits are trailing docs
  (`TESTS.md` split, DOC1, DOC2, tooling) after the merged #126. **Draft PR #189 open, mis-titled.**
- `Refactor/DomainStereotypeLayout-Phase2` — 0 / 171 — shipped (PR #133).
- `Refactor/DomainStereotypeLayout-Phase3` — 0 / 162 — shipped (PR #135).
- `Refactor/DomainStereotypeLayout-Merge3` — 0 / 177 — shipped (PR #128).

**Old real-code branches — determine shipped-elsewhere vs genuinely outstanding:**
- `Feature/MessagingOutbox` — 1 / 680 / 9wk — `Step 9: Transactional outbox in Concertable.Messaging`.
  Suspicion: superseded by the outbox work that shipped as **#162** (AsyncEmailOutbox phase 1) and
  **#179** (OutboxUnitOfWorkBehavior). Confirm whether "Step 9" is fully represented there.
- `Fix/DevSurfaceUrls` — 2 / 747 / 10wk — shares commit `c4fffa89 "ThreeSurfaceSplit 2 + dev surface
  URL fixes"` and `2a0c590c "mobile role sign up"` with the branch below.
- `Refactor/ThreeSurfaceSplit2` — 2 / 747 / 10wk — same two commits. The three-surface split clearly
  shipped (the app is three-surface today). Determine if these two commits contain anything — the dev
  surface URL fixes, the mobile role sign-up — that did **not** make it into the shipped split, or if
  they're a superseded early cut. (These two branches are near-duplicates — treat together.)
- `Refactor/UnifyReadModelMappingAndAddress` — 1 / 447 / 4wk — `refactor(tenant): drop test-only All
  accessor from permission sets`. Small; check if that specific change is in master already.

**Doc-only leftovers (code=0 — likely trivial, confirm then drop or salvage the doc):**
- `Feature/Dac7Onboarding` — 1 commit, `docs(b2b): trim CONFIG_STRATEGY to UK-only`.
- `Feature/UiE2eFlatFeeWorkflow` — 1 commit, 890 behind — ancient.
- `refactor/SharedQrCodeGenerator` — 1 commit, `docs: make working from a plan oblige reading
  plans/CLAUDE.md`. (Check: did this doc rule land via another PR? It reads like it should be in
  `plans/CLAUDE.md` already.)
- `rust-setup-backup` — 1 commit, 586 behind — a backup branch; almost certainly disposable.

**Bot branches (superseded platform pins — we're past them at ≥0.644):**
- `chore/platform-sync-0.1.0-alpha.0.590`, `chore/platform-sync-0.1.0-alpha.0.604` — stale sync PRs'
  branches; verify no manual consumer-migration commit is stranded on them, then they're deletable.

## What a good answer looks like

A **verdict table**, one row per branch: `branch | bucket | evidence | recommended action`.

Buckets:
- **SHIPPED** — content fully in master via other PR(s); ref is litter → **delete** (local + origin).
- **PARTIAL** — most shipped, but *these specific commits* (name them) are genuinely not in master →
  decide per commit: cherry-pick onto a fresh branch, or drop as obsolete.
- **SUPERSEDED** — the work was redone differently and the newer version won → **delete**, don't revive.
- **OUTSTANDING** — genuinely unshipped and still wanted → say what it'd take (rebase distance,
  conflicts, whether it still applies) to land it.
- **DISPOSABLE** — backup/experiment with no value → delete.

For the stereotype family specifically: confirm no phase was skipped, then state plainly what (if
anything) on the base branch is worth keeping, and **fix or close the mis-titled draft PR #189**.

Then, only after the table is agreed: execute the deletes (respect the **case-collision guard** — never
delete a branch a worktree holds, any casing; the `/unmerged` skill encodes this), and for OUTSTANDING
items open properly-titled PRs or say why not.

Do not merge or delete anything before the verdict table is reviewed. Report the table first.
