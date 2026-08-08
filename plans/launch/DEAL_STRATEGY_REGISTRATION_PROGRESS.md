# Deal-type strategy registration refactor progress

- Plan: `plans/launch/DEAL_STRATEGY_REGISTRATION_PLAN.md`
- Worktree: `C:\Users\tommy\source\repos\Concertable`
- Branch: `Refactor/launch_deal_strategy_registration`
- PR: not opened
- Dependency/package gates: none; this is an internal B2B refactor
- Last reconciled: 2026-08-08 against `origin/main` at `9a18371a0`

## Current state

The design investigation is complete and the implementation plan has been rewritten around a
module-local strategy factory plus one vertical registration builder. No runtime code has changed.
The branch contains only the plan, this ledger, and the owning roadmap entry.

## Next Steps

Implement Phase 1 from the plan on this branch:

1. Reconcile the branch with current `origin/main` and confirm the worktree contains no unrelated paths.
2. Add characterization tests that pin human terms rendering and canonical terms serialization for all
   four deal types.
3. Add the Concert-local generic strategy factory and vertical registration builder with duplicate,
   exact-coverage, and lifetime tests.
4. Combine the terms renderer/serializer leaves behind `IDealTerms`, migrate only that family, and keep
   fingerprints byte-for-byte stable.
5. Run `dotnet build api/Concertable.slnx` and the affected Concert unit/integration tests through the
   `integration-debug` skill. Fix every failure, update this ledger and check off Phase 1 in the plan,
   then commit the verified phase.

Do not begin Phase 2 in the same turn; hand back after the Phase 1 commit and ledger checkpoint.

## Completed work

- Repository-wide investigation inventoried nine hand-written `DealType` maps plus the workflow
  builder/factory/registry surface.
- Design decisions locked: factory semantics, vertical registration, cohesive terms/party combinations,
  explicit `IDealAccessor` separation, module-local ownership, and .NET 11 union compatibility.
- Existing investigation plan revised rather than duplicated.

## Verification

- Read-only repository searches confirmed no production `DealType == ...` or `switch (dealType)` business
  branches in Deal/Concert.
- Confirmed `main` was aligned with `origin/main` before creating the plan branch.
- Documentation-only planning changes require no build or test run.

## Reviews

- Planning document self-reviewed for module ownership, suffix semantics, DI lifetime safety, exact
  coverage validation, accessor separation, union compatibility, and phase verification gates; no open
  planning findings remain.
- Implementation code review pending after the code phases complete.

## Decisions, discoveries, blockers, and deviations

- The Concert factory sits in Concert.Application/Infrastructure because it returns Concert-owned
  strategies. Deal gets a separate module-local equivalent for mapper/updater strategies.
- `IDealAccessor` remains in Concert and is not injected into the factory; callers pass `DealType`
  explicitly after resolving the deal.
- C# 15 union types are available in .NET 11 preview, but the syntax remains preview. Union migration is
  a compatible later change to workflow internals, not a prerequisite for this plan.
- The old plan's recommendation to expose a generic strategy map to consumers was superseded by named
  facades backed by a module-local factory.
- No blocker is active.

## Event log

### 2026-08-08 — design reconciled and plan rewritten

- Action: Audited the current Deal/Concert dispatch families, resolved naming and ownership decisions,
  verified official .NET 11/C# 15 union documentation, and rewrote the existing plan.
- Evidence: `plans/launch/DEAL_STRATEGY_REGISTRATION_PLAN.md`; repository searches recorded above.
- Outcome: The implementation has one decided architecture and five independently green phases.
- Follow-up: Implement Phase 1 only.

## Resume prompt

```
cd C:\Users\tommy\source\repos\Concertable
Read @plans/launch/DEAL_STRATEGY_REGISTRATION_PLAN.md and @plans/launch/DEAL_STRATEGY_REGISTRATION_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
