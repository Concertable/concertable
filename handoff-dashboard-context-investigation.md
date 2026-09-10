Investigate the Dashboard CI failure in this worktree and explain its root cause.

`VenueDashboardService.GetAsync` starts several operations with `Task.WhenAll`, including `applicationModule.GetVenuePendingCountAsync` and `opportunityModule.GetOpenCountAsync`. CI reports EF Core's "A second operation was started on this context instance before a previous operation completed" from `OpportunityReadRepository.GetUpcomingIdsAsync` while serving `/api/venue-dashboard/kpis`.

This has appeared during the modular-monolith refactor. Explain why it happens now: intuitively, splitting modules into more DbContexts should make shared-context concurrency less likely, not more likely. Determine the actual context lifetimes and dependency path involved, and report what changed or what architectural seam makes the shared context possible.
