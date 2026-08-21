# Docs review — Docs/docs_polyrepo-ready-n4-architecture

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `2a76aa1464a025876ddb666606595e30774aee7e`  _(2026-08-21)_
**Security-reviewed up to commit:** `825e30323e01520e1833337999f34cc7b7f0a489`  _(2026-08-21)_ — docs/route-table only; no code, no runtime, no security-relevant change. Auth/Payment/Contracts paths touched are one-line doc-link repoints.

> Range reviewed: `4a478433a..2a76aa146` (3 commits — the substantive change is the single N4 commit
> `2a76aa146`; `a364bebbd` + `bd08b0402` are a merged-in `chore/platform-sync-0.1.0-alpha.0.1120` PR
> that only bumps `<ConcertablePlatformVersion>` pins).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **ACC1 — MEDIUM — accuracy (Lens A)** — `.github/workflows/claude-review.yml:34` — **fixed in PR #713** (dropped `api/ARCHITECTURE.md` from the read-list). A CI-workflow file, outside this meta PR's scope, so it rides the N4 non-doc follow-up alongside the `Directory.Build.props/.targets` and `*.Hosting.csproj` citations.
  This commit deletes `api/ARCHITECTURE.md`, but the CI review workflow's prompt still instructs the
  reviewer to "Read, and review against, the docs that ARE in the checkout: repo-root `AGENTS.md`,
  `api/ARCHITECTURE.md`, `docs/INDEX.md`, …". `api/ARCHITECTURE.md` is no longer in the checkout, so this
  is an inbound reference to a deleted file — exactly the class N4 exists to repoint. The runner does not
  have the re-homed skills vendored, so the fix is not a skill pointer but removal: drop `api/ARCHITECTURE.md,`
  from the list on that line (the remaining `AGENTS.md` + `docs/INDEX.md` still carry the premise pointers).
  Reference sits in a CI workflow — technically outside the markdown-docs lens — but the dead reference is a
  direct, permanent consequence of this PR's deletion.
