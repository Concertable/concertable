# Extract QR generation into a shared package (`Concertable.Shared.QrCode`)

## Why

`Concertable.Customer.Ticket.Infrastructure/Services/QrCodeService.cs` (`IQrCodeService`) is two
unrelated jobs stapled together, and one of them couples a generic utility to the Ticket domain:

- **Generation** — `GenerateFromTicketId(Guid)` is a pure `content → PNG bytes` transform over
  QRCoder. Ticket-agnostic in its body (`id.ToString()`); only the *name* pretends otherwise. This is
  generic plumbing, exactly like `Concertable.Shared.Pdf`'s `IPdfRenderer` — it belongs in a shared
  package, not in a domain module.
- **Stored-blob read** — `GetByTicketIdAsync(Guid)` + the injected `ITicketRepository` is a passthrough
  to `ticketRepository.GetQrCodeByIdAsync(...)` that only adds `.OrNotFound(DisplayNames.QrCode)`. This
  is a *ticket* read that has no business living on a QR abstraction.

Fix: move generation into a new shared **published package** `Concertable.Shared.QrCode` as
`IQrCodeGenerator.Generate(string content) : byte[]`, and push the stored-blob read down onto the
ticket read path. Delete `IQrCodeService`/`QrCodeService`.

## Why it's multiple PRs (boundary-blocked — the recurring platform-sync trap)

Cross-service shared code here is consumed as a **published NuGet package pinned to
`$(ConcertablePlatformVersion)`**, never a ProjectReference (the carve — `api/ARCHITECTURE.md`;
`plans/CLAUDE.md` "Boundary-blocked refactors"). `Concertable.Shared.Pdf.Application` is a
`<PackageReference>` in `Customer.Ticket.Infrastructure`, not source next to it.

So the new package and its consumption **cannot** land in one PR: Customer can't restore
`Concertable.Shared.QrCode.*` at a version that isn't on the feed yet (NU1101). Publishing is gated
purely on `<IsPackable>true</IsPackable>` and happens on merge to `master` (`publish-packages.yml`),
after which `platform-sync` bumps `ConcertablePlatformVersion` everywhere. Hence: **publish first, then
consume.**

- **Phase 1 (PR 1)** — create + publish the package. Additive only, no consumer touched.
- **Phase 2 (PR 2)** — consume it and delete the old service. Only starts once PR 1's
  `chore/platform-sync-*` PR is **green/merged** (the new packages + bumped version are on the feed).

Both phases are behaviour-preserving (QR bytes are byte-for-byte identical) → **`[skip-e2e]` in a commit
message on each PR**; the gate is build + Ticket-module unit/integration.

---

## Phase 1 — create `Concertable.Shared.QrCode` (PR 1)

Branch: `Refactor/SharedQrCodeGenerator` (this worktree). Mirror `Concertable.Shared.Pdf` exactly.

1. **`Concertable.Shared.QrCode.Application`** (new, `IsPackable`):
   ```csharp
   namespace Concertable.Shared.QrCode.Application;

   public interface IQrCodeGenerator
   {
       byte[] Generate(string content);
   }
   ```
2. **`Concertable.Shared.QrCode.Infrastructure`** (new, `IsPackable`, `PackageReference QRCoder` +
   the `.Application` ProjectReference):
   ```csharp
   internal sealed class QrCodeGenerator : IQrCodeGenerator
   {
       private readonly QRCodeGenerator qrCodeGenerator;   // QRCoder.QRCodeGenerator — case differs, no clash

       public QrCodeGenerator(QRCodeGenerator qrCodeGenerator) => this.qrCodeGenerator = qrCodeGenerator;

       public byte[] Generate(string content)
       {
           QRCodeData data = qrCodeGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
           return new PngByteQRCode(data).GetGraphic(20);
       }
   }
   ```
   > Note: our `QrCodeGenerator` and QRCoder's `QRCodeGenerator` are distinct identifiers (C# is
   > case-sensitive: `Qr` vs `QR`) — no `using` alias needed. Ctor uses an explicit `private readonly`
   > field, not a primary ctor (captured state — `CODE_CONVENTIONS.md`).
3. **`ServiceCollectionExtensions.AddQrCode()`** in `.Infrastructure` (mirror `AddSharedPdf`, but drop
   the redundant `Shared` — the package is already `Concertable.Shared.QrCode`):
   ```csharp
   public static IServiceCollection AddQrCode(this IServiceCollection services)
   {
       services.AddSingleton<QRCoder.QRCodeGenerator>();
       services.AddSingleton<IQrCodeGenerator, QrCodeGenerator>();
       return services;
   }
   ```
   (Singleton — stateless; QRCoder's generator was already a singleton. No delegating registration:
   nothing resolves the concrete type, unlike `PdfRenderer`.)
