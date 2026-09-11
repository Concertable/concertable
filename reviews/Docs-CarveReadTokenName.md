# Documentation review — Docs/CarveReadTokenName

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `249780f03d7e3de434b18eba43b44a4676899de9`  `(2026-09-11)`
**Judgment:** `approved`

Corrects [`#1006`](https://github.com/Concertable/concertable/pull/1006), which named the wrong secret
for a carve repository's restore steps. Meta-only: one plan file, no deletions.

## Review pass — 2026-09-11 — docs

**Candidate base:** `3c56aa040973ae4ec8cd7d9163a172d9ecc524fb`
**Candidate head:** `c80a4a5260940c5e3c7c2868cb595cbdb97df193`
**Candidate branch:** `Docs/CarveReadTokenName`
**Candidate scope:** `all`
**Candidate path-set:** `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md` `(1 path)`
**Work-order path:** `reviews/Docs-CarveReadTokenName.md`
**Work-order mode:** `new`
**Pass judgment:** `changes-requested`

Scope guard: the single frozen path is `plans/**`, so the docs route is correct. Insertions and
in-place corrections, not a close-out, so the lenses apply. Routed rules: `plans`, `docs-and-debt`.

Lenses run: accuracy, contradiction, one-rule-one-home. Concision, dangling references and followability
were not dispatched: the change is a four-line factual correction in the register the surrounding
sections already set. `docs_reachability.py` not run: no agent-instruction path changed.

### Findings

- [x] **CON1 — HIGH — contradiction** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md:752`
  Checkpoint 7C claims the org reusable `nuget-publish` workflow was authenticated with
  `CONCERTABLE_PACKAGES_TOKEN` "instead of `GITHUB_TOKEN`" and marks it **Done 2026-09-10**. The rail
  defect this branch adds shows that is true of the push step only: the `verify` job still hardcodes
  `GITHUB_PACKAGES_TOKEN` and `NUGET_AUTH_TOKEN` from `secrets.GITHUB_TOKEN` at the pinned `a8911e8`.
  A reader of 7C alone would take the cutover as complete. Fixed in `249780f`: 7C is narrowed to the
  push step and points forward to the token-scope section.

- [x] **ACC1 — MEDIUM — accuracy** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md:1119`
  The added paragraph switched to the bare name `PACKAGES_TOKEN` where every other occurrence in the
  document says `CONCERTABLE_PACKAGES_TOKEN`, without saying it is the reusable workflow's own
  `workflow_call` input name, so it read as a second undocumented secret. Fixed in `249780f` with a
  gloss — and writing the gloss exposed that the remedy the paragraph proposed was wrong: that input is
  declared `write:packages`, so routing it to the restore would make every consumer hold a write token
  merely to pack. The paragraph now says the workflow needs a read credential it can accept instead.

Verified against the reusable workflow's source at the pinned SHA rather than the lens report:
`Concertable/.github`'s `nuget-publish.yml@a8911e8` declares `secrets.PACKAGES_TOKEN` as
"Account-scoped token with `write:packages`"; its `verify` job sets
`env: {GITHUB_PACKAGES_TOKEN: "${{ secrets.GITHUB_TOKEN }}"}` and a step-level
`NUGET_AUTH_TOKEN: "${{ secrets.GITHUB_TOKEN }}"`; only the push step uses
`secrets.PACKAGES_TOKEN || secrets.GITHUB_TOKEN`. The consumer secret name was read off the repository:
`gh api repos/Concertable/payment/actions/organization-secrets` returns `CONCERTABLE_PACKAGES_READ`, and
`Concertable/payment` CI went green on `b76b179` once restore referenced it.

No findings from one-rule-one-home: the read/write split is stated once, in the secrets-distribution
list, and the token-scope section points at it.
