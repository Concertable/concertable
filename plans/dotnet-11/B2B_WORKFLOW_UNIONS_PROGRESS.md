# B2B .NET 11 runtime progress

- Plan: `plans/dotnet-11/B2B_WORKFLOW_UNIONS_PLAN.md`
- Roadmap: `plans/dotnet-11/DOTNET_11_ROADMAP.md`
- Roadmap item: `dotnet-11/b2b-workflow-unions`
- Worktree: not created
- Branch: `Refactor/dotnet-11_b2b-runtime`
- Plan PR: #448 merged; historical closeout PR #449 does not authorize implementation
- Dependency/package gates: blocked on terminal delivery of
  `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md`
- Downstream dependent: `plans/launch/DEAL_CLOSED_SUM_MODEL_PROGRESS.md` requires the supported C# 15
  native-union/runtime and compiler/target matrix for its method-header and published Deal cut-overs
- Last reconciled: 2026-08-21 against the direct method-header interface-union decision

## Current state

The reserved branch name above has not been created yet. No .NET 11 implementation exists. The previous design proposed unions over concrete workflow step
services. That target is superseded: the lifecycle refactor deletes the cross-stage workflow and gives
Application, Booking, and Concert independent state machines and contextual operations. Native unions
are selected for small closed internal values, beginning with the read-only combined journey
projection and extending only to proven case-specific module states, triggers, and operation outcomes.

The separate Deal plan selected a published closed record hierarchy, direct native interface unions for
heterogeneous method headers, and invariant module factories for the honest same-interface terms,
mapper, and updater families. Lifecycle executors and steps use dedicated factories plus type-pattern
matches, not the common-interface strategy factory. This plan
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
- Rejected the cross-stage union over DI step implementations after the lifecycle ownership decision.
- Replanned this work as a runtime upgrade with native unions for closed internal values; the downstream
  Deal plan owns direct unions of module-local heterogeneous method-header interfaces.
- Reconciled the landing shape against current C# 15 union semantics: direct interface cases replace the
  net10 marker return without wrapper records, while keyed DI remains confined to the net10 factory.
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
- This plan's native unions model closed values; the journey-stage projection is the first required use.
- The Deal plan may later use native unions over module-local heterogeneous method-header interfaces;
  those unions do not restore the rejected cross-module workflow or service location. The net10 factory
  may use keyed DI internally, while the .NET 11 union is `union Accept(IStandardAccept, IPrepaidAccept)`
  rather than a union of wrapper records or concrete implementations.
- Required invocation input is narrowed with an ordinary `when ... is not null` pattern arm. Its missing
  arm returns the typed Result failure; it does not use a throwing requirement helper or weaken a method
  header to accept nullable input.
- The lifecycle split owns state architecture; the Deal plan owns replacement of its provisional
  selectors after delivery with invariant same-interface factories, method-header unions, direct calls,
  or data. The later public dispatch package may replace their internal keyed implementation.
- The current [C# union reference](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/union)
  permits interfaces as union cases and defines unions over types, not raw method declarations. Therefore
  the .NET 11 boundary can directly use `IStandardAccept` and `IPrepaidAccept` without adapter records.
- [Dunet](https://github.com/domn1995/dunet) requires nested partial-record variants. It remains suitable
  for data/error unions but is rejected for this net10 DI boundary because it would add the wrapper cases
  and `.Value` access that the native interface union later removes.
- Published Contracts and persisted/wire models remain union-free in this runtime PR.
- The Deal contract's later closed hierarchy is not a native union and is outside this runtime PR.

## Resume prompt

Not emitted while `## Next Steps` carries the hard-blocker fields. The lifecycle owner opens the gate
and supplies the implementation pointer.
