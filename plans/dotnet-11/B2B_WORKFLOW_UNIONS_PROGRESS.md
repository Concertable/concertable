# B2B .NET 11 runtime and native workflow unions progress

- Plan: `plans/dotnet-11/B2B_WORKFLOW_UNIONS_PLAN.md`
- Worktree: not created
- Branch: `Refactor/dotnet-11_b2b-workflow-unions` (reserved; not created)
- PR: #448 — https://github.com/Concertable/concertable/pull/448
- Dependency/package gates: blocked on the terminal B2B typed-result lifecycle recorded in
  `plans/typed-result/B2B_PROGRESS.md`
- Last reconciled: 2026-08-09 against `origin/main` `43fe1caf4`, the current typed-result/ReUnion
  ledgers, B2B project references, workflow source, CI pins, and official .NET/Functions guidance

## Current state

The roadmap, implementation plan, and this recovery ledger exist on the isolated docs-planning branch.
Docs-only PR #448 is open from verified head `cc2f7becf`; it targets `main`, carries `skip-e2e`, and its
diff contains only the roadmap, plan, new ledger, and B2B owner ledger. The full docs review found three
issues, all fixed in `44b435779`; the blocked-ledger reconciliation review found two more, all fixed in
`0a7b0d181`; both incremental reviews were clean. No implementation worktree, SDK installation,
`global.json`, target-framework edit, workflow refactor, test run, package publication, or runtime
deployment has occurred.

The current design uses native unions only after ReUnion and the existing B2B typed-result work have
landed. Published B2B contracts stay net10-compatible; the net11 boundary is the B2B runtime and its
reverse build/test consumers. The final workflow model uses unions over concrete cases, concrete types
for single implementations, explicit optional capability data, and exhaustive guarded dispatch. It
does not introduce `IAcceptStep` or place step interfaces inside union cases.

## Next Steps

Blocked: The B2B typed-result checkpoints 6-7 source PR and every resulting publication/platform-sync gate are not yet terminal and green.
Unblock action: The owner at `plans/typed-result/B2B_PROGRESS.md` must finish checkpoints 6-7, merge the source PR, follow every resulting publication/platform-sync gate to terminal green, then update this ledger.
Resume when: Current main contains the merged B2B work and `plans/typed-result/B2B_PROGRESS.md` records the source PR and every resulting publication/platform-sync gate as terminal and green.

## Completed work

- Investigated the current .NET 11/C# union state, preview support policy, Azure Functions hosting
  limitation, ReUnion target frameworks, B2B project-reference closure, cross-service contract
  consumers, CI framework pins, and existing workflow design.
- Selected a two-checkpoint, one-PR implementation: platform/toolchain first, then one coherent
  interface-to-union workflow cutover.
- Created the `.NET 11` roadmap, implementation plan, and this companion ledger.
- Registered this plan as a downstream handoff in the authoritative B2B typed-result ledger.

## Verification

- Read-only source inventory confirmed published B2B contracts are consumed by net10 Customer, Search,
  and Payment projects and therefore cannot become net11-only in this slice.
- Read-only workflow inventory confirmed the concrete Apply, Accept, checkout, finish, book, cancel, and
  application-cancel step families plus the marker-interface/reflection registry design.
- Read-only CI inventory found explicit .NET 10 SDK and B2B Playwright output assumptions that belong in
  the platform checkpoint.
- No runtime verification is applicable yet because the implementation worktree is intentionally
  blocked and has not been created.

## Reviews

- Full docs review: `b5af92fdc..9f4993214` (1 commit), artifact
  `reviews/Docs-dotnet-11_b2b-workflow-unions-plan.md`, watermark `9f4993214`.
- `HOME1` fixed in `44b435779`: removed plan-to-roadmap coupling.
- `INST1` fixed in `44b435779`: corrected the roadmap invocation to `/resume-plan`.
- `ACC1` fixed in `44b435779`: corrected the B2B owner's premature “on current main” claim.
- Incremental docs review: `9f4993214..44b435779` (1 commit), no new findings, same artifact,
  watermark `44b435779`. Open findings: none.
- Delivery-base schema reconciliation review: `44b435779..abddcf39b`; `INST2` and `INST3` fixed in
  `0a7b0d181`.
- Incremental docs review: `abddcf39b..0a7b0d181` (1 commit), no new findings, same artifact,
  watermark `0a7b0d181`. Open findings: none.

## Decisions, discoveries, blockers, and deviations

- The B2B typed-result owner has exclusive overlapping ownership of Concert payment/cancel/finish until
  its complete delivery lifecycle is terminal. This plan waits rather than rebasing or duplicating it.
- “B2B on .NET 11” is a runtime boundary, not a directory-wide search/replace. Published B2B contracts
  and the shared seed simulator remain net10-compatible so other services stay independently buildable.
- Keep one Apply endpoint and one Accept endpoint. Conditional payment input is operation validation for
  a selected union case, not evidence that the endpoint must split.
- A guarded paid case requires an unguarded paid failure arm even though the union's type cases are
  exhaustive; the guard partitions request state inside that case.
- Application executor interfaces remain because they are dependency-inversion ports. Workflow step and
  capability interfaces are removed because they are currently standing in for closed alternatives.
- Preview churn is acceptable only because the native union types remain internal, concentrated, and
  absent from persisted/published contracts.
