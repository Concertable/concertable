# Docs review — Docs/DeploymentScopeDeferral

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `27c9d387388d1e044fec4271d1df838e5ee32aa8`  `(2026-09-09)`
**Judgment:** `approved`

## Review pass — 2026-09-09 — docs

**Candidate base:** `081e149a2e0a4544e1c781adc5e322a6ee0e4f85`
**Candidate head:** `27c9d387388d1e044fec4271d1df838e5ee32aa8`
**Candidate branch:** `Docs/DeploymentScopeDeferral`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:6412878393176ddc7c938d3b44105ff751bf3f4005ca3fad7dc583988d4b53c2` `(1 path)`
**Work-order path:** `reviews/Docs-DeploymentScopeDeferral.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

Scope guard: the only surviving path is `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`,
which is `plans/**` and `**/*.md`. No runtime, package manifest, migration or CI test-selection path is
touched, so this is a docs review and the candidate is eligible for the meta-only merge path.

Lenses applied: accuracy, contradiction, dangling references, followability.

### Findings

- [x] **ACC1 — MEDIUM — accuracy** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md:873`
  The deferral note claimed `concertable`'s `Production` is *"the only environment in the organisation."*
  That is false. Enumerating all twelve repositories in the topology returns three environments:
  `concertable` → `Production`, `.github` → `release`, and `platform-frontend` → `release`. The claim was
  generalised from a five-repository sample, which is exactly the negative-claim-off-a-partial-search error
  the global instructions forbid. Corrected to state that `Production` is the only *deployment* environment
  and to name the other two as package-publication environments, which is both true and the distinction the
  argument actually rests on.

- [x] **CON1 — MEDIUM — contradiction** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md:869`
  Deferring 10E removes the promote-to-test step, but checkpoint 14's hard stop still read *"the system
  compatibility set **and deployed test configuration** contain no monorepo-built service image."* With no
  environment promoted to, that clause gates on an artefact that will not exist, leaving checkpoint 14
  unsatisfiable as written. Narrowed to the compatibility set, with the deployed-configuration half
  explicitly deferred alongside checkpoint 15.

### Verified clean

- **Accuracy of the evidence the deferral rests on.** The 2026-03-29 last-deployment date and the `master`
  ref were read from `repos/Concertable/concertable/deployments`. The absence of a deploying workflow was
  established by searching `.github/workflows/` for `environment:`, `azure/login`, `az webapp`, `azd deploy`
  and `terraform apply`, which returned nothing — a positive enumeration of the workflow directory, not an
  inference.
- **Dangling references.** The note points to checkpoint 15's own deferral text and to checkpoint 16's
  environment clause, both durable sections of the same plan. It depends on no ticket, scratch file or
  disposable phase.
- **Followability.** Each deferred item names its owner and its resume condition: 15 and 10E resume at first
  production release; checkpoint 16's environment clause is satisfied by deleting the stale `Production`
  environment. The original checkpoint 15 scope is retained in place rather than deleted, so the later work
  has its specification.
- **One-rule-one-home.** The deferral is recorded once, in the checkpoint that owns it, with 10E and
  checkpoint 16 carrying pointers rather than restating the reasoning.

### Merge path

Meta-only, so this lands by admin merge with the queue bypassed per `merge-docs`, with the `skip-e2e` label
applied so a fallback into the queue still skips the end-to-end suites. The end-to-end gate has nothing to
prove against a plan document.
