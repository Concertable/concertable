# Code review — Feature/launch_tenant-verification

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `ca7b8ba0d2516d8efa0a4aa15d456896ed44d54b`  _(2026-08-25)_
**Security-reviewed up to commit:** `84e4d61b0d47533645e5813d39f39413a2a4073e`  _(2026-08-25)_

> Range reviewed: `ac7ff7f17..c72c79161` (1 commit; findings fixed across `84e4d61b0`, `2f2e7218d`,
> `cec78027d`, and `81b097381`; `ca7b8ba0d` is a no-conflict merge of `origin/main`, no reviewed file
> touched). Diff touches `VerificationController.cs`, matching this repo's `Controller[A-Za-z]*\.cs$`
> security-sensitive pattern (`.agents/merge-gate.json`).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 — MEDIUM — native (correctness)** — `api/Concertable.B2B/src/Modules/Tenant/Concertable.B2B.Tenant.Infrastructure/Services/VerificationService.cs`
  `SubmitAsync` used `repository.InsertAsync` directly against `Verifications.TenantId`, which is
  unique-indexed (`TenantVerificationEntityConfiguration.cs:17`). Two concurrent first submissions for the
  same tenant both pass the `existing is null` pre-check, and the second `InsertAsync` would throw an
  unhandled `DbUpdateException` (500) instead of the modeled `NotEligible` conflict — the same race the
  codebase already guards against in `VenueService.CreateAsync`/`ArtistService.CreateAsync` via
  `repository.TryInsertAsync`. **Fixed:** swapped to `TryInsertAsync`, mapping a lost race to
  `SubmitVerificationError.NotEligible` (re-reading the winner's actual status rather than assuming
  `Pending`).
- [wontfix] **NAT2 — MEDIUM — native (error handling)** — `api/Concertable.B2B/src/Modules/Tenant/Concertable.B2B.Tenant.Infrastructure/Services/VerificationService.cs`
  Evidence files upload to blob storage before the verification entity persists; a subsequent persistence
  failure leaves the blob orphaned with no compensating delete. Not fixed: this exactly mirrors the
  established codebase precedent in `VenueService.CreateAsync`, which uploads banner/avatar images via
  `imageService.UploadAsync` *before* `repository.TryInsertAsync` with the same no-compensation behavior.
  Diverging here would make this endpoint inconsistent with the accepted repo-wide tradeoff rather than
  fixing a defect specific to this diff.
- [x] **SEC1 — security (consistency, not a confirmed exploit)** — `api/Concertable.B2B/src/Modules/Tenant/Concertable.B2B.Tenant.Application/Validators/VerificationValidators.cs`
  `VerificationDocumentFileValidator` allowlisted by the client-supplied `ContentType` header only, with no
  byte-level check — a real regression against this codebase's own stronger precedent
  (`Concertable.Shared.Imaging.Application.ImageValidator`, used by the venue/artist banner/avatar upload
  paths, decodes the image and rejects on failure). An independent false-positive-filter pass scored the
  *exploitability* at 2/10 — no consumption/serving endpoint exists yet for this evidence, so a lying
  `Content-Type` has no proven path to stored XSS or disclosure from this diff alone — but confirmed the
  gap itself is real and a legitimate consistency/hardening issue worth fixing regardless of exploit
  status. **Fixed:** the validator now checks magic bytes against the declared content type (`%PDF-` for
  PDF, `FF D8 FF` for JPEG, the PNG signature for PNG) rather than trusting the header alone. Kept
  self-contained (no new `SixLabors.ImageSharp` dependency + cross-service package-version pin) rather than
  reusing `ImageValidator`'s full image-decode approach, consistent with plan §1.3's "add a dedicated
  check rather than reusing the image-specific service."

## User review — caught after the above was reported clean

Lens E (language/framework conventions) was under-applied in the first pass: the invoked skills
(`result-carriers`, `csharp-style`/`NAMING.md`, `persistence`) were read at write time but not re-checked
against the finished diff during the review itself. All five findings below are fixed.

- [x] **CV1 — convention (result-carriers)** — `VerificationService.cs` (`UploadEvidenceAsync`)
  Returned `Task<List<VerificationDocumentEntity>>`. `CARRIERS.md`: "Zero or more values | `IReadOnlyList<T>`".
  **Fixed:** return type is `IReadOnlyList<VerificationDocumentEntity>`.
- [x] **CV2 — convention (csharp-style / csharp-naming)** — `VerificationMappers.cs`
  Used legacy `public static X ToDto(this Y y)` methods. `STYLE.md`: "New extension members go in
  `extension()` blocks... Do not add a new legacy `public static … (this X x)` method." `NAMING.md` shows
  the exact `XMappers`-with-`extension()` shape. **Fixed:** converted to `extension(Y y) { public X ToDto()
  => ...; }` blocks.
- [x] **CV3 — convention (persistence)** — `VerificationRepository.cs`
  Took `TenantDbContext` and re-stored it in a private field purely to call `.Verifications.Include(...)`.
  `PERSISTENCE.md`: "Keep the concrete context in a `private readonly` field only when the repository
  genuinely needs typed `DbSet`s..." — it didn't; `Context.Query<TEntity>()` (inherited) does the same
  query. **Fixed:** primary-constructor repository, no field, queries via `Context.Query<TenantVerificationEntity>()`.
- [x] **CV4 — design (DDD)** — `VerificationService.cs` (blob-name construction)
  The evidence blob-naming convention (documented in plan §1.3) was built ad hoc as an interpolated string
  in the infrastructure service, then threaded through two separate calls. First fix (a static
  `BuildBlobName` helper) still didn't use the entity — it just moved the same string-building elsewhere.
  **Fixed properly:** a new `VerificationDocumentEntity.Create(tenantId, documentType, fileExtension,
  uploadedAt)` overload derives and owns the name internally; the service constructs the entity first and
  reads `document.BlobName` back off it for the blob upload — the entity is genuinely used, not just
  consulted for a string. Covered by two domain unit tests.
- [x] **CV5 — design (reusability)** — `IVerificationService.SubmitAsync`
  Took `SubmitVerificationRequest` directly, which carries `IFormFileCollection` — an ASP.NET Core type,
  making the service uncallable from anything that isn't already inside an HTTP request. **Fixed:**
  service now takes `IReadOnlyList<EvidenceUpload>` (a plain `Stream` + extension + document type); the
  controller maps the MVC-bound request via a new `SubmitVerificationRequest.ToEvidenceUploads()` extension.
  Also proposed as a standards addition — see Decisions below.
- [x] **CV6 — convention (result-carriers)** — `VerificationService.cs` (`GetStatusAsync`)
  Used the explicit `.ToOption().Map(v => v.ToDto())` chain (copied from `TenantService.cs` precedent).
  `CARRIERS.md` prefers target typing over `.ToOption()` where a target-typed site exists. **Fixed:**
  `(await repository.GetByTenantIdAsync(tenantId, ct))?.ToDto()` — the nullable result and the `null`/
  `VerificationStatusDto` outcomes convert implicitly at the `Option<VerificationStatusDto>`-typed return.
- [x] **CV7 — convention (csharp-naming)** — `IVerificationService.GetStatusAsync` (was `GetOwnAsync`)
  `Own` restated the active-tenant scope every B2B service method already runs under by default
  (`Concertable.B2B/AGENTS.md`: "do not add `ActiveTenant` to ordinary method names"), and this doc was
  shown to the session multiple times while the file was being written. **Fixed:** renamed to
  `GetStatusAsync`, matching `TenantService.GetDetailsAsync`'s precedent — name for what's returned,
  not the default scope.