- Approximately 30 UI E2E scenarios materially reduce regression risk but do not guarantee correctness;
  focused unit/integration coverage and the full merge-queue E2E gate remain mandatory.
- Azure Functions hosted deployment is blocked while the service matrix excludes net11. The preview
  branch may still be built, tested, reviewed, and merged for this unreleased project; the roadmap keeps
  a separate GA/deployment-readiness item open.

## Event log

### 2026-08-09 — investigation and plan drafted

- Action: audited the current SDK/TFM graph, cross-service package consumers, workflow interfaces and
  concrete steps, typed-result ownership, CI assumptions, official .NET union status, and Azure
  Functions support matrix; drafted the roadmap, plan, and recovery ledger on a branch from current
  `origin/main`.
- Evidence: `origin/main` `b5af92fdc`; repository source/project/workflow scans; official sources linked
  from the plan.
- Outcome: selected a net11 B2B runtime boundary with net10 contracts and one coherent concrete-case
  native-union workflow model, blocked behind the existing B2B owner.
- Follow-up: docs review and docs-only delivery; then wait for the registered owner handoff.

### 2026-08-09 — full docs review fixes

- Action: reviewed the committed plan through accuracy, contradiction, doc-home, concision, dangling
  reference, and followable-instruction lenses; fixed all three findings.
- Evidence: `reviews/Docs-dotnet-11_b2b-workflow-unions-plan.md`; `HOME1`, `INST1`, and `ACC1`.
- Outcome: the plan is decoupled from its roadmap, its resume command is followable, and the owner
  ledger reports the actual unmerged docs-branch state.
- Follow-up: commit the fixes, run incremental docs review, then deliver through `/merge-docs`.

### 2026-08-09 — clean incremental docs review

- Action: reviewed only the committed docs-review fixes after the recorded watermark.
- Evidence: `9f4993214..44b435779`; review watermark `44b435779`; relative-link, plan/roadmap
  decoupling, official-source, repository-path, and `git diff --check` verification.
- Outcome: no new findings; all three original findings remain resolved.
- Follow-up: checkpoint this review result and deliver the docs-only branch through `/merge-docs`.

### 2026-08-09 — current-main blocked-ledger reconciliation

- Action: merged current `origin/main` and applied its new three-line hard-blocker contract to this
  waiting ledger, the resolver roadmap instruction, and the B2B owner's downstream entry.
- Evidence: `origin/main` `43fe1caf4`; plan-handoff framework PR #447; the structured `## Next Steps`
  fields and matching owner gate.
- Outcome: resuming the blocked ledger is now explicitly forbidden; the existing B2B owner remains the
  only route that can open and surface this plan.
- Follow-up: review and checkpoint this reconciliation, then deliver through `/merge-docs`.

### 2026-08-09 — blocked-ledger reconciliation review fixes

- Action: reviewed the reconciliation against the newly merged hard-blocker rules and fixed both
  followable-instruction defects.
- Evidence: `reviews/Docs-dotnet-11_b2b-workflow-unions-plan.md`; `INST2` and `INST3`.
- Outcome: the roadmap now routes to the actionable ReUnion resolver, while both waiting ledgers carry
  structured blocker state and suppress their own pointers.
- Follow-up: commit the fixes, run incremental review, then deliver through `/merge-docs`.

### 2026-08-09 — clean blocked-handoff incremental review

- Action: reviewed only the committed `INST2`/`INST3` fixes after the reconciliation watermark.
- Evidence: `abddcf39b..0a7b0d181`; review watermark `0a7b0d181`; structured-blocker, suppressed-pointer,
  relative-link, resolver-routing, and `git diff --check` verification.
- Outcome: no new findings; all five findings across the planning lifecycle are resolved.
- Follow-up: checkpoint this result, publish the docs branch, and land it through `/merge-docs`.

### 2026-08-09 — verified docs work-head push

- Action: pushed the reviewed docs work head and fetched its remote-tracking ref for equality.
- Evidence: local and remote `Docs/dotnet-11_b2b-workflow-unions-plan` both
  `85ab55794ff870b7d8ae434746a1089eeb246afd`; no PR existed; branch was current with `origin/main`.
- Outcome: the four-path Markdown-only plan diff is durably published. Review watermark corrected to
  resolvable full SHA `0a7b0d181d44213cfcb942336d34310cdea50156`.
- Follow-up: transport this checkpoint, open and verify the docs PR, then restore the implementation
  blocker before admin merge.

### 2026-08-09 — docs PR opened and verified

- Action: transported the push checkpoint, opened PR #448, added `skip-e2e`, and verified its identity
  and diff.
- Evidence: local, remote, and initial PR head `cc2f7becfcd7a66be755e9e2a1f85ec56afbb924`;
  base `main`; head `Docs/dotnet-11_b2b-workflow-unions-plan`; exactly four Markdown paths.
- Outcome: the reviewed docs-only PR is ready for the sanctioned admin merge, and the durable ledger
  has returned to the implementation blocker that remains after delivery.
- Follow-up: transport this PR-state checkpoint, reverify all heads/paths, and admin-merge PR #448.

## Resume prompt

Not emitted while `## Next Steps` carries the hard-blocker fields. When the gate opens, the B2B owner
replaces the blocked state with an actionable Phase 0/1 handoff and restores the implementation pointer.
