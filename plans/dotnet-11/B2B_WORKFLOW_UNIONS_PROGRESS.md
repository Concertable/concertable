# B2B .NET 11 runtime progress

- Plan: `plans/dotnet-11/B2B_WORKFLOW_UNIONS_PLAN.md`
- Roadmap: `plans/dotnet-11/DOTNET_11_ROADMAP.md`
- Roadmap item: `dotnet-11/b2b-workflow-unions`
- Worktree: not created
- Branch: `Refactor/dotnet-11_b2b-runtime` (reserved; not created)
- Plan PR: #448 merged; historical closeout PR #449 does not authorize implementation
- Dependency/package gates: blocked on terminal delivery of `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md`
- Last reconciled: 2026-08-16 against the approved Application → Booking → Concert ownership decision

## Current state

No .NET 11 implementation exists. The previous design proposed unions over concrete workflow step
services. That target is superseded: the lifecycle refactor deletes the cross-stage workflow, keeps
DI behaviour behind module-local resolvers, and gives Application, Booking, and Concert independent
state machines. Native unions are selected for closed internal values, beginning with the read-only
combined journey projection and extending only to proven case-specific module states, triggers, and
operation outcomes.

The B2B typed-result dependency landed in PR #552. The lifecycle plan now owns the return path.

## Next Steps

Blocked: The Application, Booking, and Concert module/state refactor has not completed its delivery lifecycle.
Blocked by: plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md.
Unblock action: The lifecycle owner must land the approved module split and reconcile the required journey-stage union plus case-specific module state, trigger, and operation-outcome candidates against the resulting APIs and target graph.
Resume when: Current main contains the delivered lifecycle split and `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md` records every implementation, review, PR, publication, and platform-sync gate as terminal green.

## Completed work

- Established the B2B runtime/net10 Contracts compatibility boundary and SDK/Functions risks.
- Rejected unions over DI step implementations after the lifecycle ownership decision.
- Replanned the work as a runtime upgrade with native unions for closed internal values, never DI services.

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
- The lifecycle split owns state/resolver architecture; this plan cannot change it.
- Published Contracts and persisted/wire models remain union-free.

## Resume prompt

Not emitted while `## Next Steps` carries the hard-blocker fields. The lifecycle owner opens the gate
and supplies the implementation pointer.
