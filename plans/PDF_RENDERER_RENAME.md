# Rename `IPdfService` → `IPdfRenderer` (shared package, expand/contract)

> Cosmetic-but-boundary-crossing rename. `IPdfService` is a **published** type in
> `Concertable.Shared.Pdf`; both B2B and Customer consume it from the feed, so it can't be
> renamed in one PR (see `plans/CLAUDE.md` → "Boundary-blocked refactors"). Do it after
> `Feature/BookingAgreement` (#90) lands, on a `Refactor/PdfRenderer` branch.

## Why

`Concertable.Shared.Pdf.Application.IPdfService` has one member — `byte[] Render(IDocument)`.
"Service" is generic; the type *renders* a QuestPDF document to bytes. `IPdfRenderer` names what
it is (verb = `Render`; not `Writer`, which implies a stream/target — it returns `byte[]`).

## Why it's multi-merge, not one PR

`Concertable.Shared.Pdf.*` is consumed as a **published NuGet package** (pinned by
`<ConcertablePlatformVersion>`), not by project reference. Consumers see a new public shape only
after the package **republishes on merge to master**. So renaming the shared type *and* its
consumers in one PR won't build — the consumers still compile against the published package that
exports `IPdfService`. Only the public interface is the breaking surface; the concrete
`PdfService` is `internal sealed` and renames freely alongside it.

**Consumers (injection sites to migrate):**
- B2B — `Concert.Infrastructure/Services/BookingAgreementPdfService.cs`
- Customer — `Ticket.Infrastructure/Pdf/TicketPdfService.cs`
- (`AddSharedPdf()` keeps its name — only the interface it registers changes.)

## Phases

### Phase 1 — Expand (shared package)
- In `Concertable.Shared.Pdf.Application`, add `IPdfRenderer` with the same `byte[] Render(IDocument)`.
- Make `IPdfService : IPdfRenderer` **and** mark `IPdfService` `[Obsolete("Use IPdfRenderer")]`
  (keeps existing consumers compiling; both resolve the same registration).
- Rename the concrete `internal sealed PdfService` → `PdfRenderer`; register it as
  `AddScoped<IPdfRenderer, PdfRenderer>()` and keep `AddScoped<IPdfService>(sp => sp.GetRequiredService<IPdfRenderer>())`
  so both interfaces resolve during the transition.
- **Gate:** shared package builds; merge → publishes a new feed version.

### Phase 2 — Migrate (consumers)
- Bump B2B + Customer `<ConcertablePlatformVersion>` to the version Phase 1 published (arrives via
  the automated `platform-sync` PR — can piggyback on it, or bump explicitly).
- Switch `BookingAgreementPdfService` and `TicketPdfService` constructor deps from `IPdfService`
  → `IPdfRenderer`.
- **Gate:** solution build green · Concert + Ticket integration green · four web builds (no FE
  surface touched, but the boundary gate is cheap insurance).

### Phase 3 — Contract (shared package)
- Delete `IPdfService` and its bridging registration from `Concertable.Shared.Pdf`.
- Merge → publishes; nothing references `IPdfService` anymore.
- `git rm` this plan in the Phase 3 commit.

## Not in scope

- The module-local `BookingAgreementPdfService` / `TicketPdfService` names are fine — they're the
  consumers' own PDF services, not the shared renderer. Leave them.
