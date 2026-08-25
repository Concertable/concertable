# Code review — Feature/launch_tenant-verification

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `c72c791610d3e9e5b865d875f72c85fc3bf1a49a`  _(2026-08-25)_
**Security-reviewed up to commit:** `c72c791610d3e9e5b865d875f72c85fc3bf1a49a`  _(2026-08-25)_

> Range reviewed: `ac7ff7f17..c72c79161` (1 commit). Diff touches `VerificationController.cs`, matching
> this repo's `Controller[A-Za-z]*\.cs$` security-sensitive pattern (`.agents/merge-gate.json`).
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
