# `IPdfService` → `IPdfRenderer` + fold the render lock into the shared renderer

> Boundary-crossing change to the **published** `Concertable.Shared.Pdf` package. B2B and Customer
> consume it from the feed, so the public type can't be renamed and its consumers migrated in one PR
> (see `plans/CLAUDE.md` → "Boundary-blocked refactors"). Expand/contract across merges, on the
> `Refactor/PdfRenderer` worktree/branch (off `master`).

## Why (two changes, one package, one migration)

1. **Rename.** `Concertable.Shared.Pdf.Application.IPdfService` has one member — `byte[] Render(IDocument)`.
   "Service" is generic; the type *renders* a QuestPDF document to bytes. `IPdfRenderer` names what it
   is (verb = `Render`; not `Writer`, which implies a stream/target — it returns `byte[]`).
2. **Render lock.** QuestPDF's `GeneratePdf()` is **not thread-safe** — concurrent renders race on
   shared font-subset state and emit PDFs with an unusable glyph map (text renders but can't be
   extracted/searched). Today B2B's `ContractPdfService` **and** `InvoicePdfService` each carry an
   identical `SemaphoreSlim` guard (copy-paste), and **Customer's `TicketPdfService` has no guard at
   all** — the footgun a per-consumer lock always becomes. The fix: serialize inside the shared
   `PdfRenderer.Render`, so every consumer is protected without opting in, and the per-consumer guards
   are deleted. (Logged as tech debt in `api/Concertable.B2B/TECH_DEBT.md`.)

Both touch the same published type in the same files, so they ride **one** expand/contract cadence —
one package republish, consumers migrate once.

## Why it's multi-merge, not one PR

`Concertable.Shared.Pdf.Application` is consumed as a **published NuGet package** (`PackageReference`
pinned by `<ConcertablePlatformVersion>`), not a project reference — even though the source sits in the
same `.slnx`. Consumers see the new public shape only after the package **republishes on merge to
master**. So renaming the type *and* switching its consumers in one PR won't build — the consumers
still compile against the published package that exports `IPdfService`.

**Consumers (injection sites to migrate in Phase 2):**
- B2B — `Concert.Infrastructure/Services/ContractPdfService.cs` (injects `IPdfService`, has its own `renderLock`)
- B2B — `Concert.Infrastructure/Services/InvoicePdfService.cs` (injects `IPdfService`, has its own `renderLock`)
- Customer — `Ticket.Infrastructure/Pdf/TicketPdfService.cs` (injects `IPdfService`, **no lock** — relies on the shared guard once it lands)
- (`AddSharedPdf()` keeps its name — only the interface it registers changes.)

## Design decisions

- **Sync `lock` (`System.Threading.Lock`), not `SemaphoreSlim`.** The critical section is inside the
  **synchronous** `Render` — `SemaphoreSlim` only earned its place in the old consumers because *they*
  `await`ed it inside async methods. A synchronous count-1 gate is a `lock`; `System.Threading.Lock`
  (.NET 9+, we're on net10.0) is its modern form and gives try/finally release for free. Keeping the
  interface synchronous (`byte[] Render(IDocument)`) is deliberate — an async `RenderAsync` would
  change the published shape and all three consumers for no real throughput gain (renders serialize by
  design; waiters queue either way).
- **No `[Obsolete]` on the `IPdfService` alias.** We own both consumers and migrate them deliberately
  in Phase 2, and the repo's rename **grep gate** catches any straggler — so the attribute buys only a
  compile-warning we don't need, at the cost of `#pragma` noise at every internal reference. Left it a
  plain alias.

## Phases

### Phase 1 — Expand (shared package) — ✅ implemented (pending merge/publish)
- Added `IPdfRenderer { byte[] Render(IDocument); }` in `Concertable.Shared.Pdf.Application`.
- `IPdfService : IPdfRenderer` kept as a **plain alias** (no `[Obsolete]`) so existing consumers still resolve.
- Renamed concrete `PdfService` → `PdfRenderer` (`internal sealed`), moved the QuestPDF thread-safety
  lock inside its `Render` (`System.Threading.Lock`). It implements `IPdfRenderer, IPdfService` during
  the transition so one scoped instance serves both.
- `AddSharedPdf()` registers the concrete once and maps both interfaces to it.
- **Gate:** shared package builds (0/0) ✅ · full solution build green · merge → publishes a new feed version.

### Phase 2 — Migrate (consumers) — ✅ done (`Refactor/PdfRendererPhase2`, pending merge)
- ✅ Version bump was a **no-op** — the Phase-1 `platform-sync` PR already pinned every service at
  `0.1.0-alpha.0.606` (the version carrying `IPdfRenderer`), so no `<ConcertablePlatformVersion>` change was needed.
- ✅ Switched `ContractPdfService`, `InvoicePdfService`, `TicketPdfService` `IPdfService` → `IPdfRenderer`.
- ✅ Deleted the redundant `renderLock` `SemaphoreSlim` guards from `ContractPdfService` and
  `InvoicePdfService` (shared `PdfRenderer.Render` serializes via `System.Threading.Lock`). `TicketPdfService`
  gained protection for free.
- ✅ Removed the "PDF render thread-safety" entry from `api/Concertable.B2B/TECH_DEBT.md`.
- ✅ **Folded in (B2B-internal, not the shared package):** extracted the duplicated render→check-blob→upload→serve
  orchestration from `ContractPdfService`/`InvoicePdfService` into `IPdfBlobCache`/`PdfBlobCache` in
  `Concertable.B2B.Concert.Infrastructure.Pdf` — resolves the LOW `TECH_DEBT` dedup note. Homed in B2B (not
  `Concertable.Shared.Pdf`) because only B2B blob-caches PDFs (Customer renders tickets on demand) — "shared is the
  intersection" — which is also why it wasn't boundary-blocked.
- ✅ **Grep-gate consumer cleanup:** `InvoiceService`/`ContractService` fields `pdfService` →
  `invoicePdfService`/`contractPdfService`; `LEGAL_REQUIREMENTS.md` prose `IPdfService` → `IPdfRenderer`.
  Consumer side is now grep-clean; the only remaining `\bPdfService\b` hits are the 4 shared shims Phase 3 deletes.
- ✅ **Gate:** build 0 errors · B2B Concert 128/128 · Customer Ticket 15/15 (+ Customer Concert 2/2) green.
  (Customer integration tests need a short-drive `subst` to run in this deep worktree — native `Microsoft.Data.SqlClient.SNI.dll`
  path overflows `MAX_PATH`; not a code issue.)

### Phase 3 — Contract (shared package)
- Delete `IPdfService`, its `AddSharedPdf` bridge registration, and `IPdfService` from `PdfRenderer`'s
  base list (→ `: IPdfRenderer`).
- Merge → publishes; nothing references `IPdfService` anymore.
- `git rm` this plan in the Phase 3 commit.

## Definition of done — the grep gate
Rename is done only when `grep -rniE "IPdfService|\bPdfService\b"` over the repo returns **zero**
(type names, identifiers of every case, DI, comments, docs). No allowlist survivors expected.

## Not in scope
- The module-local `ContractPdfService` / `InvoicePdfService` / `TicketPdfService` names are fine —
  they're the consumers' own PDF services, not the shared renderer. Leave them.
