# Code review — Docs/polyrepo_backend-extraction-proofs

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `<this-commit>`  `(2026-08-30)`
**Judgment:** `approved`

## Review pass — 2026-08-30 — docs

**Candidate base:** `2575cfdf14d36cba0967dca5532248dc09178735`
**Candidate head:** `<this-commit>`
**Candidate branch:** `Docs/polyrepo_backend-extraction-proofs`
**Candidate scope:** `all`
**Work-order path:** `reviews/Docs-polyrepo_backend-extraction-proofs.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

Single-file, docs-only diff: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PROGRESS.md`. Confirmed
in scope for `docs-review` — no runtime, product code, package manifest, migration, or CI-selection path
touched.

**Accuracy — every claim independently re-verified, not taken on trust from the source sessions:**

- `payment-next`, `auth-next`, `search-next`, `customer-next`, `b2b-next` — all confirmed to exist,
  private, `main` branch, via `gh repo view`.
- Commit counts personally re-derived via `git rev-list --count HEAD` on independent clones for all five
  (907 / 682 / 585 / 849 / 1345), not copied from another session's self-report.
- "0-error build" claimed for all five — personally rebuilt and confirmed `0 Error(s)` for all five,
  including Search and B2B, which the originating handoff sessions completed unattended; their claim was
  corroborated rather than assumed, since a created+pushed repo is strong but not direct evidence of a
  clean build.
- Auth's two-root rename claim (`Auth.Contracts` as a sibling top-level folder, not nested under `src/`)
  checked against the real `api/Concertable.Auth.Contracts/` folder shape (its own
  `Directory.Build.props`/`Directory.Packages.props`/`nuget.config`) before being written down as a
  decision, not invented.

**One-rule-one-home:** the new "standing authorization — parallel execution" line does not restate the
plan's own `## Execution rules` text; it records that *this specific epic's* stages 5–7 were explicitly
authorized to override that default, which is exactly the kind of durable decision the ledger's own
`## Decisions` section exists to hold.

**Contradiction:** the edited stage-6 line ("Auth extraction proof done; Duende move still separate") does
not duplicate the pre-existing Duende decision bullet — it summarizes current status in the stage list,
the decision bullet still carries the original reasoning.

**Dangling references:** none introduced — no reference to the four now-deleted `PARALLEL_EXTRACT_*.md`
scratch prompts, which never existed in this branch's history to begin with (they lived only in the main
checkout's working tree, untracked, and were deleted there directly).

**Followability:** the revised `## Next Steps` names two independently actionable options (stage 3 rt2;
folding Customer/B2B's frontend into their proofs) rather than one ambiguous item.

### Findings

No findings.
