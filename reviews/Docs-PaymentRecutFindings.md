# Documentation review — Docs/PaymentRecutFindings

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `b222513ac54ec8a99f889096730f478a787fbf82`  `(2026-09-11)`
**Judgment:** `approved`

Checkpoint 11 (10A/10B) of [`REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`](../plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md).
Meta-only: one plan file, no deletions. Records what the `payment` re-cut settled, beside the existing
`auth`, `payment`/`search` and `customer` rehearsals.

## Review pass — 2026-09-11 — docs

**Candidate base:** `d6f986c2570d994891767bcb1a10fd797d2b854e`
**Candidate head:** `917e802d39574c7c9f3842fcf724bc565b938e36`
**Candidate branch:** `Docs/PaymentRecutFindings`
**Candidate scope:** `all`
**Candidate path-set:** `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md` `(1 path)`
**Work-order path:** `reviews/Docs-PaymentRecutFindings.md`
**Work-order mode:** `new`
**Pass judgment:** `changes-requested`

Scope guard: the single frozen path is `plans/**`, so no runtime, package manifest, migration or CI
test-selection path is in the candidate and the docs route is correct. Not a pure close-out — the net
diff is insertions only — so the lenses below apply. Routed rules: `plans`, `docs-and-debt`.

Lenses run: accuracy, contradiction, one-rule-one-home, recurring-context concision, dangling
references, followability. `docs_reachability.py` not run: no agent-instruction path changed.

### Findings

- [x] **CON1 — MEDIUM — contradiction** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md:829`
  The divergence section's standing reason for preferring `format-patch`/`am` over `git rebase --onto`
  is that "the two extractions share no ancestor". The added subsection asserts the opposite and names
  the boundary on both measured targets, so the document said two incompatible things about the same
  pair and a reader would trust whichever they reached first. The added claim is the verified one:
  `auth`'s own resolve commit names `532a3a6`, and `payment`'s merge base against its pushed `main` is
  `b9efa7e`. Fixed in `b222513`: the earlier sentence now says the shared ancestry exists but is too old
  to rebase onto, which preserves its conclusion, and points at the subsection that measures it.

- [x] **ACC1 — MEDIUM — accuracy** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md:1046`
  The `BaseOutputPath` bullet described the escape in the present tense — "walks five directories up" —
  while the pull request it records is the one that fixes it. A reader checking
  `Concertable.Payment.E2ETests.Web` finds `$(ConcertableServiceRoot)` and concludes the finding is
  wrong, which is how a correct section loses its authority. Fixed in `b222513`: past tense for what
  the cut produced, present for the anchor that replaced it, and `customer`'s still-open case carries a
  date rather than "is sitting in".

- [x] **DANG1 — MEDIUM — dangling references** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md:1010,1098`
  The re-cut subsection opened with no date, alone among the subsections in this file — every sibling
  opens `Surveyed`/`Rehearsed`/`Run`/`Settled` plus a date — and the read-scope paragraph said the
  finding "is what stops `Concertable/payment#5`", making durable guidance depend on an open pull
  request's state. Fixed in `b222513`: both are dated observations, and `auth#6` keeps its immutable
  merge SHA while the `payment` citation no longer carries a live disposition.

- [x] **HOME1 — LOW — one-rule-one-home** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md:1028`
  "no active carve repository is force-pushed" is owned by `## Git history preservation` step 5, which
  states it as a non-negotiable of the whole split. The added subsection restated it as its own bolded
  conclusion, giving one invariant two independently worded homes. Fixed in `b222513`: the subsection
  now says the reachable history is *how* step 5 is satisfied here, and cites it.

- [x] **HOME2 — MEDIUM — one-rule-one-home** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md:1098`
  The added paragraph put a durable, cross-cutting operational instruction — every carve repository
  needs `CONCERTABLE_PACKAGES_TOKEN`, set as part of 10B — inside a dated narrative section whose
  subject is package *binding* and the write-side publish token. Which secret a service repository
  receives is owned by the secrets-distribution list under `## CI/CD, environments, secrets, and
  deployment`. Fixed in `b222513`: the instruction moves into that list's first bullet, and the
  token-scope section keeps the `NU1301`/403 evidence and points at it.

Verified against repository evidence rather than the lens reports: `auth`'s resolve commit `675a00c`
states the `532a3a6` boundary and the `-s ours` resolution in its own message, and its tree is
identical to its first parent's; `payment`'s merge base against `Concertable/payment`'s `main` is
`b9efa7e` with 691 conflicted paths on an ordinary merge; both E2E host projects now read
`$(ConcertableServiceRoot)`, which `Directory.Build.props` defines; `eng/repository-split/map.yaml`
excludes exactly `Concertable.Payment.E2ETests.Helpers` and `.Helpers.UnitTests` from `payment` and
assigns both to `system`; `Concertable.AppHost.Shared` and `Concertable.Auth.Hosting` are both private,
both bound to `Concertable/concertable`, and both carry `0.1.0-alpha.0.1370`.

One lens reading was not taken: the accuracy lens offered, as an alternative to the tense fix, that the
`BaseOutputPath` finding might be a `customer`-only defect misattributed to `payment`. It is not —
`payment`'s two suites wrote to the escaped directory before `e8db84d` in `Concertable/payment#5`
changed them. The lens saw the post-fix tree, which is exactly what ACC1 records.

No findings from recurring-context concision (a migration plan is not recurring context) or
followability.
