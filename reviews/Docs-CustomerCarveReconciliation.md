# Documentation review — Docs/CustomerCarveReconciliation

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `74b796167c93d5128c9fc98bbcd4d08f75e4f392`  `(2026-09-11)`
**Judgment:** `approved`

Checkpoint 13 (10A/10B) of [`REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`](../plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md).
Meta-only: one plan file. Records what the `customer` re-cut settled, beside the `auth`, `payment`/`search`
and `customer` rehearsals and the `payment` re-cut, and closes that subsection's open item against
`customer`.

## Review pass — 2026-09-11 — docs

**Candidate base:** `10ff986516bef1cb70e9f4fd0413dd9402e592bd`
**Candidate head:** `74b796167c93d5128c9fc98bbcd4d08f75e4f392`
**Candidate branch:** `Docs/CustomerCarveReconciliation`
**Candidate scope:** `all`
**Candidate path-set:** `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md` `(1 path)`
**Work-order path:** `reviews/Docs-CustomerCarveReconciliation.md`
**Work-order mode:** `new`
**Pass judgment:** `changes-requested`

Scope guard: the single frozen path is `plans/**`, so no runtime, package manifest, migration or CI
test-selection path is in the candidate and the docs route is correct. Not a pure close-out — the net
diff carries insertions — so the lenses below apply. Routed rules: `plans`, `docs-and-debt`.

Lenses run: accuracy, contradiction, one-rule-one-home, recurring-context concision, dangling
references, followability. `docs_reachability.py` not run: no agent-instruction path changed.

**Lenses were run in-session rather than dispatched as fresh agents**, because this session is
instructed not to use the Agent tool unless the user asks. The independence the skill buys from fresh
lenses is therefore absent; both findings below are evidenced against the repository rather than
asserted, so they can be re-checked without re-running the pass.

### Findings

- [x] **ACC1 — MEDIUM — accuracy** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md:1077`
  The new subsection attributed a quotation to the rehearsal section — *"reconcile the solution in both
  directions"* — that appears nowhere in it. `grep -c` for that string matched only the new line itself;
  the rehearsal's actual wording is "Reconcile the solution against `git ls-files '*.csproj'` in both
  directions, not just for dangling entries." A reader would have searched for the quoted phrase and not
  found it. Fixed in `74b7961`: the sentence now paraphrases without quotation marks, and the
  substantive claim about what that instruction leaves out is unchanged.

- [x] **CON1 — MEDIUM — contradiction** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md:1186`
  The dated readiness table still gave `customer`'s remaining work as "13 fixes, all in the `.slnx`;
  green build needs disk". The new subsection directly contradicts that: the build is proven, the
  `.slnx` reconciliation is landed, and `PARALLELISATION_READINESS.md` had already retired the disk
  claim. Left alone, the document stated two incompatible readiness positions for the same target.
  Fixed in `74b7961`: the row now reports the pushed PR and points at the new subsection, with the
  real remaining gate named. Only the `customer` row was touched — the `auth`, `payment`, `search` and
  `b2b` rows are equally stale but belong to their own tracks, and the table is explicitly stamped
  `2026-09-10`.

### Not raised

- The table's footer cites `~/.claude/plans/Concertable/10A_customer_FINDINGS.md`, a scratch file
  outside the repository, which the dangling-references lens would flag. Pre-existing and unchanged by
  this candidate, so it is out of scope per the lens rules rather than fixed here.

## Watermark

`74b796167c93d5128c9fc98bbcd4d08f75e4f392` — `Docs/CustomerCarveReconciliation`, reviewed to head, no findings open.
