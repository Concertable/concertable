# Code review — Refactor/RepositoryFinderSpecifications

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]` findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `31ff9621a637`  `(2026-09-02)`
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

## Review pass — 2026-09-02 — incremental

**Candidate base:** `16a13559c9f3d13f1acf3bec009052c999153d64`
**Candidate head:** `abec4aa069efd88c5425665a94552035ab6cbe75`
**Candidate branch:** `Refactor/RepositoryFinderSpecifications`
**Candidate scope:** `branch-authored delta vs the prior watermark`
**Candidate path-set:** `sha256:e779b14cd108b9996d9177835e47618a6143c43e72b4afe766e57c58fa98fe04` `(5 paths)`
**Work-order path:** `reviews/Refactor-RepositoryFinderSpecifications.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

Replaces the include-based artist/venue specification with a projection. `ArtistAndVenue` is a
positional `record struct` in `Application/Projections`, so `GetByIdAsync` returns
`ArtistAndVenue?` through the value-type `Select` overload and `ContractIssuer` deconstructs it
directly. That removes the local whose name described nothing, restores the tuple shape the
deleted finder returned, and narrows the query from the whole application graph to the two
columns actually read. `CreateWithArtistAndVenue` lost both callers and is deleted.

The type is named rather than a `ValueTuple` because EF translates a constructor projection and
does not translate a tuple one — `GetTenantPairByIdAsync` already works around exactly that by
projecting an anonymous type and rebuilding its tuple in memory.

### Findings

No findings.

### Notes

- The translation is now proven rather than assumed: the Concert integration tier was run from a
  short-path checkout (176 passed, 0 failed, 8m27s), which also closes the gap the previous pass
  recorded, where Windows MAX_PATH prevented that tier from running in this worktree.

## Review pass — 2026-09-02 — incremental

**Candidate base:** `abec4aa069efd88c5425665a94552035ab6cbe75`
**Candidate head:** `7e1f160253a01b5a991931d1378e674659d87e6d`
**Candidate branch:** `Refactor/RepositoryFinderSpecifications`
**Candidate scope:** `base merge to platform 0.1.0-alpha.0.1310 plus one authored fix`
**Candidate path-set:** `sha256:3a373a8193efb52b30eebf019a9513945f475c0d3d729138e0e38937df35afb8` `(12 paths)`
**Work-order path:** `reviews/Refactor-RepositoryFinderSpecifications.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

Merges `origin/main` (7 commits, clean, pin `0.1.0-alpha.0.1310` from sync #929) and fixes the
one defect that merge surfaced.

Verification on the merged head: build 0 errors, Concert unit tier 234 passed.

### Findings

- [x] **3 — The projection changed which overload `ContractIssuer` calls, and its mock still
  stubbed the old one.** Moving from the include specification to `ArtistAndVenue?` moved the call
  from `GetByIdAsync(id, ISpecification<T>, ct)` to the `TResult` overload. `ContractIssuerTests`
  still set up the entity overload, so Moq returned `default` for the unstubbed projected call and
  `OrNotFound` threw `NotFoundException`. Fixed by stubbing
  `ISpecification<ApplicationEntity, ArtistAndVenue?>` and returning an `ArtistAndVenue`; the
  entity-building helper that existed only to feed the old stub is deleted. Caught by the unit tier
  locally and independently by CI on the pushed head — the previous pass had run the integration
  tier after the projection but not the unit tier, which is how it reached CI at all.

## Review pass — 2026-09-02 — incremental

**Candidate base:** `7e1f160253a0`
**Candidate head:** `9bfce704433bf160c8ec151fedb82a79757a1443`
**Candidate branch:** `Refactor/RepositoryFinderSpecifications`
**Candidate scope:** `documentation only — one tech-debt entry`
**Candidate path-set:** `sha256:198db902afcd301ecc438a1b4727217158d7bac7e6215cb3af8501217615f85e` `(2 paths)`
**Work-order path:** `reviews/Refactor-RepositoryFinderSpecifications.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

Adds one entry to the Concert module's `TECH_DEBT.md` recording that `ContractIssuer.IssueAsync`
ends both lookups in `OrNotFound` where `result-carriers` wants a `Result`. The behaviour is
pre-existing — this branch swapped the finder underneath the first `OrNotFound` and changed nothing
about how it fails — so it is logged rather than fixed here, per `docs-and-debt`: a shortcut that is
the right call is logged with its reasoning, never left silent. The entry sits in the Concert
module's own file because that module owns the problem, and carries the owner decision and a
resolves-when condition like its siblings.

No executable change, so the prior verification stands.

### Findings

No findings.

## Review pass — 2026-09-02 — incremental

**Candidate base:** `9bfce704433b`
**Candidate head:** `31ff9621a637e00d3f61d97fd063a0c5c5d4c81d`
**Candidate branch:** `Refactor/RepositoryFinderSpecifications`
**Candidate scope:** `base merge resolving one documentation conflict`
**Candidate path-set:** `sha256:973d2f14be6fb0fb560535dc747321203386bec3a751e021be14103c9713ecc0` `(16 paths)`
**Work-order path:** `reviews/Refactor-RepositoryFinderSpecifications.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

Merges `origin/main` after PR #867 landed, which cleared the branch's DIRTY state. One conflict, in
`Concert/TECH_DEBT.md`: #867 deleted the resolved `Genres allows duplicate tags` entry while this
branch appended the `IssueAsync` entry to the same tail. Resolved by keeping both intentions — the
deletion stands, since `docs-and-debt` requires a resolved entry to be removed rather than archived,
and the new entry remains.

#867 also changed `ConcertEntity.Genres` and `OpportunityEntity.Genres` from `List<Genre>` to
`EfSet<Genre>`. This branch's specifications include `Artist.Genres`, which is a different member:
`ArtistReadModel.Genres` is `ICollection<ArtistReadModelGenre>` configured with `HasMany` against
its own table, so it is a real navigation and the include is unaffected by the primitive-collection
change. Checked rather than assumed, because a specification that includes a primitive collection
would be silently wrong.

Verification on the merged head: build 0 errors, Concert unit tier 237 passed (base added three).

### Findings

No findings.
