# B2B .NET 11 runtime progress

- Plan: `plans/dotnet-11/B2B_WORKFLOW_UNIONS_PLAN.md`
- Roadmap: `plans/dotnet-11/DOTNET_11_ROADMAP.md`
- Roadmap item: `dotnet-11/b2b-workflow-unions`
- Worktree: not created
- Branch: `Refactor/dotnet-11_b2b-runtime` (reserved; not created)
- Plan PR: #448 merged; historical closeout PR #449 does not authorize implementation
- Dependency/package gates: blocked on terminal delivery of
  `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md`
- Downstream dependent: `plans/launch/DEAL_CLOSED_SUM_MODEL_PROGRESS.md` requires the supported C# 15
  native-union/runtime and compiler/target matrix for its internal-operation and published Deal cut-overs
- Last reconciled: 2026-08-19 against the classified Deal dispatch decision

## Current state

No .NET 11 implementation exists. The previous design proposed unions over concrete workflow step
services. That target is superseded: the lifecycle refactor deletes the cross-stage workflow and gives
Application, Booking, and Concert independent state machines and contextual operations. Native unions
are selected for small closed internal values, beginning with the read-only combined journey
projection and extending only to proven case-specific module states, triggers, and operation outcomes.

The separate Deal plan selected a published closed record hierarchy, native unions for heterogeneous
internal operations, and generated invariant module factories for the honest same-interface terms,
mapper, and updater families. Lifecycle executors and steps are union/match operations, not factory
families. This plan
owns the prerequisite C# 15 native-union/runtime and compiler/target/consumer matrix; the Deal plan owns
its later internal-value migration, breaking package cut-over, and classification of provisional keyed
selectors.

The B2B typed-result dependency landed in PR #552. The lifecycle plan owns the return path.

## Next Steps

Blocked: The Application, Booking, and Concert module/state refactor has not completed its delivery lifecycle.
Blocked by: `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md`.
Unblock action: The lifecycle owner must land the approved module split and reconcile the required journey-stage union plus case-specific module state, trigger, and operation-outcome candidates against the resulting APIs and target graph. This plan must then record the C# 15 native-union/runtime and closed-hierarchy compiler/target matrix and notify the Deal dispatch ledger when both gates open.
Resume when: Current `main` contains the delivered lifecycle split and the lifecycle ledger records every implementation, review, PR, publication, and platform-sync gate as terminal green.

## Completed work

- Established the B2B runtime/net10 Contracts compatibility boundary and SDK/Functions risks.
- Rejected unions over DI step implementations after the lifecycle ownership decision.
- Replanned the work as a runtime upgrade with native unions for closed internal values, never DI services.
- Registered the Deal dispatch/representation plan as a downstream consumer of the supported C# 15
  native-union/runtime and target matrix.

## Verification

- Published B2B Contracts have net10 consumers and cannot become net11-only in this slice.
- Native unions do not resolve keyed services or DI lifetimes.
- No runtime verification applies while the implementation worktree is blocked and absent.

## Reviews

Historical reviews do not approve the superseded workflow-union target. This reconciled plan requires
a fresh docs review before implementation.

## Decisions, discoveries, blockers, and deviations

- The runtime upgrade is the platform gate for native union adoption.
- Native unions model closed values, not services; the journey-stage projection is the first required use.
- The lifecycle split owns state architecture; the Deal plan owns replacement of its provisional
  selectors after delivery with generated same-interface factories, unions, direct calls, or data.
- Published Contracts and persisted/wire models remain union-free in this runtime PR.
- The Deal contract's later closed hierarchy is not a native union and is outside this runtime PR.

## Resume prompt

Not emitted while `## Next Steps` carries the hard-blocker fields. The lifecycle owner opens the gate
and supplies the implementation pointer.
