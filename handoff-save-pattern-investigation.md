Investigate the correct EF Core save-failure design in this Concertable worktree. Do not edit, commit, push, reset, or fix anything. Deliver a design recommendation only.

Read `C:\Users\TOMMYS~1\AppData\Local\Temp\concertable-save-rethrow-refactor-handoff.md`, then inspect the current DataAccess UoW, `DbContextExtensions`, and B2B Application / Booking / Concert and Payment callers and tests.

The user needs a coherent design for:

- `SaveChangesAsync` on the UoW;
- a generic no-argument `TrySaveChangesAsync(ct)`;
- a predicate overload: `TrySaveChangesAsync(exception => exception.IsDuplicateKey(), ct)`;
- a reusable DbContext extension for repositories that legitimately own a one-shot persistence race;
- per-module UoWs that wrap the generic UoW for their module DbContext. Do not recommend injecting `IUnitOfWork<TContext>` directly into module services.

Use official EF Core guidance and actual code behaviour to decide:

1. What should happen to EF tracking after a concurrency failure, duplicate-key failure, and other `DbUpdateException`?
2. Should normal `UnitOfWork.SaveChangesAsync` clear the entire tracker and rethrow? If yes, why; if no, what replaces it?
3. Should try-save call normal UoW save or a lower-level context helper?
4. For an expected duplicate, should it leave tracking alone, detach only `exception.Entries`, or clear all tracking? Explain how subsequent canonical re-reads work safely in each case.
5. How should no-argument and predicate overloads differ? Cancellation and unexpected errors must propagate.
6. Which layer owns the mechanics: DbContext extension, generic UoW, module-local UoW, repository, service/workflow?

Constraints:

- Services, workflows, and repositories must not directly clear, reload, or detach EF tracking.
- Do not hide unexpected database errors behind `false` or domain results.
- Preserve the user’s untracked `scratchpad/` and do not trust the current partial uncommitted refactor edits as a decided design.

Return one recommended API sketch, a behaviour table, exact ownership boundaries, a minimal test matrix, and any conflict with the original handoff direction. Do not modify files.
