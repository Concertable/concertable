# Code review — Refactor/PaginationMapConsumers

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it.

**Reviewed up to commit:** `b438c86c76c3e09910c4b8401749ff80d07ddd0d`  _(2026-08-16)_

> Range reviewed: `origin/main..HEAD`.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **DEAD1 — LOW — dead code** — `Concertable.Payment.Application/Interfaces/ITransactionMapper.cs:7`
  Converting `TransactionService` to `Map(transactionMapper.ToDto)` bypassed the plural default
  interface method `ToDtos(IEnumerable<TransactionEntity>)`, leaving it with **zero callers**. A cut-over
  that leaves the thing it replaced behind is half a cut-over. **Fixed:** removed. Payment 272/272 still
  pass.
  Checked the same risk in Concert: `ToResponses(IEnumerable<OpportunityDto>)` is **still used** by
  `OpportunityController:58` and `:68`, so it correctly stays.

### Checked and clean

- **The Search deletions are behaviour-preserving.** `IPagination<out T>` is covariant and
  `ArtistHeader`/`VenueHeader`/`ConcertHeader` implement `IHeader`, so returning the repository's page
  directly satisfies `Task<IPagination<IHeader>>`. Nothing depended on receiving a distinct instance —
  the pages are read-only projections, and the sibling `GetByAmountAsync` on each service already
  returned the repository result unwrapped.
- **Each `Map` conversion preserves the payload and the metadata**, because `Map` copies
  `TotalCount`/`PageNumber`/`PageSize` and re-derives `TotalPages` exactly as the hand-rolled
  constructions did — pinned by the `Contracts` unit test added with `Map`.
- **`OpportunityMapper.ToDtosAsync` is correctly excluded**: its selector is `await`-ing, and `Map`
  takes `Func<TSource, TDestination>`. The in-place comment says exactly that, so the next reader has
  the reason rather than an apparent oversight.
- **`Select` is safe to delete**: no caller remains in-repo, and it is legal now only because `Map`
  ships in the pin (`0.1.0-alpha.0.1031`) — removing it *with* `Map` is what broke the build first time.
- **`MessageService`'s helper widening** from `Pagination<MessageDto>` to `IPagination<MessageDto>` has
  no caller needing the concrete type; the only consumer is `GetInboxAsync`, which already returns the
  interface.
- **Docs match the code:** the resolved `TECH_DEBT` entry is deleted rather than archived, and
  `CODE_CONVENTIONS` documents `Map` plus the two genuine exceptions — covariant widening and async
  projection — which are precisely the cases a blanket find-replace gets wrong.