4. Add a `QRCoder` `PackageVersion` to `api/Concertable.Shared/Directory.Packages.props` if it isn't
   already pinned there (it's currently pinned in Customer's props, v1.6.0).
5. Add both new projects to `api/Concertable.slnx`.

**Gate:** `dotnet build api/Concertable.slnx` = 0 errors. No behaviour change, no consumer, no tests to
add. Commit with `[skip-e2e]`.

**After merge — own the sync (whoever merges owns it, `CLAUDE.md`):** confirm `publish-packages`
published `Concertable.Shared.QrCode.Application` + `.Infrastructure`, and drive the
`chore/platform-sync-*` PR to green/merged. Do **not** start Phase 2 until it lands.

---

## Phase 2 — cut Customer over, delete the old service (PR 2)

New branch off the updated `master` (after Phase 1's sync merged), e.g. `Refactor/QrCodeGeneratorCutover`.
This is the mechanical consumer migration — **run it through the `package-cutover` skill** (global +
file-local usings, restore, grep gate, build→fix loop) rather than by hand.

1. **Customer `Directory.Packages.props`** — pin both new packages lockstep:
   ```xml
   <PackageVersion Include="Concertable.Shared.QrCode.Application" Version="$(ConcertablePlatformVersion)" />
   <PackageVersion Include="Concertable.Shared.QrCode.Infrastructure" Version="$(ConcertablePlatformVersion)" />
   ```
2. **`Concertable.Customer.Ticket.Infrastructure.csproj`** — add
   `<PackageReference Include="Concertable.Shared.QrCode.Application" />`; **remove** the direct
   `<PackageReference Include="QRCoder" />` (it's only needed inside the shared package now).
3. **`Concertable.Customer.Web.csproj`** — add
   `<PackageReference Include="Concertable.Shared.QrCode.Infrastructure" />` (host wiring, mirrors how
   it references `Concertable.Shared.Pdf.Infrastructure`).
4. **`Concertable.Customer.Web/Program.cs`** — add `services.AddQrCode();` next to
   `services.AddSharedPdf();` (line ~90).
5. **`Ticket/.../Extensions/ServiceCollectionExtensions.cs`** — delete the two QR registrations
   (`AddSingleton<QRCoder.QRCodeGenerator>()`, `AddScoped<IQrCodeService, QrCodeService>()`, ~lines 48-49).
6. **`TicketService.BuildTicket`** — inject `IQrCodeGenerator` (from `Concertable.Shared.QrCode.Application`)
   in place of `IQrCodeService`; call `qrCodeGenerator.Generate(ticketId.ToString())`.
7. **`TicketPdfService`** — drop `IQrCodeService`; inject `ITicketRepository`; read the stored blob
   directly:
   ```csharp
   byte[] qrCode = (await ticketRepository.GetQrCodeByIdAsync(ticketId)).OrNotFound(DisplayNames.QrCode);
   ```
   (Move the `using Concertable.Kernel.Exceptions;` + `DisplayNames` usings here from the deleted service.)
8. **Delete** `Application/Interfaces/IQrCodeService.cs` and `Infrastructure/Services/QrCodeService.cs`.
9. **`Concertable.Customer/TECH_DEBT.md`** (line ~89) — the QR-read reference "behind `QrCodeService`"
   updates to "on the ticket repository (`GetQrCodeByIdAsync`)".
10. **`git rm`** this plan file in the completing commit (Lifecycle 4).

**Gate:** `dotnet build api/Concertable.slnx` = 0 errors; Ticket-module unit + integration via the
`integration-debug` skill (any red → drive that skill to green, never just report). `[skip-e2e]`.

### Definition of done (grep gate — mechanical, `plans/CLAUDE.md`)
`grep -rniE "qrcodeservice|iqrcodeservice|generatefromticketid|getbyticketidasync" api app` returns
**zero**. No allowlist — every occurrence is real work. "Build's green, tests pass" is not the bar; the
grep is.

## Open decisions / notes
- Only Customer consumes QR today, so `AddQrCode()` is wired in Customer.Web only (B2B/Auth call
  `AddSharedPdf` but need no QR).
- If a later phase makes the two ticket-list reads exclude `QrCode` and fetch it lazily (the existing
  `TECH_DEBT.md:89` item), that's independent of this move — don't fold it in here.
