# Lifecycle reporting read projections progress

- Plan: `plans/launch/LIFECYCLE_READ_PROJECTIONS_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/lifecycle-read-projections`
- Parent workstream: `launch/deal-lifecycle-ownership`
- Implementation worktree: not created
- Implementation branch: not created
- Implementation PR: not created
- Last reconciled: 2026-08-31 from the lifecycle branch's Dashboard, Application, Opportunity,
  Booking, and Concert dependency and event paths

## Current state

Investigation is complete and implementation is deferred. The current Venue dashboard serialization
is the safe production behavior. The long-term target is a selective Application-owned Opportunity
availability projection, not blanket denormalization and not a Dashboard-wide read database.

The code already demonstrates the intended pattern through Application's Concert availability
projection and Concert's Artist/Venue read models. Opportunity currently publishes no creation or
schedule-change integration event, so durable event production, generated-ID atomicity, ordering,
rebuild, and seeding are real prerequisites rather than incidental implementation details.

## Next Steps

Do not begin implementation on PR #633. After `launch/deal-lifecycle-ownership` is terminal on `main`,
open a fresh `Refactor/lifecycle-read-projections` worktree from current `origin/main`, reconcile this
plan against the shipped dependency graph, and execute Phase 0. Stop after the consistency-budget gate
if dashboard eventual consistency is not acceptable; otherwise proceed with the narrow Opportunity
availability projection before considering any additional read model.

## Decisions, discoveries, blockers, and deviations

- **Decision:** selective consumer-owned projections are the preferred long-term design for reporting
  fan-in; authoritative command-support reads remain synchronous.
- **Decision:** Application owns the first projection because it owns the pending-application metric.
  Dashboard remains a composer and does not gain persistence in this slice.
- **Discovery:** the EF concurrency failure is a diamond dependency: Dashboard calls Opportunity both
  directly and transitively through Application inside one request scope.
- **Discovery:** the lifecycle graph is one-way for command authority, not for all information. Existing
  integration events already feed later lifecycle facts into upstream-owned availability state and
  read projections without transferring aggregate authority.
- **Blocker:** Opportunity has no availability event contract today. Event atomicity for its
  database-generated integer identity must be solved before a projection consumer is introduced.
