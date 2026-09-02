# Code review — Refactor/RepositoryFinderSpecifications

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]` findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `16a13559c9f3`  `(2026-09-02)`
**Judgment:** `approved`

## Review pass — 2026-09-02 — full

**Candidate base:** `b91ed63bf4c14484805a99db30b074fe0a90a646`
**Candidate head:** `16a13559c9f3d13f1acf3bec009052c999153d64`
**Candidate branch:** `Refactor/RepositoryFinderSpecifications`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:7f07201c43e4fe0808e29419d2826a1919e2b8716d27c6333b5a7d36e6077c58` `(34 paths)`
**Work-order path:** `reviews/Refactor-RepositoryFinderSpecifications.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

Deletes 17 graph-named repository finders and moves the include shape to a caller-supplied
specification. Routed skills re-read against the frozen diff: `persistence`, `csharp-naming`,
`csharp-style`, `module-structure`, `multitenancy`, `domain-events`, `unit-testing`,
`integration-testing`, `dependency-injection`, `result-carriers`.

Verification: `local-platform.ps1 build api/Concertable.slnx` 0 errors; Concert unit tier 234 passed.
The Concert integration tier could not run in this worktree — Windows MAX_PATH (0x800700CE) defeats
the SqlClient native DLL under the path — so that tier is covered by CI rather than locally.

### Findings

- [x] **1 — The cancellation token in scope was dropped at three call sites.** `persistence` requires
  every awaited call that accepts a `CancellationToken` to receive it. The deleted finders
  (`GetByIdWithArtistAndVenueAsync`, `GetByIdWithVenueAsync`, `GetByIdWithBookingAsync`) declared no
  token, so the handlers physically could not pass one; the replacement `GetByIdAsync` overload does
  accept one, and the first version of this change still omitted it. Fixed in
  `ConcertChangedDomainEventHandler`, `ConcertPostedDomainEventHandler` and
  `ConcertCancelledDomainEventHandler`, which each hold `ct` from `HandleAsync`. The remaining call
  sites are in methods that genuinely take no token, so nothing is available to pass there.

- [x] **2 — Merging two by-concert finders changed the failure mode from First to Single.**
  `BookingService.GetSettlementByConcertIdAsync` previously called `GetWithApplicationByConcertIdAsync`,
  which ended `FirstOrDefaultAsync`; it now calls `GetByConcertIdAsync`, which ends
  `SingleOrDefaultAsync`. With two bookings for one concert the old path silently picked one and the new
  path throws. Disposition: **no change needed.** `ConcertEntity` holds `BookingId` as a one-to-one
  relationship, so a second booking is a data defect rather than a supported state, and surfacing it is
  the better failure. Recorded because it is a real behavioural change rather than a pure refactor.

### Notes

- Four factories survive because their graph is built at more than one site (6, 5, 2 and 2 call sites);
  the other nine specifications are built inline at their single call site and `BookingSpecification.CreateDealId`
  was deleted for having no caller. That rule is the user's, recorded here so a later pass does not
  "restore consistency" by re-adding single-use factories.
- Every factory is expression-bodied. A `Specification` instance accumulates its includes, so a static
  field would share one mutable graph across callers — the same hazard `reviews/Refactor-data-access-specification-query-boundary.md`
  recorded as finding 2.
- An inlined call site needs `Concertable.Kernel.Specifications` in scope, or the fluent `Include`/`Select`
  bind to the protected `Specification.Include` and to LINQ's `Select` instead.

**Reviewer independence:** this pass was performed by the session that authored the change; independent
lens subagents were not dispatched. It is weaker than an isolated multi-lens pass.
