# B2B .NET 11 runtime and native workflow unions progress

- Plan: `plans/dotnet-11/B2B_WORKFLOW_UNIONS_PLAN.md`
- Worktree: not created
- Branch: `Refactor/dotnet-11_b2b-workflow-unions` (reserved; not created)
- PR: not opened
- Dependency/package gates: blocked on the terminal B2B typed-result lifecycle recorded in
  `plans/typed-result/B2B_PROGRESS.md`
- Last reconciled: 2026-08-09 against `origin/main` `b5af92fdc`, the current typed-result/ReUnion
  ledgers, B2B project references, workflow source, CI pins, and official .NET/Functions guidance

## Current state

The roadmap, implementation plan, and this recovery ledger exist on the isolated docs-planning branch.
No implementation worktree, SDK installation, `global.json`, target-framework edit, workflow refactor,
test run, package publication, or runtime deployment has occurred.

The current design uses native unions only after ReUnion and the existing B2B typed-result work have
landed. Published B2B contracts stay net10-compatible; the net11 boundary is the B2B runtime and its
reverse build/test consumers. The final workflow model uses unions over concrete cases, concrete types
for single implementations, explicit optional capability data, and exhaustive guarded dispatch. It
does not introduce `IAcceptStep` or place step interfaces inside union cases.

## Next Steps

Wait for `plans/typed-result/B2B_PROGRESS.md` to record that its checkpoints 6-7 source PR and every
resulting publication/platform-sync gate are terminal and green. That owner will update this ledger and
surface the resume prompt. Then create `Refactor/dotnet-11_b2b-workflow-unions` from fresh
`origin/main`, execute Phase 0, and stop after committing the independently green Phase 1 platform-only
.NET 11 checkpoint. Do not create the worktree, copy the overlapping Concert workflow changes, or begin
native unions before the owner opens the gate.

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

Pending docs review of the planning branch.

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

## Resume prompt

```text
/worktree create Refactor/dotnet-11_b2b-workflow-unions
Read @plans/dotnet-11/B2B_WORKFLOW_UNIONS_PLAN.md and @plans/dotnet-11/B2B_WORKFLOW_UNIONS_PROGRESS.md and do what its `## Next Steps` says.
```
