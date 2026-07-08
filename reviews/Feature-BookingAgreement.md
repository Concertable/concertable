# Code review — Feature/BookingAgreement

**Reviewed up to commit:** `e118a38225f041dcc07227f613d03a32076f9a49`  _(2026-07-08)_

> Range reviewed: `97b3c758..e118a382` (5 commits). Focus: Phase 3 (commit `e118a382`) —
> agreement PDF generation/storage, download + metadata endpoints, HATEOAS/AgreementId,
> background+lazy generation, FE download links. Phases 1–2 (`3d71bef1`, `009cd41b`) already
> shipped and were spot-checked where Phase 3 builds on them.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

### Correctness (Lens A)

- [x] **BUG1 — LOW — correctness** — `BookingAgreementPdfService.cs` — **fixed** in commit after review.
  Original issue: the PDF blob *name* was minted lazily by whichever generator ran first, so a
  background-at-Accept render racing a lazy download (or two concurrent party downloads) could mint two
  competing names and orphan a blob — `AttachPdf`'s `if (PdfBlobName is null)` guard didn't enforce
  single-write across scopes (no concurrency token). Outcome was benign (correct bytes served) but the
  "write-once" claim was hollow. **Fix:** the blob name is now assigned once, transactionally, at Accept
  (`BookingAgreementBuilder` → `BookingAgreementEntity.AssignPdfBlobName`), before any bytes exist;
  generation only ever *fills that fixed location* (idempotent overwrite), never mints a name and never
  writes the DB. This removes the race entirely and also drops the GET-path DB write. No migration
  (column already existed); name keeps its random GUID (still unguessable — matters because the
  `BlobController` passthrough is unauthenticated). Residual storage-level byte-immutability
  (`overwrite: true`) remains logged in `TECH_DEBT.md`.

## Notes (not findings — verified OK)

- **Both-party auth** — `GetAgreement`/`GetAgreementPdf` carry no `[HasPermission]` and lean on the
  two-party tenant query filter, identical to `GetById`. A non-party gets 404 (not 403), which matches
  the deliberate, documented `TenantScopingTests` stance ("the filter answers 404, not 403, so third
  parties can't even probe which ids exist"). The plan gate's "403" wording is superseded by that
  convention. Integration tests assert 404 for a stranger and 200 for both parties. ✔
- **DbContext sharing** — the lazy-generate path relies on the request-scoped `ConcertDbContext` being
  shared between `BookingAgreementService`'s repo and `BookingAgreementPdfService`'s repo, so the
  tracked-entity `AttachPdf` mutation persists via the PDF service's `SaveChangesAsync`. Scoped
  lifetimes make this correct. The background path resolves its own fresh scope (via
  `IBackgroundTaskRunner.RunAsync<T>`) and reads unfiltered by design (no tenant off-request). ✔
- **Transaction ordering** — background PDF generation is *enqueued* inside the accept transition but
  runs post-commit on the queue drainer; if the transition rolls back, `GenerateForBookingAsync` finds
  no agreement and no-ops. Mirrors the existing `RejectAllExceptAsync` enqueue. ✔
- **Microservice isolation (Lens B)** — all changes are within B2B. QuestPDF and
  `Concertable.Shared.Pdf.Application` / `.Blob.Application` are shared published packages (host wires
  the `.Infrastructure` impls); no data-service→data-service reference. FE `useDownloadAgreement` in
  `b2b/shared` imports only `@concertable/shared`. ✔
- **Module boundaries (Lens C)** — agreement service/repo/PDF/document all live in the Concert module;
  persistence goes through `IBookingAgreementRepository.SaveChangesAsync`, not `IUnitOfWork`. ✔
- **Seeding (Lens D)** — no seeder changes; no direct writes to reaction-owned tables. ✔
- **C# conventions (Lens E)** — explicit ctors + `private readonly` (no `_`), `is not null`,
  source-gen logging untouched, no additive migration (no model change — `PdfBlobName` column shipped
  in Phase 1). `BookingAgreementDto` returned verbatim from the metadata endpoint per the
  "Dto verbatim by default" rule. ✔
- **FE tiering** — the DOM object-URL download lives in `b2b/shared` (web-only), not the
  cross-platform `@concertable/shared` core; the download link is gated on the HATEOAS `agreement`
  link in both manager SPAs and is unreachable from the customer app. ✔
