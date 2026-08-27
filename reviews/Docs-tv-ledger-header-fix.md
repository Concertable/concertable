# Docs review — Docs/tv-ledger-header-fix

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `c7cb108a9f3c4a7cbdc3e8034b19501d2a03afb8`  _(2026-08-27)_

> Range reviewed: `28e31acb4..c7cb108a9` (2 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No findings. Checked Lens A (accuracy): every relocated fact was verified still present — either moved
into the ledger's own `## Current state` prose (`TENANT_VERIFICATION_PROGRESS.md`,
`REUNION_ALPHA2_BASELINE_PROGRESS.md`, `REUNION_SHARED_CONTRACTION_PROGRESS.md`,
`B2B_WORKFLOW_UNIONS_PROGRESS.md`, `POLYREPO_FULLSTACK_PROGRESS.md`), already redundant with existing
prose (`MUSIC_LICENCE_ATTESTATION_PROGRESS.md`'s "(created)"), or confirmed still available in the
ledger's own `## Resume prompt` section (`REPOSITORY_PER_MICROSERVICE_MIGRATION_PROGRESS.md`'s dropped
"see `## Resume prompt`" pointer — the section exists at line 483). Verified every fixed file's
`Worktree`/`Branch` line now parses correctly against `plan_graph.py`'s actual `metadata()` regex, run
directly in Python before and after. Lens B/C/D/E/F: no contradictions, all edits stay inside each
ledger's own required sections, none are harness-reloaded docs, no dangling references introduced, no
instruction broken.
